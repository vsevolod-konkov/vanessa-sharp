using System;
using Moq;
using NUnit.Framework;
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
        /// <summary>Мок для <see cref="ITypeDescriptionConverter"/>.</summary>
        internal Mock<ITypeDescriptionConverter> ValueTypeConverterMock { get; private set; }

        /// <summary>Создание тестового экземпляра <see cref="ITypeDescriptionConverter"/>.</summary>
        internal sealed override ITypeDescriptionConverter CreateValueTypeConverter()
        {
            ValueTypeConverterMock = new Mock<ITypeDescriptionConverter>(MockBehavior.Strict);

            return ValueTypeConverterMock.Object;
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
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у <see cref="QueryResultColumnsMock"/>.
        /// </summary>
        protected void VerifyColumnsDispose()
        {
            VerifyDispose(QueryResultColumnsMock);
        }

        /// <summary>
        /// Установка рeализации <see cref="IQueryResultColumnsCollection.Count"/>
        /// у мока <see cref="QueryResultColumnsMock"/>.
        /// </summary>
        /// <param name="columnsCount">Количество колонок.</param>
        protected void SetupColumnsGetCount(int columnsCount)
        {
            QueryResultColumnsMock
                .SetupGet(cs => cs.Count)
                .Returns(columnsCount)
                .Verifiable();
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

        /// <summary>Мок для <see cref="IQueryResultColumnsCollection"/>.</summary>
        protected Mock<IQueryResultColumnsCollection> QueryResultColumnsMock { get; private set; }

        /// <summary>Создание тестового экземпляра <see cref="IQueryResult"/>.</summary>
        protected sealed override IQueryResult CreateQueryResult()
        {
            QueryResultColumnsMock = CreateDisposableMock<IQueryResultColumnsCollection>();

            QueryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            QueryResultMock
                .SetupGet(r => r.Columns)
                .Returns(QueryResultColumnsMock.Object)
                .Verifiable();

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

            QueryResultMock.VerifyGet(r => r.Columns, Times.Once());
            QueryResultColumnsMock.VerifyGet(cs => cs.Count, Times.Once());

            VerifyColumnsDispose();
        }

        /// <summary>Установка получения колонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        /// <param name="column">Экземпляр получаемой колонки.</param>
        private void SetupGetColumn(int index, IQueryResultColumn column)
        {
            SetupColumnsGetCount(index + 1);

            QueryResultColumnsMock
                .Setup(cs => cs.Get(It.IsAny<int>()))
                .Returns(column)
                .Verifiable();
        }

        /// <summary>Проверка получения полонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        private void VerifyGetColumn(int index)
        {
            QueryResultMock.Verify(r => r.Columns);
            QueryResultColumnsMock.Verify(cs => cs.Get(index), Times.Once());
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
            var actualName = TestedInstance.GetName(TEST_FIELD_ORDINAL);

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
            var valueTypeMock = CreateDisposableMock<ITypeDescription>();

            var columnMock = CreateDisposableMock<IQueryResultColumn>();
            columnMock
                .SetupGet(c => c.ValueType)
                .Returns(valueTypeMock.Object)
                .Verifiable();

            SetupGetColumn(TEST_FIELD_ORDINAL, columnMock.Object);

            var expectedType = typeof(decimal);
            ValueTypeConverterMock
                .Setup(c => c.ConvertFrom(It.IsAny<ITypeDescription>()))
                .Returns(expectedType)
                .Verifiable();

            // Act
            var actualType = TestedInstance.GetFieldType(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(expectedType, actualType);

            VerifyGetColumn(TEST_FIELD_ORDINAL);
            columnMock.VerifyGet(c => c.ValueType, Times.Once());
            ValueTypeConverterMock.Verify(c => c.ConvertFrom(valueTypeMock.Object), Times.Once());

            VerifyColumnsDispose();
            VerifyDispose(columnMock);
            VerifyDispose(valueTypeMock);
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

            var columnMock = CreateDisposableMock<IQueryResultColumn>();
            var column = columnMock.Object;

            SetupColumnsGetCount(TEST_EXPECTED_ORDINAL + 1);

            QueryResultColumnsMock
                .Setup(cs => cs.Find(It.IsAny<string>()))
                .Returns(column)
                .Verifiable();

            QueryResultColumnsMock
                .Setup(cs => cs.IndexOf(It.IsAny<IQueryResultColumn>()))
                .Returns(TEST_EXPECTED_ORDINAL)
                .Verifiable();
               
            // Act & Assert
            Assert.AreEqual(TEST_EXPECTED_ORDINAL, TestedInstance.GetOrdinal(TEST_COLUMN_NAME));

            QueryResultColumnsMock
                .Verify(cs => cs.Find(TEST_COLUMN_NAME), Times.Once());
            QueryResultColumnsMock
                .Verify(cs => cs.IndexOf(column), Times.Once());
            VerifyColumnsDispose();

            VerifyDispose(columnMock);
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

            QueryResultColumnsMock
                .Setup(cs => cs.Find(It.IsAny<string>()))
                .Returns<IQueryResultColumn>(null)
                .Verifiable();

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var ordinal = TestedInstance.GetOrdinal(TEST_COLUMN_NAME);
                });

            QueryResultColumnsMock
                .Verify(cs => cs.Find(TEST_COLUMN_NAME), Times.Once());
            VerifyColumnsDispose();
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
