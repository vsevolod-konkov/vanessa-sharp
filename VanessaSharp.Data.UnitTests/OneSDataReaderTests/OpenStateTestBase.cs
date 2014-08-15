using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра
    /// <see cref="OneSDataReader"/>
    /// находящегося в открытом состоянии. 
    /// </summary>
    public abstract class OpenStateTestBase : OneSDataReaderComponentTestBase
    {
        /// <summary>
        /// Мок для <see cref="IDataReaderFieldInfoCollection"/>.
        /// </summary>
        internal Mock<IDataReaderFieldInfoCollection> DataReaderFieldInfoCollectionMock { get; private set; }

        /// <summary>
        /// Создание тестового экземпляра <see cref="IDataReaderFieldInfoCollection"/>.
        /// </summary>
        internal sealed override IDataReaderFieldInfoCollection CreateDataReaderFieldInfoCollection()
        {
            DataReaderFieldInfoCollectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);

            return DataReaderFieldInfoCollectionMock.Object;
        }

        /// <summary>Мок для <see cref="IValueConverter"/>.</summary>
        internal Mock<IValueConverter> ValueConverterMock { get; private set; }

        /// <summary>Создание тестового экземпляра <see cref="IValueConverter"/>.</summary>
        internal override IValueConverter CreateValueConverter()
        {
            ValueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            return ValueConverterMock.Object;
        }

        /// <summary>Мок для <see cref="IQueryResult"/>.</summary>
        protected Mock<IQueryResult> QueryResultMock { get; private set; }

        /// <summary>
        /// Установка рeализации <see cref="IQueryResultColumnsCollection.Count"/>.
        /// </summary>
        /// <param name="columnsCount">Количество колонок.</param>
        protected void SetupColumnsGetCount(int columnsCount)
        {
            DataReaderFieldInfoCollectionMock
                .SetupGet(c => c.Count)
                .Returns(columnsCount);
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected Mock<IQueryResultSelection> CreateQueryResultSelectionMock(RowsManager rowsManager)
        {
            return CreateQueryResultSelectionMock(QueryResultMock, rowsManager);
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected Mock<IQueryResultSelection> CreateQueryResultSelectionMock()
        {
            return CreateQueryResultSelectionMock(QueryResultMock);
        }

        /// <summary>Создание тестового экземпляра <see cref="IQueryResult"/>.</summary>
        protected sealed override IQueryResult CreateQueryResult()
        {
            QueryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            OnAfterInitQueryResultMock();

            return QueryResultMock.Object;
        }

        /// <summary>
        /// Выполнение действий после инициализации <see cref="QueryResultMock"/>.
        /// </summary>
        protected virtual void OnAfterInitQueryResultMock() {}


        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsClosed"/>.
        /// </summary>
        [Test]
        public void TestIsClosed()
        {
            Assert.IsFalse(TestedInstance.IsClosed);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public virtual void TestClose()
        {
            // Arrange
            SetupDispose(QueryResultMock);

            // Act
            TestedInstance.Close();

            // Assert
            Assert.IsTrue(TestedInstance.IsClosed);
            VerifyDispose(QueryResultMock);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.FieldCount"/>.</summary>
        [Test]
        public void TestFieldCount()
        {
            const int TEST_FIELD_COUNT = 8;

            // Arrange
            SetupColumnsGetCount(TEST_FIELD_COUNT);

            // Act & Assert
            Assert.AreEqual(TEST_FIELD_COUNT, TestedInstance.FieldCount);

            DataReaderFieldInfoCollectionMock.VerifyGet(cs => cs.Count);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.GetDataTypeName"/>.</summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestGetDataTypeName()
        {
            const int TEST_FIELD_INDEX = 4;

            // Arrange
            SetupColumnsGetCount(TEST_FIELD_INDEX + 1);

            // Act & Assert
            var result = TestedInstance.GetDataTypeName(TEST_FIELD_INDEX);
        }

        /// <summary>Установка получения колонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        /// <param name="fieldInfo">Поле читателя.</param>
        private void SetupGetColumn(int index, DataReaderFieldInfo fieldInfo)
        {
            SetupColumnsGetCount(index + 1);

            DataReaderFieldInfoCollectionMock
                .Setup(c => c[index])
                .Returns(fieldInfo);
        }

        /// <summary>Проверка получения полонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        private void VerifyGetColumn(int index)
        {
            DataReaderFieldInfoCollectionMock.Verify(c => c[index]);
        }

        /// <summary>Тестирование <see cref="OneSDataReader.GetName"/>.</summary>
        [Test]
        public void TestGetName()
        {
            const string TEST_FIELD_NAME = "Тестовая колонка";
            const int TEST_FIELD_ORDINAL = 3;

            // Arrange
            var fieldInfo = new DataReaderFieldInfo(TEST_FIELD_NAME, typeof(object));

            SetupGetColumn(TEST_FIELD_ORDINAL, fieldInfo);

            // Act
            var actualName = TestedInstance.GetName(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(TEST_FIELD_NAME, actualName);

            VerifyGetColumn(TEST_FIELD_ORDINAL);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFieldType"/>.
        /// </summary>
        [Test]
        public void TestGetFieldType()
        {
            const int TEST_FIELD_ORDINAL = 3;

            // Arrange
            var expectedType = typeof(decimal);
            var fieldInfo = new DataReaderFieldInfo("[SomeName]", expectedType);
            
            SetupGetColumn(TEST_FIELD_ORDINAL, fieldInfo);

            // Act
            var actualType = TestedInstance.GetFieldType(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(expectedType, actualType);
            
            VerifyGetColumn(TEST_FIELD_ORDINAL);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetOrdinal"/>
        /// в случае если колонка с заданным именем существует.
        /// </summary>
        [Test]
        public void TestGetOrdinalWhenColumnExist()
        {
            // Arrange
            const string TEST_COLUMN_NAME = "Test";
            const int TEST_EXPECTED_ORDINAL = 2;

            SetupColumnsGetCount(TEST_EXPECTED_ORDINAL + 1);

            DataReaderFieldInfoCollectionMock
                .Setup(c => c.IndexOf(TEST_COLUMN_NAME))
                .Returns(TEST_EXPECTED_ORDINAL)
                .Verifiable();
               
            // Act & Assert
            Assert.AreEqual(TEST_EXPECTED_ORDINAL, TestedInstance.GetOrdinal(TEST_COLUMN_NAME));

            DataReaderFieldInfoCollectionMock
                .Verify(c => c.IndexOf(TEST_COLUMN_NAME));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetOrdinal"/>
        /// в случае если колонка с заданным именем не существует.
        /// </summary>
        [Test]
        public void TestGetOrdinalWhenColumnNotExist()
        {
            // Arrange
            const string TEST_COLUMN_NAME = "Test";

            DataReaderFieldInfoCollectionMock
                .Setup(c => c.IndexOf(TEST_COLUMN_NAME))
                .Returns(-1);

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var ordinal = TestedInstance.GetOrdinal(TEST_COLUMN_NAME);
                });

            DataReaderFieldInfoCollectionMock
                .Verify(c => c.IndexOf(TEST_COLUMN_NAME));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.NextResult"/>.
        /// </summary>
        [Test]
        public void TestNextResult()
        {
            // TODO: Пока не реализована возможность работы с пакетными запросами.
            Assert.IsFalse(TestedInstance.NextResult());
        }

        /// <summary>Настройка для получения значения.</summary>
        protected override void ArrangeGetValue(int ordinal, object returnValue)
        {
            SetupColumnsGetCount(ordinal + 1);
        }
    }
}
