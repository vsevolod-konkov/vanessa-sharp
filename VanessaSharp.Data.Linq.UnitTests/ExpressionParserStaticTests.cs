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

            var provider = new TestHelperQueryProvider();
            var query = provider.CreateQuery<OneSDataRecord>(
                OneSQueryExpressionHelper.GetRecordsExpression(SOURCE_NAME));

            var selectExpression = Trait.Of<OneSDataRecord>().SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });

            var selectQuery = query.Select(selectExpression);
            var trait = selectQuery.GetTrait();

            var expression = Expression.Call(
                selectQuery.Expression,
                GetGetEnumeratorMethodInfo(trait));

            // Act
            var result = ExpressionParser.GetQueryFromQueryableExpression(expression);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            var typedQuery = AssertAndCastSimpleQuery(trait, result);
            Assert.AreEqual(selectExpression, typedQuery.SelectExpression);
        }

        private static CustomDataTypeQuery<T> AssertAndCastSimpleQuery<T>(Trait<T> trait, SimpleQuery query)
        {
            return AssertAndCast<CustomDataTypeQuery<T>>(query);
        }
    }
}
