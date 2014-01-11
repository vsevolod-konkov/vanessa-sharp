using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests.ValueConverterTests
{
    /// <summary>Базовые методы для тестирования <see cref="ValueConverter"/>.</summary>
    public abstract class ValueConverterTestsBase<T>
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private static IValueConverter TestedInstance { get { return ValueConverter.Default; } }

        /// <summary>Тестируемый метод.</summary>
        internal abstract Func<IValueConverter, object, T> TestedMethod { get; }

        /// <summary>Тестирование метода.</summary>
        /// <param name="value">Передаваемое значение в метод.</param>
        protected T Act(object value)
        {
            return TestedMethod(TestedInstance, value);
        }

        /// <summary>Запуск метода и проверка резултата.</summary>
        /// <param name="value">Передаваемое значение.</param>
        /// <param name="expectedResult">Ожидаемый результат.</param>
        protected void ActAndAssertResult(object value, T expectedResult)
        {
            Assert.AreEqual(expectedResult, Act(value));
        }

       

        /// <summary>
        /// Тестирование 
        /// в случае когда передается значение требуемого типа.
        /// </summary>
        protected void TestWhenSameType(T value)
        {
            ActAndAssertResult(value, value);
        }

        /// <summary>
        /// Тестировани в случае, 
        /// если передается объект поддерживающий <see cref="IConvertible"/>.
        /// </summary>
        protected void TestWhenConvertible(T expectedValue, Expression<Func<IConvertible, T>> convertibleMethod)
        {
            var mockValue = new Mock<IConvertible>(MockBehavior.Strict);
            mockValue
                .Setup(convertibleMethod)
                .Returns(expectedValue)
                .Verifiable();

            ActAndAssertResult(mockValue.Object, expectedValue);

            mockValue
                .Verify(convertibleMethod, Times.Once());
        }
    }
}
