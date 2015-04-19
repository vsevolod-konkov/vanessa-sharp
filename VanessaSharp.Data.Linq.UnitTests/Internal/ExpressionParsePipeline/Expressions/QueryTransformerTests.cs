using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="QueryTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryTransformerTests : QueryTransformerTestsBase
    {
        private Mock<IExpressionTransformMethods> _transformMethodsMock;
        private Mock<ISourceDescription> _sourceDescriptionMock; 
        private QueryTransformer _testedInstance;

        private const string SOURCE_NAME = "[source]";

        private IQuery<TInput, TOutput> CreateQuery<TInput, TOutput>(Expression<Func<TInput, TOutput>> selector = null,
                                                                     Expression<Func<TInput, bool>> filter = null,
                                                                     params SortExpression[] sorters)
        {
            return CreateQuery(_sourceDescriptionMock.Object, selector, filter, sorters);
        }

        private static SelectionPartParseProduct<T> CreateSelectionPartParseProduct<T>(
            Func<IValueConverter, object[], T> selectionFunc, params string[] fields)
        {
            return new SelectionPartParseProduct<T>(
                fields.Select(c => (SqlExpression)new SqlFieldExpression(SqlDefaultTableExpression.Instance, c)).ToReadOnly(),
                selectionFunc);
        }

        private SelectionPartParseProduct<TOutput> SetupTransformSelector<TInput, TOutput>(
            Expression<Func<TInput, TOutput>> selector, Func<IValueConverter, object[], TOutput> expectedItemReader, params string[] expectedFields)
        {
            var selectionPart = CreateSelectionPartParseProduct(expectedItemReader, expectedFields);

            _transformMethodsMock
                .Setup(m => m.TransformSelectExpression(It.IsAny<QueryParseContext>(), selector))
                .Returns(selectionPart);

            return selectionPart;
        }

        private void SetupTransformFilter<T>(Expression<Func<T, bool>> filter, string filterField, string filterValue)
        {
            _transformMethodsMock
                .Setup(m => m.TransformWhereExpression(It.IsAny<QueryParseContext>(), filter))
                .Returns<QueryParseContext, Expression<Func<T, bool>>>((ctx, f) =>
                {
                    var parameterName = ctx.Parameters.GetOrAddNewParameterName(filterValue);

                    return new SqlBinaryRelationCondition(
                        SqlBinaryRelationType.Equal,
                        new SqlFieldExpression(SqlDefaultTableExpression.Instance, filterField),
                        new SqlParameterExpression(parameterName));
                });
        }

        private void SetupTransformSorter<T, TKey>(Expression<Func<T, TKey>> sortKeyExpression, string fieldName)
        {
            _transformMethodsMock
                .Setup(t => t.TransformOrderByExpression(It.IsAny<QueryParseContext>(), sortKeyExpression))
                .Returns(new SqlFieldExpression(SqlDefaultTableExpression.Instance, fieldName));
        }

        private void VerifySourceDescription()
        {
            _sourceDescriptionMock
                .Verify(sd => sd.GetSourceName(_transformMethodsMock.Object));
        }

        private static void AssertFilteringCommand(string expectedSqlPart, string expectedParameterValue, SqlCommand command)
        {
            Assert.AreEqual(1, command.Parameters.Count);
            var parameter = command.Parameters[0];
            Assert.AreEqual(expectedParameterValue, parameter.Value);

            AssertSql(
                expectedSqlPart + parameter.Name, command);
        }

        private static void AssertCollectionReadExpressionParseProduct<T>(
            Func<IValueConverter, object[], T> expectedItemReader, CollectionReadExpressionParseProduct<T> result)
        {
            var noSideItemReaderFactory = AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<T>>(result.ItemReaderFactory);
            Assert.AreSame(expectedItemReader, noSideItemReaderFactory.ItemReader);
        }

        private static void AssertSql(string expectedSql, SqlCommand command)
        {
            Assert.AreEqual(
                string.Format(expectedSql, SOURCE_NAME),
                command.Sql
                );
        }

        private void AssertResult<T>(
            string expectedSql, Func<IValueConverter, object[], T> expectedItemReader,
            CollectionReadExpressionParseProduct<T> result)
        {
            AssertSql(expectedSql, result.Command);
            Assert.AreEqual(0, result.Command.Parameters.Count);
            AssertCollectionReadExpressionParseProduct(expectedItemReader, result);
            VerifySourceDescription();
        }

        private static Func<IValueConverter, object[], T> CreateItemReader<T>(Func<IValueConverter, object[], T> itemReader)
        {
            return itemReader;
        }

        /// <summary>Инициализация тестов.</summary>
        [SetUp]
        public void SetUp()
        {
            _sourceDescriptionMock = new Mock<ISourceDescription>(MockBehavior.Strict);
            _transformMethodsMock = new Mock<IExpressionTransformMethods>(MockBehavior.Strict);
            _testedInstance = new QueryTransformer(_transformMethodsMock.Object);

            _sourceDescriptionMock
                .Setup(sd => sd.GetSourceName(_transformMethodsMock.Object))
                .Returns(SOURCE_NAME);
        }
        
        /// <summary>
        /// Тестирование преобразования запроса получения записей без проекции.
        /// </summary>
        [Test]
        public void TestTransformGetDataRecordsQuery()
        {
            // Arrange
            const string FILTER_VALUE = "filter_value";

            var query = CreateQuery<OneSDataRecord, OneSDataRecord>(filter: r => true);

            SetupTransformFilter(query.Filter, "ФильтрПоле", FILTER_VALUE);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            AssertFilteringCommand(
                "SELECT * FROM {0} WHERE ФильтрПоле = &",
                FILTER_VALUE,
                result.Command);
            var parseProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.AreSame(OneSDataRecordReaderFactory.Default, parseProduct.ItemReaderFactory);
            VerifySourceDescription();
        }

        /// <summary>
        /// Тестирование преобразования запроса записей с проекцией на анонимный тип.
        /// </summary>
        [Test]
        public void TestTransformSelectDataRecordsQuery()
        {
            // Arrange
            
            // Select
            var selector = Trait
                .Of<OneSDataRecord>()
                .SelectExpression(r => new { First = "any", Second = -1 });

            var selectionPart = SetupTransformSelector(
                selector,
                (vc, vs) => new { First = "Тест", Second = 32},
                "ПервоеПоле", "ВтороеПоле");

            // Order by
            Expression<Func<OneSDataRecord, int>> sortKeyExpression1 = r => 10;
            SetupTransformSorter(sortKeyExpression1, "Ключ1");

            Expression<Func<OneSDataRecord, string>> sortKeyExpression2 = r => "Test";
            SetupTransformSorter(sortKeyExpression2, "Ключ2");

            var query = CreateQuery(
                selector, 
                sorters: new[]
                    {
                        new SortExpression(sortKeyExpression1, SortKind.Descending),
                        new SortExpression(sortKeyExpression2, SortKind.Ascending)
                    });

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            AssertResult(
                "SELECT ПервоеПоле, ВтороеПоле FROM {0} ORDER BY Ключ1 DESC, Ключ2",
                selectionPart.SelectionFunc,
                result
                );
        }

        /// <summary>
        /// Тестирование преобразования запроса получения типизированных записей.
        /// </summary>
        [Test]
        public void TestTransformGetTypedRecordsQuery()
        {
            // Arrange
            Func<IValueConverter, object[], SomeData> someDataReader = (c, v) => new SomeData();

            var selectPart = CreateSelectionPartParseProduct(someDataReader, "Наименование", "Идентификатор", "Цена");

            _transformMethodsMock
                .Setup(t => t.TransformSelectTypedRecord<SomeData>())
                .Returns(selectPart);

            Expression<Func<SomeData, int>> sortKeyExpression1 = d => 5;
            SetupTransformSorter(sortKeyExpression1, "Ключ1");

            Expression<Func<SomeData, string>> sortKeyExpression2 = d => "Test";
            SetupTransformSorter(sortKeyExpression2, "Ключ2");
            
            var testedQuery = CreateQuery<SomeData, SomeData>(sorters: 
                new[]
                    {
                        new SortExpression(sortKeyExpression1, SortKind.Ascending),
                        new SortExpression(sortKeyExpression2, SortKind.Descending)
                    }
                );

            // Act
            var result = _testedInstance.Transform(testedQuery);

            // Assert
            AssertResult(
                "SELECT Наименование, Идентификатор, Цена FROM {0} ORDER BY Ключ1, Ключ2 DESC",
                someDataReader,
                result
                );
        }

        /// <summary>
        /// Тестирование преобразования запроса выборки типизированных записей.
        /// </summary>
        [Test]
        public void TestTransformSelectTypedRecordsQuery()
        {
            // Arrange
            _transformMethodsMock
                .Setup(t => t.ResolveSourceNameForTypedRecord<SomeData>())
                .Returns("Тест");

            Expression<Func<SomeData, bool>> filter = d => true;
            SetupTransformFilter(filter, "КраткоеИмя", "Фильтр");

            var selector = Trait
                .Of<SomeData>()
                .SelectExpression(d => new { First = 5, Second = "TestName" });
            var expectedItemReader = CreateItemReader((c, v) => new { First = 45, Second = "XXX" });
            SetupTransformSelector(selector, expectedItemReader, "Первый", "Второй");

            var testedQuery = CreateQuery(selector, filter);

            // Act
            var result = _testedInstance.Transform(testedQuery);

            // Assert
            AssertFilteringCommand(
                "SELECT Первый, Второй FROM {0} WHERE КраткоеИмя = &",
                "Фильтр", result.Command);
            AssertCollectionReadExpressionParseProduct(expectedItemReader, result);
            VerifySourceDescription();
        }
    }
}
