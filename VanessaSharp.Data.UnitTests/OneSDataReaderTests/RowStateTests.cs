using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование экземпляра <see cref="OneSDataReader"/>
    /// в состоянии нахождения на строке данных.
    /// </summary>
    [TestFixture]
    public sealed class RowStateTests : OpenStateTestBase
    {
        /// <summary>Мок курсора данных.</summary>
        private DisposableMock<IDataCursor> _dataCursorMock;

        /// <summary>Мэнеджер строк.</summary>
        private readonly RowsManager _rowsManager = new RowsManager();

        /// <summary>Инициализация данных.</summary>
        protected override void SetUpData()
        {
            _rowsManager.RowsCount = 1;
            _rowsManager.Reset();
            
            _dataCursorMock = CreateDataCursorMock(_rowsManager);
        }

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected override void ScenarioAfterInitTestedInstance()
        {
            Assert.IsTrue(TestedInstance.Read());
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public override void TestClose()
        {
            // Arrange - Act - Assert
            base.TestClose();
            
            // Assert
            _dataCursorMock.VerifyDispose();
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
            _dataCursorMock.Verify(qrs => qrs.Next(), Times.Exactly(TEST_ROWS_COUNT + 1));
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

            _dataCursorMock
                .Setup(c => c.GetValue(It.IsAny<int>()))
                .Returns<int>(i => rowData[i]);

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

            _dataCursorMock
                .Verify(c => c.GetValue(It.IsAny<int>()), Times.Exactly(expectedResult));
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
            _dataCursorMock
                .Setup(c => c.GetValue(columnName))
                .Returns(returnValue);
        }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected override void AssertGetValue(string columnName)
        {
            _dataCursorMock
                .Verify(c => c.GetValue(columnName), Times.Once());
        }

        /// <summary>Настройка для получения значения.</summary>
        protected override void ArrangeGetValue(int ordinal, object returnValue)
        {
            base.ArrangeGetValue(ordinal, returnValue);

            _dataCursorMock
                .Setup(c => c.GetValue(ordinal))
                .Returns(returnValue);
        }

        /// <summary>
        /// Проверка вызовов получения значения.
        /// </summary>
        protected override void AssertGetValue(int ordinal)
        {
            _dataCursorMock.Verify(c => c.GetValue(ordinal), Times.Once());
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
        /// Подготовка для тестирования <see cref="OneSDataReader.GetChar"/>.
        /// </summary>
        protected override void ArrangeGetChar(object returnValue, char expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToChar(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetChar"/>.
        /// </summary>
        protected override void AssertGetChar(object returnValue)
        {
            AssertGetTypedValue(c => c.ToChar(returnValue));
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

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        protected override void ArrangeGetDecimal(object returnValue, decimal expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToDecimal(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDecimal"/>.
        /// </summary>
        protected override void AssertGetDecimal(object returnValue)
        {
            AssertGetTypedValue(c => c.ToDecimal(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        protected override void ArrangeGetBoolean(object returnValue, bool expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToBoolean(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetBoolean"/>.
        /// </summary>
        protected override void AssertGetBoolean(object returnValue)
        {
            AssertGetTypedValue(c => c.ToBoolean(returnValue));
        }

        /// <summary>
        /// Подготовка для тестирования <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        protected override void ArrangeGetDateTime(object returnValue, DateTime expectedResult)
        {
            ArrangeGetTypedValue(c => c.ToDateTime(returnValue), expectedResult);
        }

        /// <summary>
        /// Проверка вызовов в <see cref="OneSDataReader.GetDateTime"/>.
        /// </summary>
        protected override void AssertGetDateTime(object returnValue)
        {
            AssertGetTypedValue(c => c.ToDateTime(returnValue));
        }
    }
}
