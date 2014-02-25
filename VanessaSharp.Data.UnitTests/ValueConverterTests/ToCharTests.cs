using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToChar"/>.</summary>
    [TestFixture]
    public sealed class ToCharTests : ToSmallNumberTestsBase<char>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, char> TestedMethod
        {
            get { return (c, v) => c.ToChar(v); }
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToChar"/>
        /// в случае когда передается <see cref="Char"/>.
        /// </summary>
        [Test]
        public void TestWhenChar()
        {
            const char TEST_VALUE = 'A';
            TestWhenSameType(TEST_VALUE);
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToChar"/>
        /// в случае, если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            const char EXPECTED_VALUE = 'Z';
            TestWhenConvertible(EXPECTED_VALUE, c => c.ToChar(null));
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToChar"/>
        /// в случае, если передается строка имеющая единичную длину.
        /// </summary>
        [Test]
        public void TestWhenStringHasOneLength()
        {
            ActAndAssertResult("A", 'A');
        }
    }
}