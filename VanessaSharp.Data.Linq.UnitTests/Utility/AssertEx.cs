using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Дополнительные методы для проверки результатов тестов.
    /// </summary>
    internal static class AssertEx
    {
        /// <summary>
        /// Проверка и приведение к типу <typeparamref name="T"/> значения <paramref name="value"/>.
        /// </summary>
        public static T IsInstanceAndCastOf<T>(object value)
        {
            Assert.IsInstanceOf<T>(value);

            return (T)value;
        }
    }
}
