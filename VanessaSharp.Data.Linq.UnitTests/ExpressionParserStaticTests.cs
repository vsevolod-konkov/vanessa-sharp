using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование статических методов парсера выражений <see cref="ExpressionParser"/>.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionParserStaticTests : ExpressionParserTestBase
    {
        /// <summary>
        /// Тестирование получения <see cref="ExpressionParser.GetQueryFromQueryableExpression"/>
        /// в случае когда передается выражение получения записей из источника.
        /// </summary>
        [Test]
        public void TestGetQueryFromGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expression = GetGetRecordsExpression(SOURCE_NAME);

            // Act
            var query = ExpressionParser.GetQueryFromQueryableExpression(expression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, query.Source);
            Assert.IsNull(query.Filter);
            Assert.IsInstanceOf<DataRecordsQuery>(query);
        }

        /// <summary>
        /// Тестирование получения <see cref="ExpressionParser.GetQueryFromQueryableExpression"/>
        /// в случае когда передается выражение выборки полей из записей источника.
        /// </summary>
        [Test]
        public void TestGetQueryFromSelectRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var selectExpression = Trait.Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.Select(selectExpression));
            var trait = selectExpression.GetTraitOfOutputType();

            // Act
            var result = ExpressionParser.GetQueryFromQueryableExpression(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.IsNull(result.Filter);
            var typedQuery = AssertAndCastSimpleQuery(trait, result);
            Assert.AreEqual(selectExpression, typedQuery.SelectExpression);
        }

        /// <summary>
        /// Тестирование получения <see cref="ExpressionParser.GetQueryFromQueryableExpression"/>
        /// в случае когда передается выражение получения записей из источника с фильтром.
        /// </summary>
        [Test]
        public void TestGetQueryFromWhereRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("filterField") == "filterValue";
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.Where(filterExpression));

            // Act
            var result = ExpressionParser.GetQueryFromQueryableExpression(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(filterExpression, result.Filter);
            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        // TODO: CopyPaste
        /// <summary>
        /// Тестирование получения <see cref="ExpressionParser.GetQueryFromQueryableExpression"/>
        /// в случае когда передается выражение получения записей из источника с сортировкой.
        /// </summary>
        [Test]
        public void TestGetQueryFromOrderByRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.OrderBy(orderbyExpression));

            // Act
            var result = ExpressionParser.GetQueryFromQueryableExpression(testedExpression);

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
        /// Тестирование получения <see cref="ExpressionParser.GetQueryFromQueryableExpression"/>
        /// в случае когда передается выражение получения записей из источника с сортировкой по убыванию.
        /// </summary>
        [Test]
        public void TestGetQueryFromOrderByDescendingRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");
            var testedExpression = TestHelperQueryProvider
                .BuildTestQueryExpression(SOURCE_NAME, q => q.OrderByDescending(orderbyExpression));

            // Act
            var result = ExpressionParser.GetQueryFromQueryableExpression(testedExpression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(1, result.Sorters.Count);

            var sortExpression = result.Sorters[0];
            Assert.AreEqual(orderbyExpression, sortExpression.KeyExpression);
            Assert.AreEqual(SortKind.Descending, sortExpression.Kind);

            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        private static CustomDataTypeQuery<T> AssertAndCastSimpleQuery<T>(Trait<T> trait, SimpleQuery query)
        {
            return AssertAndCast<CustomDataTypeQuery<T>>(query);
        }
    }
}
