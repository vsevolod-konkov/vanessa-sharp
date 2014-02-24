using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для компонентных тестов над
    /// <see cref="OneSDataReader"/>.
    /// </summary>
    public abstract class OneSDataReaderComponentTestBase
    {
        #region Вспомогательные статические методы

        // TODO: CopyPaste Необходимо выделить во вспомогательный класс
        /// <summary>
        /// Установка реализации <see cref="IDisposable.Dispose"/>
        /// для мока.
        /// </summary>
        protected static void SetupDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock
                .Setup(o => o.Dispose())
                .Verifiable();
        }

        // TODO: CopyPaste Необходимо выделить во вспомогательный класс
        /// <summary>Создание мока реализующего <see cref="IDisposable"/>.</summary>
        protected static Mock<T> CreateDisposableMock<T>()
            where T : class, IDisposable
        {
            var mock = new Mock<T>(MockBehavior.Strict);
            SetupDispose(mock);

            return mock;
        }

        // TODO: CopyPaste Необходимо выделить во вспомогательный класс
        /// <summary>
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у мока.
        /// </summary>
        protected static void VerifyDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock.Verify(o => o.Dispose(), Times.AtLeastOnce());
        }

        /// <summary>Менеджер строк тестового экземпляра.</summary>
        protected sealed class RowsManager
        {
            /// <summary>Индекс строки.</summary>
            private int _rowIndex;

            public RowsManager()
            {
                Reset();
                RowsCount = 1;
            }

            /// <summary>Количество строк.</summary>
            public int RowsCount { get; set; }

            /// <summary>Достигнут ли конец в коллекции строк.</summary>
            public bool IsEof
            {
                get { return !(_rowIndex < RowsCount); }
            }

            /// <summary>Переход на следующую строку.</summary>
            public void Next()
            {
                _rowIndex++;
            }

            /// <summary>Сброс. Перевод в начало.</summary>
            public void Reset()
            {
                _rowIndex = -1;
            }
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected static Mock<IQueryResultSelection> CreateQueryResultSelectionMock(Mock<IQueryResult> queryResultMock, RowsManager rowsManager)
        {
            var queryResultSelectionMock = new Mock<IQueryResultSelection>(MockBehavior.Strict);
            queryResultSelectionMock
                .Setup(s => s.Next())
                .Returns(() =>
                {
                    rowsManager.Next();
                    return !rowsManager.IsEof;
                })
                .Verifiable();

            queryResultMock
                .Setup(r => r.IsEmpty())
                .Returns(false);
            queryResultMock
                .Setup(r => r.Choose())
                .Returns(queryResultSelectionMock.Object);

            return queryResultSelectionMock;
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected static Mock<IQueryResultSelection> CreateQueryResultSelectionMock(Mock<IQueryResult> queryResultMock)
        {
            return CreateQueryResultSelectionMock(queryResultMock, new RowsManager());
        }

        #endregion

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSDataReader TestedInstance { get; private set; }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            TestedInstance = new OneSDataReader(
                CreateQueryResult(),  
                CreateValueTypeConverter(),
                CreateValueConverter());

            ScenarioAfterInitTestedInstance();
        }

        /// <summary>Создание тестового экземпляра <see cref="IQueryResult"/>.</summary>
        protected abstract IQueryResult CreateQueryResult();

        /// <summary>Создание тестового экземпляра <see cref="ITypeDescriptionConverter"/>.</summary>
        internal abstract ITypeDescriptionConverter CreateValueTypeConverter();

        /// <summary>Создание тестового экземпляра <see cref="IValueConverter"/>.</summary>
        internal abstract IValueConverter CreateValueConverter();

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected virtual void ScenarioAfterInitTestedInstance() {}

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public void TestDepth()
        {
            Assert.AreEqual(0, TestedInstance.Depth);
        }

        /// <summary>Тестирование <see cref="OneSDataReader.RecordsAffected"/>.</summary>
        [Test]
        public void TestRecordsAffected()
        {
            Assert.AreEqual(-1, TestedInstance.RecordsAffected);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetSchemaTable"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestNotImplementedGetSchemaTable()
        {
            TestedInstance.GetSchemaTable();
        }

        /// <summary>
        /// Следует ли вбрасывать исключение
        /// <see cref="InvalidOperationException"/>
        /// в случае попытки получения значения.
        /// </summary>
        protected virtual bool ShouldBeThrowInvalidOperationExceptionWhenGetValue
        {
            get { return true; }
        }

        /// <summary>Настройка для получения значения.</summary>
        protected virtual void ArrangeGetValue(string columnName, object returnValue)
        { }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected virtual void AssertGetValue(string columnName)
        { }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(string)"/>.
        /// </summary>
        [Test]
        public void TestItemByName()
        {
            // Arrange
            const string TEST_COLUMN_NAME = "TestColumn";
            const string TEST_VALUE = "TestValue";
            ArrangeGetValue(TEST_COLUMN_NAME, TEST_VALUE);

            // Act & Assert
            Func<object> testedFunc = () => TestedInstance[TEST_COLUMN_NAME];
            if (ShouldBeThrowInvalidOperationExceptionWhenGetValue)
            {
                Assert.Throws<InvalidOperationException>(() => testedFunc());
            }
            else
            {
                Assert.AreEqual(TEST_VALUE, testedFunc());
                AssertGetValue(TEST_COLUMN_NAME);
            }
        }

        /// <summary>Настройка для получения значения.</summary>
        protected virtual void ArrangeGetValue(int ordinal, object returnValue)
        { }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected virtual void AssertGetValue(int ordinal)
        { }

        /// <summary>
        /// Тестирование получения значения.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="testedAction">Тестируемое действие.</param>
        /// <param name="ordinal">Индекс колонки.</param>
        /// <param name="expectedResult">Ожидаемый результат</param>
        /// <returns>
        /// Полученное значение.
        /// </returns>
        private T ArrangeAndGetValue<T>(Func<OneSDataReader, int, T> testedAction, int ordinal, T expectedResult)
        {
            // Arrange
            ArrangeGetValue(ordinal, expectedResult);
            return testedAction(TestedInstance, ordinal);
        }

        private void TestGetValue<T>(Func<OneSDataReader, int, T> testedAction, T expectedResult)
        {
            const int TEST_ORDINAL = 5;
            Func<T> testedFunc = () => ArrangeAndGetValue(testedAction, TEST_ORDINAL, expectedResult);

            if (ShouldBeThrowInvalidOperationExceptionWhenGetValue)
            {
                Assert.Throws<InvalidOperationException>(() => testedFunc());
            }
            else
            {
                Assert.AreEqual(expectedResult, testedFunc());
                AssertGetValue(TEST_ORDINAL);
            }
        }

        private void TestGetValue(Func<OneSDataReader, int, object> testedAction)
        {
            TestGetValue(testedAction, "Test");
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(int)"/>.
        /// </summary>
        [Test]
        public void TestItemByIndex()
        {
            TestGetValue((reader, ordinal) => reader[ordinal]);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.GetValue"/>.
        /// </summary>
        [Test]
        public void TestGetValue()
        {
            TestGetValue((reader, ordinal) => reader.GetValue(ordinal));
        }

        /// <summary>Тестирование получения типизированного значения.</summary>
        /// <typeparam name="T">Тип получаемого значения.</typeparam>
        /// <param name="expectedResult">Ожидаемый результат.</param>
        /// <param name="arrange">Подготовка теста.</param>
        /// <param name="testedAction">Тестовое действие.</param>
        /// <param name="assert">Проверка вызовов.</param>
        private void TestGetTypedValue<T>(
            T expectedResult,
            Action<object, T> arrange,
            Func<OneSDataReader, int, T> testedAction,
            Action<object> assert)
        {
            object returnValue = expectedResult;
            arrange(returnValue, expectedResult);

            TestGetValue(testedAction, expectedResult);

            if (!ShouldBeThrowInvalidOperationExceptionWhenGetValue)
                assert(returnValue);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected virtual void ArrangeGetString(object returnValue, string expectedResult)
        {}

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected virtual void AssertGetString(object returnValue)
        {}

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        [Test]
        public void TestGetString()
        {
            TestGetTypedValue(
                "Test", 
                ArrangeGetString, 
                (reader, i) => reader.GetString(i), 
                AssertGetString);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected virtual void ArrangeGetByte(object returnValue, byte expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected virtual void AssertGetByte(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        [Test]
        public void TestGetByte()
        {
            TestGetTypedValue(
                (byte)123,
                ArrangeGetByte,
                (reader, i) => reader.GetByte(i),
                AssertGetByte);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        protected virtual void ArrangeGetInt16(object returnValue, short expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        protected virtual void AssertGetInt16(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        [Test]
        public void TestGetInt16()
        {
            TestGetTypedValue(
                (short)12353,
                ArrangeGetInt16,
                (reader, i) => reader.GetInt16(i),
                AssertGetInt16);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        protected virtual void ArrangeGetInt32(object returnValue, int expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        protected virtual void AssertGetInt32(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        [Test]
        public void TestGetInt32()
        {
            TestGetTypedValue(
                12345,
                ArrangeGetInt32,
                (reader, i) => reader.GetInt32(i),
                AssertGetInt32);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        protected virtual void ArrangeGetInt64(object returnValue, long expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        protected virtual void AssertGetInt64(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        [Test]
        public void TestGetInt64()
        {
            TestGetTypedValue(
                123456453634535,
                ArrangeGetInt64,
                (reader, i) => reader.GetInt64(i),
                AssertGetInt64);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        protected virtual void ArrangeGetFloat(object returnValue, float expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        protected virtual void AssertGetFloat(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        [Test]
        public void TestGetFloat()
        {
            TestGetTypedValue(
                4564536363463424254534541.14154F,
                ArrangeGetFloat,
                (reader, i) => reader.GetFloat(i),
                AssertGetFloat);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        protected virtual void ArrangeGetDouble(object returnValue, double expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        protected virtual void AssertGetDouble(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        [Test]
        public void TestGetDouble()
        {
            TestGetTypedValue(
                4564536345355141235631246363634643635252136534534542352153534634634242545345345345345345645322243241.14154153535323436363543553543223455345543532534554552509898009790709809809809890,
                ArrangeGetDouble,
                (reader, i) => reader.GetDouble(i),
                AssertGetDouble);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        protected virtual void ArrangeGetDecimal(object returnValue, decimal expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        protected virtual void AssertGetDecimal(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        [Test]
        public void TestGetDecimal()
        {
            TestGetTypedValue(
                45645363453551412356341.14154153535323436363543553543223455345543532534M,
                ArrangeGetDecimal,
                (reader, i) => reader.GetDecimal(i),
                AssertGetDecimal);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        protected virtual void ArrangeGetBoolean(object returnValue, bool expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        protected virtual void AssertGetBoolean(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        [Test]
        public void TestGetBoolean()
        {
            TestGetTypedValue(
                true,
                ArrangeGetBoolean,
                (reader, i) => reader.GetBoolean(i),
                AssertGetBoolean);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        protected virtual void ArrangeGetDateTime(object returnValue, DateTime expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        protected virtual void AssertGetDateTime(object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        [Test]
        public void TestGetDateTime()
        {
            TestGetTypedValue(
                new DateTime(2029, 03, 14),
                ArrangeGetDateTime,
                (reader, i) => reader.GetDateTime(i),
                AssertGetDateTime);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.IsDBNull"/>.</summary>
        [Test]
        public void TestIsDbNull([Values(false, true)] bool expectedResult)
        {
            const int TEST_ORDINAL = 5;
            
            // В случае если возвращается null из 1С,
            // то метод должен вернуть true, в ином случае false
            var returnValue = expectedResult
                                  ? null
                                  : new object();

            Func<bool> testedFunc = () =>
                {
                    ArrangeGetValue(TEST_ORDINAL, returnValue);
                    return TestedInstance.IsDBNull(TEST_ORDINAL);
                };

            if (ShouldBeThrowInvalidOperationExceptionWhenGetValue)
            {
                Assert.Throws<InvalidOperationException>(() => testedFunc());
            }
            else
            {
                Assert.AreEqual(expectedResult, testedFunc());
                AssertGetValue(TEST_ORDINAL);
            }
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetBytes"/>.
        /// </summary>
        [Test]
        public void TestGetBytes()
        {
            TestNotImplementedStreamReading(new byte[] {0xFF, 0x01, 0x56, 0x67, 0x54},
                (reader, ordinal, dataOffset, buffer, bufferOffset, length) => 
                    reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetChars"/>.
        /// </summary>
        [Test]
        public void TestGetChars()
        {
            TestNotImplementedStreamReading(@"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                (reader, ordinal, dataOffset, buffer, bufferOffset, length) =>
                    reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length));
        }

        /// <summary>Тестирование того, что не реализовано потоковое чтение значения поля.</summary>
        /// <typeparam name="T">Тип потоковых данных.</typeparam>
        /// <param name="expectedStream">Ожидаемые потоковые данные</param>
        /// <param name="streamReader">Тестируемый метод потокового чтения.</param>
        private void TestNotImplementedStreamReading<T>(IEnumerable<T> expectedStream, Action<OneSDataReader, int, long, T[], int, int> streamReader)
        {
            const int TEST_ORDINAL = 5;
            ArrangeGetValue(TEST_ORDINAL, expectedStream);

            const int BUFFER_SIZE = 1024;

            var buffer = new T[BUFFER_SIZE];

            var exceptionType = ShouldBeThrowInvalidOperationExceptionWhenGetValue
                                    ? typeof(InvalidOperationException)
                                    : typeof(NotImplementedException);

            Assert.Throws(exceptionType,
                          () => streamReader(TestedInstance, TEST_ORDINAL, 0, buffer, 0, BUFFER_SIZE));
        }
    }
}
