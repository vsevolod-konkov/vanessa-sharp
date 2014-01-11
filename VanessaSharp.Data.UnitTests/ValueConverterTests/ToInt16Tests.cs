using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToInt16"/>.</summary>
    [TestFixture]
    public sealed class ToInt16Tests : ToSmallNumberTestsBase<short>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, short> TestedMethod
        {
            get { return (c, v) => c.ToInt16(v); }
        }

        /// <summary>
        /// Тестирование <see cref="ValueConverter.ToInt16"/>
        /// в случае когда передается <see cref="Int16"/>.
        /// </summary>
        [Test]
        public void TestWhenInt16()
        {
            TestWhenSameType(12345);
        }

        /// <summary>
        /// Тестирование метода в случае, 
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(12345, c => c.ToInt16(null));
        }
    }
}
