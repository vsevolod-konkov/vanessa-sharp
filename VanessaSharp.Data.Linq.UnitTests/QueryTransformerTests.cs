using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="QueryTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryTransformerTests : QueryTransformerTestsBase
    {
        private Mock<IExpressionTransformMethods> _transformMethodsMock;
        private QueryTransformer _testedInstance;

        private static SelectionPartParseProduct<T> CreateSelectionPartParseProduct<T>(
            ReadOnlyCollection<SqlExpression> columns, Func<IValueConverter, object[], T> selectionFunc)
        {
            return new SelectionPartParseProduct<T>(columns, selectionFunc);
        }

        private static void AssertCollectionReadExpressionParseProduct<T>(
            Func<IValueConverter, object[], T> expectedItemReader, CollectionReadExpressionParseProduct<T> result)
        {
            var noSideItemReaderFactory = AssertAndCast<NoSideEffectItemReaderFactory<T>>(result.ItemReaderFactory);
            Assert.AreSame(expectedItemReader, noSideItemReaderFactory.ItemReader);
        }

            /// <summary>Инициализация тестов.</summary>
        [SetUp]
        public void SetUp()
        {
            _transformMethodsMock = new Mock<IExpressionTransformMethods>(MockBehavior.Strict);
            _testedInstance = new QueryTransformer(_transformMethodsMock.Object);
        }
        
        /// <summary>Тестирование преобразования запроса простой выборки записей.</summary>
        [Test]
        public void TestTransformSelectOneSDataRecord()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var query = new DataRecordsQuery(SOURCE_NAME, null, new ReadOnlyCollection<SortExpression>(new SortExpression[0]));

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.AreSame(OneSDataRecordReaderFactory.Default, parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса с выборкой элементов анонимного типа.
        /// </summary>
        [Test]
        public void TestTransformSelectTuple()
        {
            const string SOURCE_NAME = "[source]";
            const string FIRST_FIELD_NAME = "[first_field]";
            const string SECOND_FIELD_NAME = "[second_field]";

            // Arrange
            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = "any", IntField = -1 });

            var columns = Array.AsReadOnly(
                new[] {FIRST_FIELD_NAME, SECOND_FIELD_NAME}
                .Select(s => (SqlExpression) new SqlFieldExpression(s)).ToArray());

            var selectionPart = CreateSelectionPartParseProduct(columns, (vc, vs) => new {StringField = "Тест", IntField = 32});

            _transformMethodsMock
                .Setup(m => m.TransformSelectExpression(It.IsAny<QueryParseContext>(), selectExpression))
                .Returns(selectionPart);

            var query = CreateQuery(SOURCE_NAME, selectExpression);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT " + FIRST_FIELD_NAME + ", " + SECOND_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            AssertCollectionReadExpressionParseProduct(selectionPart.SelectionFunc, result);
        }

        /// <summary>Тестирование преобразования запроса простой выборки и фильтрации записей.</summary>
        [Test]
        public void TestTransformFilterOneSDataRecord()
        {
            const string SOURCE = "[source]";
            const string FILTER_FIELD = "[filter_field]";
            const string FILTER_VALUE = "filter_value";

            // Arrange
            var query = new DataRecordsQuery(SOURCE, r => r.GetString("any_field") == "Any", new ReadOnlyCollection<SortExpression>(new SortExpression[0]));
            var filter = query.Filter;

            _transformMethodsMock
                .Setup(m => m.TransformWhereExpression(It.IsAny<QueryParseContext>(), filter))
                .Returns<QueryParseContext, Expression<Func<OneSDataRecord, bool>>>((ctx, f) =>
                    {
                        var parameterName = ctx.Parameters.GetOrAddNewParameterName(FILTER_VALUE);

                        return new SqlBinaryRelationCondition(
                            SqlBinaryRelationType.Equal,
                            new SqlFieldExpression(FILTER_FIELD),
                            new SqlParameterExpression(parameterName));
                    });

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            
            Assert.AreEqual(1, command.Parameters.Count);
            var parameter = command.Parameters[0];
            Assert.AreEqual(FILTER_VALUE, parameter.Value);

            Assert.AreEqual(
                "SELECT * FROM " + SOURCE + " WHERE " + FILTER_FIELD + " = &" + parameter.Name, command.Sql);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.AreSame(OneSDataRecordReaderFactory.Default, parseProduct.ItemReaderFactory);
        }

        /// <summary>Тестирование преобразования запроса выборки и фильтрации данных.</summary>
        [Test]
        public void TestTransformSelectTuppleAndFilter()
        {
            const string SOURCE = "[source]";
            const string FIRST_FIELD_NAME = "[first_field]";
            const string SECOND_FIELD_NAME = "[second_field]";
            const string FILTER_VALUE = "filter_value";

            // Arrange
            
            // Where
            Expression<Func<OneSDataRecord, bool>> filter = r => r.GetString("any_field") == "Any";

            _transformMethodsMock
                .Setup(m => m.TransformWhereExpression(It.IsAny<QueryParseContext>(), filter))
                .Returns<QueryParseContext, Expression<Func<OneSDataRecord, bool>>>((ctx, f) =>
                {
                    var parameterName = ctx.Parameters.GetOrAddNewParameterName(FILTER_VALUE);

                    return new SqlBinaryRelationCondition(
                        SqlBinaryRelationType.Equal,
                        new SqlFieldExpression(FIRST_FIELD_NAME),
                        new SqlParameterExpression(parameterName));
                });


            // Select
            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = "any", IntField = -1 });

            var columns = Array.AsReadOnly(
                new[] { FIRST_FIELD_NAME, SECOND_FIELD_NAME }
                .Select(s => (SqlExpression)new SqlFieldExpression(s)).ToArray());

            var selectionPart = CreateSelectionPartParseProduct(columns, (vc, vs) => new { StringField = "Тест", IntField = 32 });

            _transformMethodsMock
                .Setup(m => m.TransformSelectExpression(It.IsAny<QueryParseContext>(), selectExpression))
                .Returns(selectionPart);

            // Query
            var query = CreateQuery(SOURCE, selectExpression, filter);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(1, command.Parameters.Count);
            var parameter = command.Parameters[0];
            Assert.AreEqual(FILTER_VALUE, parameter.Value);

            Assert.AreEqual(
                "SELECT " + FIRST_FIELD_NAME + ", " + SECOND_FIELD_NAME + " FROM " + SOURCE + " WHERE " + FIRST_FIELD_NAME + " = &" + parameter.Name, command.Sql);

            AssertCollectionReadExpressionParseProduct(selectionPart.SelectionFunc, result);
        }

        /// <summary>Тестирование преобразования запроса простой выборки и сортировки записей по одному полю.</summary>
        [Test]
        public void TestTransformOrderByOneSDataRecord()
        {
            const string SOURCE = "[source]";
            const string SORT_FIELD = "[sort_field]";

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(SORT_FIELD);
            var sortExpression = new SortExpression(sortKeyExpression, SortKind.Ascending);

            var query = new DataRecordsQuery(SOURCE, null, new ReadOnlyCollection<SortExpression>(new[]{sortExpression}));

            _transformMethodsMock
                .Setup(m => m.TransformOrderByExpression(It.IsAny<QueryParseContext>(), sortKeyExpression))
                .Returns(new SqlFieldExpression(SORT_FIELD));

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(0, command.Parameters.Count);
            Assert.AreEqual(
                "SELECT * FROM " + SOURCE + " ORDER BY " + SORT_FIELD, command.Sql);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.AreSame(OneSDataRecordReaderFactory.Default, parseProduct.ItemReaderFactory);
        }
    }
}
