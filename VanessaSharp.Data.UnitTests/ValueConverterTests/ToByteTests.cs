using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToByte"/>.</summary>
    [TestFixture]
    public sealed class ToByteTests : ToSmallNumberTestsBase<byte>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, byte> TestedMethod
        {
            get { return (c, v) => c.ToByte(v); }
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToByte"/>
        /// в случае когда передается <see cref="Byte"/>.
        /// </summary>
        [Test]
        public void TestWhenByte()
        {
            const byte TEST_VALUE = 123;
            TestWhenSameType(TEST_VALUE);
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToByte"/>
        /// в случае, если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            const byte EXPECTED_VALUE = 123;
            TestWhenConvertible(EXPECTED_VALUE, c => c.ToByte(null));
        }
    }
}
