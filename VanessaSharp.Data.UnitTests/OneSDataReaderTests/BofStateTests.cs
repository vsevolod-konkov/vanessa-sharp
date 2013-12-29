using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataReader"/>
    /// в случае когда экземпляр находится в состоянии начала чтения данных.
    /// </summary>
    [TestFixture]
    public sealed class BofStateTests
    {
        /// <summary>Мок для <see cref="IValueTypeConverter"/>.</summary>
        private Mock<IValueTypeConverter> _valueTypeConverterMock; 
        
        /// <summary>Мок для <see cref="IQueryResult"/>.</summary>
        private Mock<IQueryResult> _queryResultMock;

        /// <summary>Мок для <see cref="IQueryResultColumnsCollection"/>.</summary>
        private Mock<IQueryResultColumnsCollection> _queryResultColumnsMock; 

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSDataReader _testedInstance;

        /// <summary>
        /// Установка реализации <see cref="IDisposable.Dispose"/>
        /// для мока.
        /// </summary>
        private static void SetupDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock
                .Setup(o => o.Dispose())
                .Verifiable();
        }

        /// <summary>Создание мока реализующего <see cref="IDisposable"/>.</summary>
        private static Mock<T> CreateDisposableMock<T>()
            where T : class, IDisposable
        {
            var mock = new Mock<T>(MockBehavior.Strict);
            SetupDispose(mock);

            return mock;
        }

        /// <summary>
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у мока.
        /// </summary>
        private static void VerifyDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock.Verify(o => o.Dispose(), Times.AtLeastOnce());
        }

        /// <summary>
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у <see cref="_queryResultColumnsMock"/>.
        /// </summary>
        private void VerifyColumnsDispose()
        {
            VerifyDispose(_queryResultColumnsMock);
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _valueTypeConverterMock = new Mock<IValueTypeConverter>(MockBehavior.Strict);

            _queryResultColumnsMock = CreateDisposableMock<IQueryResultColumnsCollection>();

            _queryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            _queryResultMock
                .SetupGet(r => r.Columns)
                .Returns(_queryResultColumnsMock.Object)
                .Verifiable();

            _testedInstance = new OneSDataReader(_queryResultMock.Object, _valueTypeConverterMock.Object);
        }

        /// <summary>
        /// Тестирование инициализации.
        /// </summary>
        [Test]
        public void TestInit()
        {
            // Assert
            Assert.AreSame(_queryResultMock.Object, _testedInstance.QueryResult);
            Assert.IsFalse(_testedInstance.IsClosed);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public void TestClose()
        {
            // Arrange
            SetupDispose(_queryResultMock);

            // Act
            _testedInstance.Close();

            // Assert
            Assert.IsTrue(_testedInstance.IsClosed);
            VerifyDispose(_queryResultMock);
        }

        /// <summary>
        /// Установка рeализации <see cref="IQueryResultColumnsCollection.Count"/>
        /// у мока <see cref="_queryResultColumnsMock"/>.
        /// </summary>
        /// <param name="columnsCount">Количество колонок.</param>
        private void SetupColumnsGetCount(int columnsCount)
        {
            _queryResultColumnsMock
                .SetupGet(cs => cs.Count)
                .Returns(columnsCount)
                .Verifiable();
        }
        
        /// <summary>Тестирование метода <see cref="OneSDataReader.FieldCount"/>.</summary>
        [Test]
        public void TestFieldCount()
        {
            const int TEST_FIELD_COUNT = 8;

            // Arrange
            SetupColumnsGetCount(TEST_FIELD_COUNT);

            // Act & Assert
            Assert.AreEqual(TEST_FIELD_COUNT, _testedInstance.FieldCount);

            _queryResultMock.VerifyGet(r => r.Columns, Times.Once());
            _queryResultColumnsMock.VerifyGet(cs => cs.Count, Times.Once());

            VerifyColumnsDispose();
        }

        /// <summary>Установка получения колонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        /// <param name="column">Экземпляр получаемой колонки.</param>
        private void SetupGetColumn(int index, IQueryResultColumn column)
        {
            SetupColumnsGetCount(index + 1);

            _queryResultColumnsMock
                .Setup(cs => cs.Get(It.IsAny<int>()))
                .Returns(column)
                .Verifiable();
        }

        /// <summary>Проверка получения полонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        private void VerifyGetColumn(int index)
        {
            _queryResultMock.Verify(r => r.Columns);
            _queryResultColumnsMock.Verify(cs => cs.Get(index), Times.Once());
        }

        /// <summary>Тестирование <see cref="OneSDataReader.GetName"/>.</summary>
        [Test]
        public void TestGetName()
        {
            const string TEST_FIELD_NAME = "Тестовая колонка";
            const int TEST_FIELD_ORDINAL = 3;
            
            // Arrange
            var columnMock = CreateDisposableMock<IQueryResultColumn>();
            columnMock
                .SetupGet(c => c.Name)
                .Returns(TEST_FIELD_NAME)
                .Verifiable();

            SetupGetColumn(TEST_FIELD_ORDINAL, columnMock.Object);

            // Act
            var actualName = _testedInstance.GetName(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(TEST_FIELD_NAME, actualName);

            VerifyGetColumn(TEST_FIELD_ORDINAL);
            columnMock.VerifyGet(c => c.Name, Times.Once());

            VerifyColumnsDispose();
            VerifyDispose(columnMock);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFieldType"/>.
        /// </summary>
        [Test]
        public void TestGetFieldType()
        {
            const int TEST_FIELD_ORDINAL = 3;

            // Arrange
            var valueTypeMock = CreateDisposableMock<IValueType>();

            var columnMock = CreateDisposableMock<IQueryResultColumn>();
            columnMock
                .SetupGet(c => c.ValueType)
                .Returns(valueTypeMock.Object)
                .Verifiable();

            SetupGetColumn(TEST_FIELD_ORDINAL, columnMock.Object);

            var expectedType = typeof(decimal);
            _valueTypeConverterMock
                .Setup(c => c.ConvertFrom(It.IsAny<IValueType>()))
                .Returns(expectedType)
                .Verifiable();

            // Act
            var actualType = _testedInstance.GetFieldType(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(expectedType, actualType);

            VerifyGetColumn(TEST_FIELD_ORDINAL);
            columnMock.VerifyGet(c => c.ValueType, Times.Once());
            _valueTypeConverterMock.Verify(c => c.ConvertFrom(valueTypeMock.Object), Times.Once());

            VerifyColumnsDispose();
            VerifyDispose(columnMock);
            VerifyDispose(valueTypeMock);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса данных пуст.
        /// </summary>
        [Test]
        public void TestReadWhenEmptyData()
        {
            // Arrange
            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(true)
                .Verifiable();

            // Act & Assert
            Assert.IsFalse(_testedInstance.Read());

            // Act & Assert
            Assert.IsFalse(_testedInstance.Read());

            // Assert
            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса имеет одну запись.
        /// </summary>
        [Test]
        public void TestReadWhenOneRow()
        {
            // Arrange
            const int TEST_ROWS_COUNT = 1;
            var rowIndex = -1;

            var queryResultSelectionMock = new Mock<IQueryResultSelection>(MockBehavior.Strict);
            queryResultSelectionMock
                .Setup(qrs => qrs.Next())
                .Returns(() =>
                    {
                        ++rowIndex;
                        return rowIndex < TEST_ROWS_COUNT;
                    });

            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(false)
                .Verifiable();
            _queryResultMock
                .Setup(qr => qr.Choose())
                .Returns(queryResultSelectionMock.Object)
                .Verifiable();

            // Act & Assert
            // 1
            Assert.IsTrue(_testedInstance.Read());
            // 2
            Assert.IsFalse(_testedInstance.Read());

            // Assert
            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
            _queryResultMock.Verify(qr => qr.Choose(), Times.Once());
            queryResultSelectionMock.Verify(qrs => qrs.Next(), Times.Exactly(2));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            var result = _testedInstance.GetValues(new object[10]);
        }

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public void TestDepth()
        {
            Assert.AreEqual(0, _testedInstance.Depth);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>
        /// в случае когда нет строк в результате запроса.
        /// </summary>
        [Test]
        public void TestHasRows([Values(false, true)] bool expectedHasRows)
        {
            // Arrange
            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(!expectedHasRows)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(expectedHasRows, _testedInstance.HasRows);

            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(int)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByIndex()
        {
            // Arrange
            const int TEST_FIELD_INDEX = 5;
            
            _queryResultColumnsMock
                .Setup(cs => cs.Count)
                .Returns(TEST_FIELD_INDEX + 1);

            // Act & Assert
            var value = _testedInstance[TEST_FIELD_INDEX];
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(string)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByName()
        {
            // Act & Assert
            var value = _testedInstance["TEST_FIELD"];
        }

        /// <summary>Тестирование <see cref="OneSDataReader.RecordsAffected"/>.</summary>
        [Test]
        public void TestRecordsAffected()
        {
            Assert.AreEqual(-1, _testedInstance.RecordsAffected);
        }
    }
}
