using System;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToString(object)"/>.</summary>
    [TestFixture]
    public sealed class ToStringTests : ValueConverterTestsBase<string>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, string> TestedMethod
        {
            get { return (c, v) => c.ToString(v); }
        }

        /// <summary>
        /// Тестирование в случае когда в метод передается <c>null</c>.
        /// </summary>
        [Test]
        public void TestWhenNull()
        {
            Assert.IsNull(Act(null));
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToString(object)"/>
        /// в случае когда передается строка.
        /// </summary>
        [Test]
        public void TestWhenString()
        {
            TestWhenSameType("Test");
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToString(object)"/>
        /// в случае, если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible("Test", c => c.ToString(null));
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToString(object)"/>
        /// в случае, если передается объект поддерживающий <see cref="IFormattable"/>.
        /// </summary>
        [Test]
        public void TestWhenFormattable()
        {
            const string EXPECTED_VALUE = "Test";

            var mockValue = new Mock<IFormattable>(MockBehavior.Strict);
            mockValue
                .Setup(c => c.ToString(null, null))
                .Returns(EXPECTED_VALUE)
                .Verifiable();

            ActAndAssertResult(mockValue.Object, EXPECTED_VALUE);

            mockValue
                .Verify(c => c.ToString(null, null), Times.Once());
        }
    }
}
