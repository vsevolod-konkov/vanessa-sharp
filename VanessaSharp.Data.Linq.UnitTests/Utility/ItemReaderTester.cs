using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>Тестировщик читателя элементов из буфера сырых значений 1С.</summary>
    internal static class ItemReaderTester
    {
        /// <summary>Создание построителя теста.</summary>
        /// <typeparam name="T">Тип читаемого элемента данных.</typeparam>
        /// <param name="testedItemReader">Тестируемый делегат.</param>
        /// <param name="fieldsCount">Количество читаемых полей.</param>
        public static TestBuilder<T> For<T>(Func<IValueConverter, object[], T> testedItemReader, int fieldsCount)
        {
            Contract.Requires<ArgumentNullException>(testedItemReader != null);
            Contract.Requires<ArgumentException>(fieldsCount > 0);

            return new TestBuilder<T>(testedItemReader, fieldsCount);
        }

        /// <summary>Построитель теста для конкретного типа читаемых элементов.</summary>
        /// <typeparam name="T">Тип читаемых элементов.</typeparam>
        internal sealed class TestBuilder<T>
        {
            /// <summary>Тестируемый делегат чтения элементов данных.</summary>
            private readonly Func<IValueConverter, object[], T> _testedItemReader;

            /// <summary>Буфер значений используемых тестируемым делегатом.</summary>
            private readonly object[] _values;

            /// <summary>Тестировщики полей вычитанного элемента.</summary>
            private readonly IFieldTester[] _fieldTesters;

            /// <summary>Мок конвертера значений.</summary>
            private readonly Mock<IValueConverter> _valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict); 
            
            /// <summary>Конструктор.</summary>
            /// <param name="testedItemReader">Тестируемый делегат чтения элементов данных.</param>
            /// <param name="fieldsCount">Количество читаемых полей.</param>
            public TestBuilder(Func<IValueConverter, object[], T> testedItemReader, int fieldsCount)
            {
                Contract.Requires<ArgumentNullException>(testedItemReader != null);
                Contract.Requires<ArgumentException>(fieldsCount > 0);
                
                _testedItemReader = testedItemReader;
                _values = Enumerable.Range(0, fieldsCount).Select(i => new object()).ToArray();
                _fieldTesters = new IFieldTester[fieldsCount];
            }

            /// <summary>
            /// Количество полей соответствующее размеру буфера значений,
            /// используемых делегатом чтения элементов.
            /// </summary>
            public int FieldsCount
            {
                get { return _values.Length; }
            }
            
            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            public TestBuilder<T> Field<TValue>(int valueIndex, Func<T, TValue> fieldAccessor, Expression<Func<IValueConverter, TValue>> convertMethod, TValue expectedValue)
            {
                Contract.Requires<ArgumentOutOfRangeException>(valueIndex >= 0 && valueIndex < FieldsCount);
                Contract.Requires<ArgumentNullException>(fieldAccessor != null);
                Contract.Requires<ArgumentNullException>(convertMethod != null);

                var value = _values[valueIndex];
                var substitutedConvertMethod = SubstituteConvertValue(convertMethod, value);

                // Инициализация мока конвертера вызова метода с заданным аргументом из буфера
                _valueConverterMock
                    .Setup(substitutedConvertMethod)
                    .Returns(expectedValue);

                _fieldTesters[valueIndex] = new FieldTester<TValue>(fieldAccessor, expectedValue, substitutedConvertMethod);
                
                return this;
            }

            /// <summary>
            /// Конструирование выражения вызова метода конвертации
            /// для конкретного значения буфера.
            /// </summary>
            /// <typeparam name="TValue">Тип значения, ожидаемого на выходе.</typeparam>
            /// <param name="convertMethodExpression">Переданное выражение вызова метода конвертации.</param>
            /// <param name="value">Значение из буфера данных.</param>
            private static Expression<Func<IValueConverter, TValue>> SubstituteConvertValue<TValue>(
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

            /// <summary>
            /// Запуск теста делегата.
            /// </summary>
            public void Test()
            {
                // Проверка что все поля проинициализированы для теста.
                for (var valueIndex = 0; valueIndex < _fieldTesters.Length; valueIndex++)
                {
                    Contract.Assert(_fieldTesters[valueIndex] != null,
                        string.Format("Для индекса буфера значений \"{0}\" не проинициализировано поле для тестирования", valueIndex));
                }
                
                // Выполнение запуска делегата
                var testedItem = _testedItemReader(_valueConverterMock.Object, _values);

                Assert.IsNotNull(testedItem);

                foreach (var fieldTester in _fieldTesters)
                    fieldTester.Test(testedItem);

                foreach (var fieldTester in _fieldTesters)
                    fieldTester.VerifyConvert(_valueConverterMock);
            }

            /// <summary>
            /// Интерфейс тестировщика поля, независимый от типа поля.
            /// </summary>
            private interface IFieldTester
            {
                void Test(T testedItem);

                void VerifyConvert(Mock<IValueConverter> valueConverterMock);
            }

            /// <summary>
            /// Тестировщик поля для конкретного типа поля.
            /// </summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            private sealed class FieldTester<TValue> : IFieldTester
            {
                /// <summary>
                /// Получатель значения поля из вычитанного элемента.
                /// </summary>
                private readonly Func<T, TValue> _fieldAccessor;

                /// <summary>
                /// Ожидаемое значение элемента.
                /// </summary>
                private readonly TValue _expectedValue;
                
                /// <summary>
                /// Выражение конвертации значения из буфера.
                /// </summary>
                private readonly Expression<Func<IValueConverter, TValue>> _convertExpression;

                public FieldTester(Func<T, TValue> fieldAccessor, TValue expectedValue, Expression<Func<IValueConverter, TValue>> convertExpression)
                {
                    _fieldAccessor = fieldAccessor;
                    _expectedValue = expectedValue;
                    _convertExpression = convertExpression;
                }

                /// <summary>Тестирование значения поля.</summary>
                /// <param name="testedItem">Тестовый элемент, вычитанный при помощи тестируемого делегата.</param>
                public void Test(T testedItem)
                {
                    var actualValue = _fieldAccessor(testedItem);

                    Assert.AreEqual(_expectedValue, actualValue);
                }

                /// <summary>Проверка вызова метода конвертации для заданного значения.</summary>
                /// <param name="valueConverterMock">Мок конвертора используемого тестируемым делегатом.</param>
                public void VerifyConvert(Mock<IValueConverter> valueConverterMock)
                {
                    valueConverterMock
                        .Verify(_convertExpression, Times.Once());
                }
            }
        }
    }
}
