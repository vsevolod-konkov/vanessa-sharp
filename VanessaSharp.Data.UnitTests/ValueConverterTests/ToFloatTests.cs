using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Тестирование <see cref="ValueConverter.ToFloat"/>.</summary>
    [TestFixture]
    public sealed class ToFloatTests : ToValueTypeTestsBase<float>
    {
        /// <summary>Тестируемый метод.</summary>
        internal override System.Func<IValueConverter, object, float> TestedMethod
        {
            get { return (c, v) => c.ToFloat(v); }
        }

        /// <summary>
        /// Тестирование метода в случае когда передается <see cref="float"/>.
        /// </summary>
        [Test]
        public void TestWhenFloat()
        {
            TestWhenSameType(123.325f);
        }

        /// <summary>
        /// Тестирование метода в случае,
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        [Test]
        public void TestWhenConvertible()
        {
            TestWhenConvertible(636.453f, c => c.ToSingle(null));
        }
    }
}
