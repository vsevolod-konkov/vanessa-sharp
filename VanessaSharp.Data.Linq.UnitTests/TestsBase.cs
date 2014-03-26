using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Базовый класс для тестов.
    /// </summary>
    public abstract class TestsBase
    {
        /// <summary>
        /// Проверка и приведение к типу <typeparamref name="T"/> значения <paramref name="value"/>.
        /// </summary>
        protected static T AssertAndCast<T>(object value)
        {
            Assert.IsInstanceOf<T>(value);

            return (T)value;
        }
    }
}
