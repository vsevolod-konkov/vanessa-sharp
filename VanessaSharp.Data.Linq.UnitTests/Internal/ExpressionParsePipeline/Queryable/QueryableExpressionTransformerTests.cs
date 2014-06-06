using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Тестирование статических методов парсера выражений <see cref="QueryableExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryableExpressionTransformerTests : TestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private QueryableExpressionTransformer _testedInstance;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new QueryableExpressionTransformer();
        }

        /// <summary>
        /// Получение выражения получения записей из источника данных.
        /// </summary>
        /// <param name="sourceName">Источник.</param>
        private static Expression GetGetRecordsExpression(string sourceName)
        {
            return Expression.Call(
                OneSQueryExpressionHelper.GetRecordsExpression(sourceName),
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());
        }

        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение получения записей из источника.
        /// </summary>
        [Test]
        public void TestTransformFromGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expression = GetGetRecordsExpression(SOURCE_NAME);

            // Act
            var query = _testedInstance.Transform(expression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, query.Source);
            Assert.IsNull(query.Filter);
            Assert.IsInstanceOf<DataRecordsQuery>(query);
        }

        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение выборки полей из записей источника.
        /// </summary>
        [Test]
        public void TestTransformFromSelectRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var selectExpression = Trait.Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.Select(selectExpression));
            var trait = selectExpression.GetTraitOfOutputType();

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.IsNull(result.Filter);
            var typedQuery = AssertAndCastSimpleQuery(trait, result);
            Assert.AreEqual(selectExpression, typedQuery.SelectExpression);
        }

        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение получения записей из источника с фильтром.
        /// </summary>
        [Test]
        public void TestTransformFromWhereRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("filterField") == "filterValue";
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.Where(filterExpression));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(filterExpression, result.Filter);
            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        // TODO: CopyPaste
        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение получения записей из источника с сортировкой.
        /// </summary>
        [Test]
        public void TestTransformFromOrderByRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.OrderBy(orderbyExpression));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(1, result.Sorters.Count);
            
            var sortExpression = result.Sorters[0];
            Assert.AreEqual(orderbyExpression, sortExpression.KeyExpression);
            Assert.AreEqual(SortKind.Ascending, sortExpression.Kind);

            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        // TODO: CopyPaste
        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение получения записей из источника с сортировкой по убыванию.
        /// </summary>
        [Test]
        public void TestTransformFromOrderByDescendingRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.OrderByDescending(orderbyExpression));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(1, result.Sorters.Count);

            var sortExpression = result.Sorters[0];
            Assert.AreEqual(orderbyExpression, sortExpression.KeyExpression);
            Assert.AreEqual(SortKind.Descending, sortExpression.Kind);

            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        // TODO: CopyPaste
        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда передается выражение получения записей из источника с сортировкой по убыванию.
        /// </summary>
        [Test]
        public void TestTransformFromOrderByDescendingAndThenByRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> orderbyExpression1 = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> orderbyExpression2 = r => r.GetString("sort_field_2");

            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.OrderByDescending(orderbyExpression1).ThenBy(orderbyExpression2));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(2, result.Sorters.Count);

            var sortExpression1 = result.Sorters[0];
            Assert.AreEqual(orderbyExpression1, sortExpression1.KeyExpression);
            Assert.AreEqual(SortKind.Descending, sortExpression1.Kind);

            var sortExpression2 = result.Sorters[1];
            Assert.AreEqual(orderbyExpression2, sortExpression2.KeyExpression);
            Assert.AreEqual(SortKind.Ascending, sortExpression2.Kind);

            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        private static CustomDataTypeQuery<T> AssertAndCastSimpleQuery<T>(Trait<T> trait, SimpleQuery query)
        {
            return AssertAndCast<CustomDataTypeQuery<T>>(query);
        }
    }
}
