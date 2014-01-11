using System;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>
    /// Базовый класс для 
    /// тестирование метода
    /// возвращающий число не самого большого размера.
    /// </summary>
    /// <typeparam name="T">Возвращаемый тип тестируемого метода.</typeparam>
    public abstract class ToSmallNumberTestsBase<T> : ToValueTypeTestsBase<T>
        where T : struct
    {
        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void TestWhenOverflowValue()
        {
            const long TEST_VALUE = 23421542564634643;
            Act(TEST_VALUE);
        }
    }
}
