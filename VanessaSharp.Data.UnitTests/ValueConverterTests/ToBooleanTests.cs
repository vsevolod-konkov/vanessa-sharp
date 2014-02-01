using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToDecimal"/>.</summary>
    [TestFixture]
    public sealed class ToBooleanTests : ToValueTypeTestsBase<bool>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, bool> TestedMethod
        {
            get { return (c, v) => c.ToBoolean(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="bool"/>.
        /// </summary>
        [Test]
        public void TestWhenBoolean()
        {
            TestWhenSameType(true);
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(true, c => c.ToBoolean(null));
        }
    }
}
