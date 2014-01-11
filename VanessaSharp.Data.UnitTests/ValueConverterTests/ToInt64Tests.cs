using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToInt64"/>.</summary>
    [TestFixture]
    public sealed class ToInt64Tests : ToValueTypeTestsBase<long>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override Func<IValueConverter, object, long> TestedMethod
        {
            get { return (c, v) => c.ToInt64(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="Int64"/>.
        /// </summary>
        [Test]
        public void TestWhenInt64()
        {
            TestWhenSameType(12345345346363634);
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(1234536243263636, c => c.ToInt64(null));
        }
    }
}
