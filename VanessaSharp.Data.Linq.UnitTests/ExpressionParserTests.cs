using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование парсера выражений.</summary>
    [TestFixture]
    public sealed class ExpressionParserTests : TestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private readonly ExpressionParser _testedInstance = ExpressionParser.Default;

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";

            var expression = Expression.Call(
                OneSQueryExpressionHelper.GetRecordsExpression(SOURCE_NAME),
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCast<ExpressionParseProduct<OneSDataRecord>>(product);
            var itemReader = recordProduct.ItemReader;


        }
    }
}
