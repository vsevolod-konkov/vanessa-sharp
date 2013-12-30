using System;
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
        /// Тестирование свойства <see cref="OneSDataReader.Item(int)"/>.
        /// </summary>
        [Test]
        public void TestItemByIndex()
        {
            // Arrange
            const string TEST_VALUE = "TEST_VALUE";
            const int TEST_FIELD_INDEX = 5;
            
            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(It.IsAny<int>()))
                .Returns(TEST_VALUE)
                .Verifiable();

            SetupColumnsGetCount(TEST_FIELD_INDEX + 1);

            // Act & Assert
            Assert.AreEqual(TEST_VALUE, TestedInstance[TEST_FIELD_INDEX]);
            _queryResultSelectionMock.Verify(qrs => qrs.Get(TEST_FIELD_INDEX), Times.Once());
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(string)"/>.
        /// </summary>
        [Test]
        public void TestItemByName()
        {
            // Arrange
            const string TEST_VALUE = "TEST_VALUE";
            const string TEST_FIELD_NAME = "TEST_FIELD";

            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(It.IsAny<string>()))
                .Returns(TEST_VALUE)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(TEST_VALUE, TestedInstance[TEST_FIELD_NAME]);
            _queryResultSelectionMock.Verify(qrs => qrs.Get(TEST_FIELD_NAME), Times.Once());
        }
    }
}
