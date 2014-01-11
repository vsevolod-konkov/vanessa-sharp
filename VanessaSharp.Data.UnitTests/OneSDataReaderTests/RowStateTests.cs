using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование экземпляра <see cref="OneSDataReader"/>
    /// в состоянии нахождения на строке данных.
    /// </summary>
    [TestFixture]
    public sealed class RowStateTests : OpenStateTestBase
    {
        /// <summary>Мок для <see cref="IQueryResultSelection"/>.</summary>
        private Mock<IQueryResultSelection> _queryResultSelectionMock;

        /// <summary>Мэнеджер строк.</summary>
        private readonly RowsManager _rowsManager = new RowsManager();

        /// <summary>
        /// Выполнение действий после инициализации <see cref="OpenStateTestBase.QueryResultMock"/>.
        /// </summary>
        protected override void OnAfterInitQueryResultMock()
        {
            _queryResultSelectionMock = CreateQueryResultSelectionMock(_rowsManager);
        }

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected override void ScenarioAfterInitTestedInstance()
        {
            _rowsManager.RowsCount = 1;
            _rowsManager.Reset();
            Assert.IsTrue(TestedInstance.Read());
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public override void TestClose()
        {
            // Arrange
            SetupDispose(_queryResultSelectionMock);
            
            // Arrange - Act - Assert
            base.TestClose();
            
            // Assert
            VerifyDispose(_queryResultSelectionMock);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса имеет одну запись.
        /// </summary>
        [Test]
        public void TestRead()
        {
            // Arrange
            const int TEST_ROWS_COUNT = 10;

            _rowsManager.RowsCount = TEST_ROWS_COUNT;

            // Act & Assert
            for (var rowsCounter = 1; rowsCounter < TEST_ROWS_COUNT; rowsCounter++)
                Assert.IsTrue(TestedInstance.Read());
            
            Assert.IsFalse(TestedInstance.Read());

            // Assert
            _queryResultSelectionMock.Verify(qrs => qrs.Next(), Times.Exactly(TEST_ROWS_COUNT + 1));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        /// <param name="deltaLengthOfArray">Разница между длиной массива и количеством колонок.</param>
        [Test]
        public void TestGetValues([Values(-1, 0, 1)] int deltaLengthOfArray)
        {
            // Arrange
            _rowsManager.RowsCount = 1;
            var rowData = new object[] { "TEST", 12, 23.54 };
            SetupColumnsGetCount(rowData.Length);

            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(It.IsAny<int>()))
                .Returns<int>(i => rowData[i])
                .Verifiable();

            var bufferLength = rowData.Length + deltaLengthOfArray;
            var expectedValues = new object[bufferLength];

            var expectedResult = rowData.Length + Math.Min(deltaLengthOfArray, 0);
            Array.Copy(rowData, expectedValues, expectedResult);

            // Act
            var actualValues = new object[bufferLength];
            var actualResult = TestedInstance.GetValues(actualValues);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
            CollectionAssert.AreEqual(expectedValues, actualValues);

            _queryResultSelectionMock
                .Verify(qrs => qrs.Get(It.IsAny<int>()), Times.Exactly(expectedResult));
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>.
        /// </summary>
        [Test]
        public void TestHasRows()
        {
            Assert.IsTrue(TestedInstance.HasRows);
        }

        /// <summary>
        /// Следует ли вбрасывать исключение
        /// <see cref="InvalidOperationException"/>
        /// в случае попытки получения значения.
        /// </summary>
        protected override bool ShouldBeThrowInvalidOperationExceptionWhenGetValue
        {
            get { return false; }
        }

        /// <summary>Настройка для получения значения.</summary>
        protected override void ArrangeGetValue(string columnName, object returnValue)
        {
            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(columnName))
                .Returns(returnValue)
                .Verifiable();
        }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected override void AssertGetValue(string columnName)
        {
            _queryResultSelectionMock
                .Verify(qrs => qrs.Get(columnName), Times.Once());
        }

        /// <summary>Настройка для получения значения.</summary>
        protected override void ArrangeGetValue(int ordinal, object returnValue)
        {
            base.ArrangeGetValue(ordinal, returnValue);

            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(ordinal))
                .Returns(returnValue)
                .Verifiable();
        }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected override void AssertGetValue(int ordinal)
        {
            _queryResultSelectionMock.Verify(qrs => qrs.Get(ordinal), Times.Once());
        }

        private void ArrangeGetTypedValue<T>(Expression<Func<IValueConverter, T>> setupAction, T expectedResult)
        {
            ValueConverterMock
                .Setup(setupAction)
                .Returns(expectedResult)
                .Verifiable();
        }

        private void AssertGetTypedValue<T>(Expression<Func<IValueConverter, T>> verifyAction)
        {
            ValueConverterMock
                .Verify(verifyAction, Times.Once());
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected override void ArrangeGetString(object returnValue, string expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToString(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetString"/>.
        /// </summary>
        protected override void AssertGetString(object returnValue)
        {
            AssertGetTypedValue(c => c.ToString(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected override void ArrangeGetByte(object returnValue, byte expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToByte(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetByte"/>.
        /// </summary>
        protected override void AssertGetByte(object returnValue)
        {
            AssertGetTypedValue(c => c.ToByte(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        protected override void ArrangeGetInt16(object returnValue, short expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToInt16(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt16"/>.
        /// </summary>
        protected override void AssertGetInt16(object returnValue)
        {
            AssertGetTypedValue(c => c.ToInt16(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        protected override void ArrangeGetInt32(object returnValue, int expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToInt32(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt32"/>.
        /// </summary>
        protected override void AssertGetInt32(object returnValue)
        {
            AssertGetTypedValue(c => c.ToInt32(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        protected override void ArrangeGetInt64(object returnValue, long expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToInt64(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetInt64"/>.
        /// </summary>
        protected override void AssertGetInt64(object returnValue)
        {
            AssertGetTypedValue(c => c.ToInt64(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        protected override void ArrangeGetFloat(object returnValue, float expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToFloat(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetFloat"/>.
        /// </summary>
        protected override void AssertGetFloat(object returnValue)
        {
            AssertGetTypedValue(c => c.ToFloat(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        protected override void ArrangeGetDouble(object returnValue, double expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToDouble(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDouble"/>.
        /// </summary>
        protected override void AssertGetDouble(object returnValue)
        {
            AssertGetTypedValue(c => c.ToDouble(returnValue));
        }
    }
}
