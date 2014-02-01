using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToDecimal"/>.</summary>
    [TestFixture]
    public sealed class ToDateTimeTests : ToValueTypeTestsBase<DateTime>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, DateTime> TestedMethod
        {
            get { return (c, v) => c.ToDateTime(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="DateTime"/>.
        /// </summary>
        [Test]
        public void TestWhenDateTime()
        {
            TestWhenSameType(new DateTime(2029, 3, 16));
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(new DateTime(2029, 3, 16), c => c.ToDateTime(null));
        }
    }
}
