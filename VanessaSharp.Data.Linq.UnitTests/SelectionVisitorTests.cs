using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    [TestFixture]
    public sealed class SelectionVisitorTests : TestsBase
    {
        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей из записи.
        /// </summary>
        [Test]
        public void TestParseSelectTuple()
        {
            // Arrange
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = r.GetString(FIRST_FIELD_NAME), IntField = r.GetInt32(SECOND_FIELD_NAME) });

            // Act
            var result = SelectionVisitor.Parse(selectExpression);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { new SqlFieldExpression(FIRST_FIELD_NAME), new SqlFieldExpression(SECOND_FIELD_NAME) }, 
                result.Columns);

            // Тестирование полученного делегата чтения кортежа
            var itemReader = result.SelectionFunc;

            // Arrange
            const string STRING_VALUE = "Test";
            const int INT32_VALUE = 34;

            var values = new object[] { STRING_VALUE, INT32_VALUE };
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToString(values[0]))
                .Returns(STRING_VALUE)
                .Verifiable();
            valueConverterMock
                .Setup(c => c.ToInt32(values[1]))
                .Returns(INT32_VALUE)
                .Verifiable();

            // Act
            var item = itemReader(valueConverterMock.Object, values);

            // Assert
            Assert.AreEqual(STRING_VALUE, item.StringField);
            Assert.AreEqual(INT32_VALUE, item.IntField);
            valueConverterMock
                .Verify(c => c.ToString(values[0]), Times.Once());
            valueConverterMock
                .Verify(c => c.ToInt32(values[1]), Times.Once());
        }
    }
}
