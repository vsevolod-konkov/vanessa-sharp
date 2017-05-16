using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

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

        /// <summary>
        /// Интерфейс тестировщика поля, независимый от типа поля.
        /// </summary>
        internal interface IFieldTester<in T>
        {
            void Test(T testedItem);

            void VerifyConvert(Mock<IValueConverter> valueConverterMock);
        }

        /// <summary>
        /// Интерфейс для получения значения.
        /// </summary>
        /// <remarks>
        /// Нужен только для экранирования метода получения значения внутри класса-владельца.
        /// </remarks>
        private interface IValueProvider
        {
            object Value { get; }
        }

        /// <summary>Базовый класс построения теста.</summary>
        /// <typeparam name="T">Тип данных в тесте.</typeparam>
        public abstract class TestBuilderBase<T>
        {
            /// <summary>Буфер значений используемых тестируемым делегатом.</summary>
            private readonly object[] _values;

            /// <summary>Тестировщики полей вычитанного элемента.</summary>
            private readonly IFieldTester<T>[] _fieldTesters;

            /// <summary>Мок конвертера значений.</summary>
            private readonly Mock<IValueConverter> _valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            /// <summary>Конструктор.</summary>
            /// <param name="fieldsCount">Количество читаемых полей.</param>
            protected TestBuilderBase(int fieldsCount)
            {
                Contract.Requires<ArgumentException>(fieldsCount > 0);
                
                _values = Enumerable.Range(0, fieldsCount).Select(i => new object()).ToArray();
                _fieldTesters = new IFieldTester<T>[fieldsCount];
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
            protected void InternalField<TValue>(int valueIndex, Func<T, TValue> fieldAccessor, Expression<Func<IValueConverter, TValue>> convertMethod, TValue expectedValue)
            {
                InternalField(valueIndex, fieldAccessor, convertMethod, expectedValue, expectedValue);
            }

            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <typeparam name="TRawValue">Тип значения колонки.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="rawValue">Сырое значение колонки.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            protected void InternalField<TRawValue, TValue>(int valueIndex, Func<T, TValue> fieldAccessor, Expression<Func<IValueConverter, TRawValue>> convertMethod, TRawValue rawValue, TValue expectedValue)
            {
                Contract.Requires<ArgumentOutOfRangeException>(valueIndex >= 0 && valueIndex < FieldsCount);
                Contract.Requires<ArgumentNullException>(fieldAccessor != null);
                Contract.Requires<ArgumentNullException>(convertMethod != null);

                var value = _values[valueIndex];
                var substitutedConvertMethod = ConverterTester.SubstituteConvertValue(convertMethod, value);

                // Инициализация мока конвертера вызова метода с заданным аргументом из буфера
                _valueConverterMock
                    .Setup(substitutedConvertMethod)
                    .Returns(rawValue);

                _fieldTesters[valueIndex] = new FieldTester<T, TRawValue, TValue>(fieldAccessor, expectedValue, substitutedConvertMethod);
            }

            /// <summary>
            /// Проверка перед запуском теста.
            /// </summary>
            protected void OnBeforeTest()
            {
                // Проверка что все поля проинициализированы для теста.
                for (var valueIndex = 0; valueIndex < _fieldTesters.Length; valueIndex++)
                {
                    Contract.Assert(_fieldTesters[valueIndex] != null,
                        string.Format("Для индекса буфера значений \"{0}\" не проинициализировано поле для тестирования", valueIndex));
                }
            }

            /// <summary>Тело теста.</summary>
            /// <param name="testedItem">Тестируемый элемент данных.</param>
            protected void InternalTest(T testedItem)
            {
                Assert.IsNotNull(testedItem);

                foreach (var fieldTester in _fieldTesters)
                    fieldTester.Test(testedItem);

                foreach (var fieldTester in _fieldTesters)
                    fieldTester.VerifyConvert(_valueConverterMock);
            }

            /// <summary>Установка значения.</summary>
            protected void SetValue(int valueIndex, object value)
            {
                _values[valueIndex] = value;
            }

            /// <summary>Установка тестировщика поля.</summary>
            protected void SetFieldTester(int valueIndex, IFieldTester<T> fieldTester)
            {
                _fieldTesters[valueIndex] = fieldTester;
            }

            protected IValueConverter ValueConverter
            {
                get { return _valueConverterMock.Object; }
            }

            protected void GetValues(object[] buffer)
            {
                Array.Copy(_values, buffer, FieldsCount);
            }
        }

        /// <summary>Построитель теста для конкретного типа читаемых элементов.</summary>
        /// <typeparam name="T">Тип читаемых элементов.</typeparam>
        internal sealed class TestBuilder<T> : TestBuilderBase<T>
        {
            /// <summary>Тестируемый делегат чтения элементов данных.</summary>
            private readonly Func<IValueConverter, object[], T> _testedItemReader;

            /// <summary>Конструктор.</summary>
            /// <param name="testedItemReader">Тестируемый делегат чтения элементов данных.</param>
            /// <param name="fieldsCount">Количество читаемых полей.</param>
            public TestBuilder(Func<IValueConverter, object[], T> testedItemReader, int fieldsCount)
                : base(fieldsCount)
            {
                Contract.Requires<ArgumentNullException>(testedItemReader != null);
                Contract.Requires<ArgumentException>(fieldsCount > 0);
                
                _testedItemReader = testedItemReader;
            }

            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            public TestBuilder<T> Field<TValue>(int valueIndex, Func<T, TValue> fieldAccessor, Expression<Func<IValueConverter, TValue>> convertMethod, TValue expectedValue)
            {
                InternalField(valueIndex, fieldAccessor, convertMethod, expectedValue);

                return this;
            }

            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <typeparam name="TRawValue">Тип значения колонки.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="rawValue">Сырое значение колонки.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            public TestBuilder<T> Field<TRawValue, TValue>(int valueIndex, Func<T, TValue> fieldAccessor, Expression<Func<IValueConverter, TRawValue>> convertMethod, TRawValue rawValue, TValue expectedValue)
            {
                Contract.Requires<ArgumentOutOfRangeException>(valueIndex >= 0 && valueIndex < FieldsCount);
                Contract.Requires<ArgumentNullException>(fieldAccessor != null);
                Contract.Requires<ArgumentNullException>(convertMethod != null);

                InternalField(valueIndex, fieldAccessor, convertMethod, rawValue, expectedValue);

                return this;
            }

            /// <summary>Начало описания тестирования чтения табличной части.</summary>
            /// <typeparam name="TTablePartItem">Тип элемента табличной части.</typeparam>
            /// <param name="valueIndex">Индекс поля.</param>
            /// <param name="tablePartAccessor">Делегат доступа к перечислению элементов табличной части.</param>
            /// <param name="tablePartFieldsCount">Количество полей в табличной части.</param>
            public TablePartTestBuilder<T, TTablePartItem> BeginTablePart<TTablePartItem>(
                int valueIndex, Func<T, IEnumerable<TTablePartItem>> tablePartAccessor, int tablePartFieldsCount)
            {
                Contract.Requires<ArgumentException>(valueIndex >= 0 && valueIndex < FieldsCount);
                Contract.Requires<ArgumentNullException>(tablePartAccessor != null);
                Contract.Requires<ArgumentException>(tablePartFieldsCount > 0);
                Contract.Ensures(Contract.Result<TablePartTestBuilder<T, TTablePartItem>>() != null);
                
                var tablePartTestBuilder = new TablePartTestBuilder<T, TTablePartItem>(this, tablePartAccessor,
                                                                                       tablePartFieldsCount);

                IValueProvider valueProvider = tablePartTestBuilder;

                SetValue(valueIndex, valueProvider.Value);
                SetFieldTester(valueIndex, tablePartTestBuilder);

                return tablePartTestBuilder;
            }

            /// <summary>
            /// Запуск теста делегата.
            /// </summary>
            public void Test()
            {
                OnBeforeTest();
                
                // Выполнение запуска делегата
                var testedItem = GetTestedItem();

                InternalTest(testedItem);
            }

            private T GetTestedItem()
            {
                var buffer = new object[FieldsCount];
                GetValues(buffer);

                return _testedItemReader(ValueConverter, buffer);
            }
        }

        /// <summary>
        /// Построитель тестирования элемента табличной части.
        /// </summary>
        /// <typeparam name="T">Тип элемента данных верхнего уровня.</typeparam>
        /// <typeparam name="TTablePartItem">Тип элемента табличной части.</typeparam>
        internal sealed class TablePartTestBuilder<T, TTablePartItem>
            : TestBuilderBase<TTablePartItem>, IFieldTester<T>, IValueProvider
        {
            private readonly TestBuilder<T> _rootTestBuilder;
            private readonly Func<T, IEnumerable<TTablePartItem>> _tablePartAccessor;
            private readonly ISqlResultReader _sqlResultReader;

            /// <summary>Конструктор.</summary>
            /// <param name="rootTestBuilder">Построитель теста верхнего уровня.</param>
            /// <param name="tablePartAccessor">Делегат доступа к последовательности элементов табличной части.</param>
            /// <param name="fieldsCount">Количество полей в табличной части.</param>
            internal TablePartTestBuilder(TestBuilder<T> rootTestBuilder, Func<T, IEnumerable<TTablePartItem>> tablePartAccessor, int fieldsCount)
                : base(fieldsCount)
            {
                _rootTestBuilder = rootTestBuilder;
                _tablePartAccessor = tablePartAccessor;

                _sqlResultReader = InitSqlResultReader();
            }

            private ISqlResultReader InitSqlResultReader()
            {
                var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
                sqlResultReaderMock
                    .Setup(r => r.Dispose())
                    .Verifiable();
                sqlResultReaderMock
                    .Setup(r => r.FieldCount)
                    .Returns(FieldsCount);
                sqlResultReaderMock
                    .Setup(r => r.ValueConverter)
                    .Returns(ValueConverter);

                var counter = 0;
                sqlResultReaderMock
                    .Setup(r => r.Read())
                    .Returns(() => ++counter == 1)
                    .Verifiable();
                sqlResultReaderMock
                    .Setup(r => r.GetValues(It.IsAny<object[]>()))
                    .Callback<object[]>(GetValues)
                    .Verifiable();

                return sqlResultReaderMock.Object;
            }

            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <typeparam name="TRawValue">Тип значения колонки.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="rawValue">Сырое значение колонки.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            public TablePartTestBuilder<T, TTablePartItem> Field<TRawValue, TValue>(int valueIndex,
                                                                             Func<TTablePartItem, TValue> fieldAccessor,
                                                                             Expression<Func<IValueConverter, TRawValue>> convertMethod,
                                                                             TRawValue rawValue,
                                                                             TValue expectedValue)
            {
                InternalField(valueIndex, fieldAccessor, convertMethod, rawValue, expectedValue);

                return this;
            }

            /// <summary>Инициализация тестирования поля.</summary>
            /// <typeparam name="TValue">Тип значения поля.</typeparam>
            /// <param name="valueIndex">Индекс значения в буфере данных.</param>
            /// <param name="fieldAccessor">Получатель значения поля из вычитанного элемента.</param>
            /// <param name="convertMethod">Выражение используемого метода конвертации.</param>
            /// <param name="expectedValue">Ожидаемое значение элемента.</param>
            public TablePartTestBuilder<T, TTablePartItem> Field<TValue>(int valueIndex,
                   Func<TTablePartItem, TValue> fieldAccessor,
                   Expression<Func<IValueConverter, TValue>> convertMethod, TValue expectedValue)
            {
                InternalField(valueIndex, fieldAccessor, convertMethod, expectedValue);

                return this;
            }

            /// <summary>
            /// Завершение описания тестирования табличной части.
            /// </summary>
            public TestBuilder<T> EndTablePart
            {
                get { return _rootTestBuilder; }
            }

            private TTablePartItem GetTestedItem(T rootData)
            {
                // Выполнение запуска делегата
                var tablePartEnumerable = _tablePartAccessor(rootData);

                Assert.IsNotNull(tablePartEnumerable);

                using (var tablePartEnumerator = tablePartEnumerable.GetEnumerator())
                {
                    Assert.IsNotNull(tablePartEnumerator);
                    Assert.IsTrue(tablePartEnumerator.MoveNext());

                    return tablePartEnumerator.Current;
                }
            }

            void IFieldTester<T>.Test(T rootData)
            {
                OnBeforeTest();

                var testedItem = GetTestedItem(rootData);

                InternalTest(testedItem);
            }

            void IFieldTester<T>.VerifyConvert(Mock<IValueConverter> valueConverterMock)
            {}

            object IValueProvider.Value
            {
                get { return _sqlResultReader; }
            }
        }

        /// <summary>
        /// Тестировщик поля для конкретного типа поля.
        /// </summary>
        /// <typeparam name="TRawValue">Тип колонки.</typeparam>
        /// <typeparam name="TValue">Тип значения поля.</typeparam>
        /// <typeparam name="T">Тип тестируемого элемента</typeparam>
        private sealed class FieldTester<T, TRawValue, TValue> : IFieldTester<T>
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
            private readonly Expression<Func<IValueConverter, TRawValue>> _convertExpression;

            public FieldTester(Func<T, TValue> fieldAccessor, TValue expectedValue, Expression<Func<IValueConverter, TRawValue>> convertExpression)
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
