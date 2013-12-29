using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>Тестирование <see cref="OneSDataReader"/>.</summary>
    [TestFixture(false, Description = "Когда не было ни одной строки")]
    [TestFixture(true, Description = "Когда была хотя бы одна строка")]
    public sealed class EofStateTests
    {
        /// <summary>Мок для <see cref="IValueTypeConverter"/>.</summary>
        private Mock<IValueTypeConverter> _valueTypeConverterMock; 
        
        /// <summary>Мок для <see cref="IQueryResult"/>.</summary>
        private Mock<IQueryResult> _queryResultMock;

        /// <summary>Мок для <see cref="IQueryResultColumnsCollection"/>.</summary>
        private Mock<IQueryResultColumnsCollection> _queryResultColumnsMock;

        /// <summary>Мок для <see cref="IQueryResultSelection"/>.</summary>
        private Mock<IQueryResultSelection> _queryResultSelectionMock;

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSDataReader _testedInstance;

        /// <summary>Были ли строки.</summary>
        private readonly bool _hasRows;

        /// <summary>Конструктор.</summary>
        /// <param name="hasRows">Наличие строк в результате.</param>
        public EofStateTests(bool hasRows)
        {
            _hasRows = hasRows;
        }

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

            var rowIndex = -1;
            var rowsCount = _hasRows ? 1 : 0;

            _queryResultSelectionMock = new Mock<IQueryResultSelection>(MockBehavior.Strict);
            _queryResultSelectionMock
                .Setup(s => s.Next())
                .Returns(() =>
                {
                    ++rowIndex;
                    return (rowIndex < rowsCount);
                })
                .Verifiable();

            _queryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            _queryResultMock
                .SetupGet(r => r.Columns)
                .Returns(_queryResultColumnsMock.Object)
                .Verifiable();
            _queryResultMock
                .Setup(r => r.IsEmpty())
                .Returns(!_hasRows);
            _queryResultMock
                .Setup(r => r.Choose())
                .Returns(_queryResultSelectionMock.Object);

            _testedInstance = new OneSDataReader(_queryResultMock.Object, _valueTypeConverterMock.Object);

            for (var counter = 0; counter < rowsCount; counter++)
                Assert.IsTrue(_testedInstance.Read());    
            
            Assert.IsFalse(_testedInstance.Read());
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsClosed"/>.
        /// </summary>
        [Test]
        public void TestIsClosed()
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
            SetupDispose(_queryResultSelectionMock);

            // Act
            _testedInstance.Close();

            // Assert
            Assert.IsTrue(_testedInstance.IsClosed);
            VerifyDispose(_queryResultMock);
            if (_hasRows) 
                VerifyDispose(_queryResultSelectionMock);
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
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            // Act
            var result =_testedInstance.GetValues(new object[10]);
        }

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public void TestDepth()
        {
            Assert.AreEqual(0, _testedInstance.Depth);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>.
        /// </summary>
        [Test]
        public void TestHasRows()
        {
            Assert.AreEqual(_hasRows, _testedInstance.HasRows);
            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.AtLeastOnce());
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(int)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByIndex()
        {
            var value = _testedInstance[5];
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(string)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByName()
        {
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
