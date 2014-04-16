using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тесты на экземпляр <see cref="ExpressionParser"/>.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionParserInstanceTests : ExpressionParserTestBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private readonly ExpressionParser _testedInstance = ExpressionParser.Default;

        private static CollectionReadExpressionParseProduct<T> AssertAndCastCollectionReadExpressionParseProduct<T>(
            Trait<T> trait, ExpressionParseProduct product)
        {
            return AssertAndCast<CollectionReadExpressionParseProduct<T>>(product);
        }

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expression = GetGetRecordsExpression(SOURCE_NAME);

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(product);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга выражения выборки полей из записей источника.
        /// </summary>
        [Test]
        public void TestParseSelectRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string STRING_FIELD_NAME = "[string_field]";
            const string INT32_FIELD_NAME = "[int_field]";

            var provider = new TestHelperQueryProvider();
            var query = provider.CreateQuery<OneSDataRecord>(
                OneSQueryExpressionHelper.GetRecordsExpression(SOURCE_NAME));

            var selectQuery = query.Select(r => new { StringField = r.GetString(STRING_FIELD_NAME), IntField = r.GetInt32(INT32_FIELD_NAME) });
            var trait = selectQuery.GetTrait();

            var expression = Expression.Call(
                selectQuery.Expression,
                GetGetEnumeratorMethodInfo(trait));

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT " + STRING_FIELD_NAME + ", " + INT32_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCastCollectionReadExpressionParseProduct(trait, product);

            // TODO test recordProduct
        }
    }
}