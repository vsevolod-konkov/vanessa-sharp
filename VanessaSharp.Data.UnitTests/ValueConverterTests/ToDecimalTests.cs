using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToDecimal"/>.</summary>
    [TestFixture]
    public sealed class ToDecimalTests : ToValueTypeTestsBase<decimal>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, decimal> TestedMethod
        {
            get { return (c, v) => c.ToDecimal(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="decimal"/>.
        /// </summary>
        [Test]
        public void TestWhenDecimal()
        {
            TestWhenSameType(12345345346363634.32525316136m);
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(1234536243263636.4534256326326m, c => c.ToDecimal(null));
        }
    }
}
