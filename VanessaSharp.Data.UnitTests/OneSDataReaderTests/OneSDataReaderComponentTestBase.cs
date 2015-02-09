using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для компонентных тестов над
    /// <see cref="OneSDataReader"/>.
    /// </summary>
    public abstract class OneSDataReaderComponentTestBase
    {
        /// <summary>
        /// Является ли тестируемый экземпляр - читателем табличной части.
        /// </summary>
        private const bool IS_TABLE_PART = false;

        #region Вспомогательные методы и типы

        /// <summary>Менеджер строк тестового экземпляра.</summary>
        internal sealed class RowsManager
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
        /// Создание мока курсора <see cref="IDataCursor"/>.
        /// </summary>
        /// <param name="dataRecordsProviderMock"><see cref="IDataRecordsProvider"/></param>
        /// <param name="rowsManager">Менеджер строк данных.</param>
        private static DisposableMock<IDataCursor> CreateDataCursorMock(
            Mock<IDataRecordsProvider> dataRecordsProviderMock, 
            RowsManager rowsManager)
        {
            var dataCursorMock = new DisposableMock<IDataCursor>();
            dataCursorMock
                .Setup(s => s.Next())
                .Returns(() =>
                {
                    rowsManager.Next();
                    return !rowsManager.IsEof;
                });


            var hasRecords = rowsManager.RowsCount > 0;

            dataRecordsProviderMock
                .Setup(d => d.HasRecords)
                .Returns(hasRecords);

            if (hasRecords)
            {
                IDataCursor dataCursor = dataCursorMock.Object;
                dataRecordsProviderMock
                    .Setup(d => d.TryCreateCursor(out dataCursor))
                    .Returns(true);
            }
            else
            {
                IDataCursor nullDataCursor = null;
                dataRecordsProviderMock
                    .Setup(d => d.TryCreateCursor(out nullDataCursor))
                    .Returns(false);
            }

            return dataCursorMock;
        }

        /// <summary>
        /// Создание мока курсора <see cref="IDataCursor"/>.
        /// </summary>
        /// <param name="rowsManager">Менеджер строк данных.</param>
        internal DisposableMock<IDataCursor> CreateDataCursorMock(
            RowsManager rowsManager)
        {
            return CreateDataCursorMock(DataRecordsProviderMock, rowsManager);
        }

        /// <summary>
        /// Создание мока курсора <see cref="IDataCursor"/>.
        /// </summary>
        /// <param name="rowsCount">Количество строк.</param>
        internal DisposableMock<IDataCursor> CreateDataCursorMock(
            int rowsCount = 1)
        {
            return CreateDataCursorMock(new RowsManager { RowsCount = rowsCount });
        }

        #endregion

        /// <summary>Мок для <see cref="IDataRecordsProvider"/>.</summary>
        internal DisposableMock<IDataRecordsProvider> DataRecordsProviderMock { get; private set; }

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSDataReader TestedInstance { get; private set; }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            // Создание тестовых экземпляров и моков
            var fieldInfoCollection = CreateDataReaderFieldInfoCollectionMock().Object;
            DataRecordsProviderMock = new DisposableMock<IDataRecordsProvider>();
            DataRecordsProviderMock
                .Setup(d => d.Fields)
                .Returns(fieldInfoCollection);

            //
            SetUpData();

            TestedInstance = new OneSDataReader(
                DataRecordsProviderMock.Object,
                CreateValueConverter(),
                IS_TABLE_PART,
                null);

            ScenarioAfterInitTestedInstance();
        }

        /// <summary>Инициализация данных.</summary>
        protected virtual void SetUpData() {}

        /// <summary>
        /// Создание мока тестового экземпляра <see cref="IDataReaderFieldInfoCollection"/>.
        /// </summary>
        internal virtual Mock<IDataReaderFieldInfoCollection> CreateDataReaderFieldInfoCollectionMock()
        {
            return new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);
        }

        /// <summary>Создание тестового экземпляра <see cref="IValueConverter"/>.</summary>
        internal abstract IValueConverter CreateValueConverter();

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected virtual void ScenarioAfterInitTestedInstance() {}

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsTablePart"/>.
        /// </summary>
        [Test]
        public void TestIsTablePart()
        {
            Assert.AreEqual(IS_TABLE_PART, TestedInstance.IsTablePart);   
        }

        /// <summary>Тестирование свойства записи.</summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="testedProperty">Тестируемое свойство.</param>
        /// <param name="dataCursorProperty">
        /// Свойство <see cref="IDataCursor"/>
        /// соответствующее тестируемому свойству.
        /// </param>
        /// <param name="expectedValue">Ожидаемое значение.</param>
        internal virtual void TestRecordProperty<T>(
            Func<OneSDataReader, T> testedProperty,
            Expression<Func<IDataCursor, T>> dataCursorProperty, 
            T expectedValue)
        {
            Assert.Throws<InvalidOperationException>(
                () => { var result = testedProperty(TestedInstance); });
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Level"/>.
        /// </summary>
        [Test]
        public virtual void TestLevel()
        {
            const int EXPECTED_LEVEL = 4;
            
            TestRecordProperty(
                r => r.Level, 
                c => c.Level, 
                EXPECTED_LEVEL);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.GroupName"/>.
        /// </summary>
        [Test]
        public virtual void TestGroupName()
        {
            const string EXPECTED_GROUP_NAME = "TestGroup";
            
            TestRecordProperty(
                r => r.GroupName,
                c => c.GroupName,
                EXPECTED_GROUP_NAME);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.RecordType"/>.
        /// </summary>
        [Test]
        public virtual void TestRecordType()
        {
            const SelectRecordType EXPECTED_RECORD_TYPE = SelectRecordType.TotalByHierarchy;
            
            TestRecordProperty(
                r => r.RecordType,
                c => c.RecordType,
                EXPECTED_RECORD_TYPE);
        }

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public virtual void TestDepth()
        {
            Assert.Throws<InvalidOperationException>(
                () => { var depth = TestedInstance.Depth; });
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
        /// Тестирование <see cref="OneSDataReader.GetEnumerator"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestNotImplementedGetEnumerator()
        {
            var enumerator = TestedInstance.GetEnumerator();
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

        private void TestGetValue<T>(int ordinal, Func<OneSDataReader, int, T> testedAction, T expectedResult)
        {
            Func<T> testedFunc = () => ArrangeAndGetValue(testedAction, ordinal, expectedResult);

            if (ShouldBeThrowInvalidOperationExceptionWhenGetValue)
            {
                Assert.Throws<InvalidOperationException>(() => testedFunc());
            }
            else
            {
                var actualResult = testedFunc();
                if (!ReferenceEquals(expectedResult, actualResult))
                    Assert.AreEqual(expectedResult, actualResult);

                AssertGetValue(ordinal);
            }
        }

        private void TestGetValue(Func<OneSDataReader, int, object> testedAction)
        {
            const int TEST_ORDINAL = 5;
            TestGetValue(TEST_ORDINAL, testedAction, "Test");
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
            Action<int, object, T> arrange,
            Func<OneSDataReader, int, T> testedAction,
            Action<int, object> assert)
        {
            const int TEST_ORDINAL = 5;
            
            object returnValue = expectedResult;
            arrange(TEST_ORDINAL, returnValue, expectedResult);

            TestGetValue(TEST_ORDINAL, testedAction, expectedResult);

            if (!ShouldBeThrowInvalidOperationExceptionWhenGetValue)
                assert(TEST_ORDINAL, returnValue);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected virtual void ArrangeGetString(int ordinal, object returnValue, string expectedResult)
        {}

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected virtual void AssertGetString(int ordinal, object returnValue)
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
        /// Подготовка для тестирования <see cref="OneSDataReader.GetChar"/>.
        /// </summary>
        protected virtual void ArrangeGetChar(int ordinal, object returnValue, char expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetChar"/>.
        /// </summary>
        protected virtual void AssertGetChar(int ordinal, object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetChar"/>.
        /// </summary>
        [Test]
        public void TestGetChar()
        {
            TestGetTypedValue(
                'A',
                ArrangeGetChar,
                (reader, i) => reader.GetChar(i),
                AssertGetChar);
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected virtual void ArrangeGetByte(int ordinal, object returnValue, byte expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected virtual void AssertGetByte(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetInt16(int ordinal, object returnValue, short expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        protected virtual void AssertGetInt16(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetInt32(int ordinal, object returnValue, int expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        protected virtual void AssertGetInt32(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetInt64(int ordinal, object returnValue, long expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        protected virtual void AssertGetInt64(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetFloat(int ordinal, object returnValue, float expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        protected virtual void AssertGetFloat(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetDouble(int ordinal, object returnValue, double expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        protected virtual void AssertGetDouble(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetDecimal(int ordinal, object returnValue, decimal expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        protected virtual void AssertGetDecimal(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetBoolean(int ordinal, object returnValue, bool expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        protected virtual void AssertGetBoolean(int ordinal, object returnValue)
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
        protected virtual void ArrangeGetDateTime(int ordinal, object returnValue, DateTime expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        protected virtual void AssertGetDateTime(int ordinal, object returnValue)
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

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetGuid"/>
        /// </summary>
        [Test]
        public void TestGetGuid()
        {
            TestGetValue(3, (r, i) => r.GetGuid(i), Guid.NewGuid());
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.IsDBNull"/>.</summary>
        [Test]
        public void TestIsDbNull([Values(false, true)] bool expectedResult)
        {
            const int TEST_ORDINAL = 5;
            
            // В случае если возвращается null из 1С,
            // то метод должен вернуть true, в ином случае false
            var returnValue = expectedResult
                                  ? DBNull.Value
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

        /// <summary>Тестирование отсутствия реализации получения типизированного значения.</summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="expectedValue">Ожидаемое значение.</param>
        /// <param name="testedAction">Тестируемое действие.</param>
        private void TestNotImplementedGetTypedValue<T>(T expectedValue, Action<OneSDataReader, int> testedAction)
        {
            const int TEST_ORDINAL = 5;
            ArrangeGetValue(TEST_ORDINAL, expectedValue);

            var exceptionType = ShouldBeThrowInvalidOperationExceptionWhenGetValue
                                    ? typeof(InvalidOperationException)
                                    : typeof(NotImplementedException);

            Assert.Throws(exceptionType,
                          () => testedAction(TestedInstance, TEST_ORDINAL));
        }

        /// <summary>Тестирование того, что не реализовано потоковое чтение значения поля.</summary>
        /// <typeparam name="T">Тип потоковых данных.</typeparam>
        /// <param name="expectedStream">Ожидаемые потоковые данные</param>
        /// <param name="streamReader">Тестируемый метод потокового чтения.</param>
        private void TestNotImplementedStreamReading<T>(
            IEnumerable<T> expectedStream, Action<OneSDataReader, int, long, T[], int, int> streamReader)
        {
            TestNotImplementedGetTypedValue(expectedStream,
                                            (reader, ordinal) =>
                                            {
                                                const int BUFFER_SIZE = 1024;
                                                var buffer = new T[BUFFER_SIZE];

                                                streamReader(reader, ordinal, 0, buffer, 0, BUFFER_SIZE);
                                            }
                );
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

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDataReader"/>.
        /// </summary>
        protected virtual void ArrangeGetDataReader(int ordinal, object returnValue, OneSDataReader expectedResult)
        { }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDataReader"/>.
        /// </summary>
        protected virtual void AssertGetDataReader(int ordinal, object returnValue)
        { }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetDataReader"/>.
        /// </summary>
        [Test]
        public void TestGetDataReader()
        {
            var tablePartQueryResult = new Mock<IQueryResult>(MockBehavior.Strict).Object;
            var expectedResult = OneSDataReader.CreateTablePartDataReader(tablePartQueryResult);

            Assert.IsTrue(expectedResult.IsTablePart);

            TestGetTypedValue(
                expectedResult,
                ArrangeGetDataReader,
                (reader, ordinal) => reader.GetDataReader(ordinal),
                AssertGetDataReader
                );
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetDescendantsReader(VanessaSharp.Proxy.Common.QueryResultIteration)"/>.
        /// </summary>
        [Test]
        public virtual void TestGetDescendantsReader()
        {
            const QueryResultIteration QUERY_RESULT_ITERATION = QueryResultIteration.ByGroupsWithHierarchy;
            
            // Act
            Assert.Throws<InvalidOperationException>(() =>
                {
                    var result = TestedInstance.GetDescendantsReader(QUERY_RESULT_ITERATION);
                });
        }
    }
}
