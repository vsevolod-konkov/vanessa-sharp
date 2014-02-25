using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>
    /// Базовый класс для 
    /// тестирование метода
    /// возвращающий значимый тип.
    /// </summary>
    /// <typeparam name="T">Возвращаемый тип тестируемого метода.</typeparam>
    public abstract class ToValueTypeTestsBase<T> : ValueConverterTestsBase<T>
        where T : struct
    {
        /// <summary>
        /// Тестирование в случае когда в метод передается <c>null</c>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void TestWhenNull()
        {
            Act(null);
        }
    }
}
