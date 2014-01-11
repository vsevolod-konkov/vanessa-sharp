using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToInt32"/>.</summary>
    [TestFixture]
    public sealed class ToInt32Tests : ToSmallNumberTestsBase<int>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, int> TestedMethod
        {
            get { return (c, v) => c.ToInt32(v); }
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToInt32"/>
        /// в случае когда передается <see cref="Int32"/>.
        /// </summary>
        [Test]
        public void TestWhenInt32()
        {
            TestWhenSameType(12345);
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToInt32"/>
        /// в случае, если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(12345, c => c.ToInt32(null));
        }
    }
}
