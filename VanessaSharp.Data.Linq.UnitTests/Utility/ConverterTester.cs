using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Тестировщик конвертера значений.
    /// </summary>
    internal static class ConverterTester
    {
        public static void Test<T>(
            Func<IValueConverter, object, T> testedConverter, Expression<Func<IValueConverter, T>> callValueConverter, T expectedValue)
        {
            var rawValue = new object();

            callValueConverter = SubstituteConvertValue(callValueConverter, rawValue);
            
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(callValueConverter)
                .Returns(expectedValue);

            Assert.AreEqual(expectedValue, testedConverter(valueConverterMock.Object, rawValue));
            valueConverterMock
                .Verify(callValueConverter, Times.Once());
        }

        /// <summary>
        /// Конструирование выражения вызова метода конвертации
        /// для конкретного значения буфера.
        /// </summary>
        /// <typeparam name="TValue">Тип значения, ожидаемого на выходе.</typeparam>
        /// <param name="convertMethodExpression">Переданное выражение вызова метода конвертации.</param>
        /// <param name="value">Значение из буфера данных.</param>
        public static Expression<Func<IValueConverter, TValue>> SubstituteConvertValue<TValue>(
            Expression<Func<IValueConverter, TValue>> convertMethodExpression, object value)
        {
            var convertMethod = ((MethodCallExpression)convertMethodExpression.Body).Method;
            var converterParameter = Expression.Parameter(typeof(IValueConverter));

            return Expression.Lambda<Func<IValueConverter, TValue>>(
                Expression.Call(
                    converterParameter,
                    convertMethod,
                    Expression.Constant(value)),
                converterParameter);
        }
    }
}
