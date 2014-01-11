using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToDouble"/>.</summary>
    [TestFixture]
    public sealed class ToDoubleTests : ToValueTypeTestsBase<double>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, double> TestedMethod
        {
            get { return (c, v) => c.ToDouble(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="double"/>.
        /// </summary>
        [Test]
        public void TestWhenDouble()
        {
            TestWhenSameType(12345345346363634.32525316136);
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(1234536243263636.4534256326326, c => c.ToDouble(null));
        }
    }
}
