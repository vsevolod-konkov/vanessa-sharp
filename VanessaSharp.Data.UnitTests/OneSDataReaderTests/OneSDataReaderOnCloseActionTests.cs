using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование действия при закрытии экземпляра <see cref="OneSDataReader"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDataReaderOnCloseActionTests
    {
        public enum StartState
        {
            Bof,
            Row,
            Eof,
            Closed
        }

        /// <summary>Тест.</summary>
        /// <param name="startState">
        /// Начальное состояние при закрытии.
        /// </param>
        /// <param name="onCloseActionExists">
        /// Установлено ли действие при закрытии.
        /// </param>
        [Test]
        public void Test(
            [Values(
                StartState.Bof,
                StartState.Row,
                StartState.Eof,
                StartState.Closed)] 
            StartState startState, 
            
            [Values(false, true)] 
            bool onCloseActionExists)
        {
            // Arrange
            var actualExecutedCounter = 0;

            var onCloseAction = onCloseActionExists
                                    ? () => actualExecutedCounter++
                                    : (Action)null;

            var rowCounter = 0;
            var dataCursorMock = new DisposableMock<IDataCursor>();
            dataCursorMock
                .Setup(c => c.Next())
                .Returns(() => rowCounter++ < 2);

            var dataCursor = dataCursorMock.Object;

            var dataRecordsProviderMock = new DisposableMock<IDataRecordsProvider>();
            dataRecordsProviderMock
                .Setup(p => p.TryCreateCursor(out dataCursor))
                .Returns(true);

            var testedInstance = new OneSDataReader(
                dataRecordsProviderMock.Object,
                new Mock<IValueConverter>(MockBehavior.Strict).Object,
                false,
                onCloseAction);

            if (startState != StartState.Bof)
            {
                testedInstance.Read();

                if (startState != StartState.Row)
                    testedInstance.Read();

                if (startState == StartState.Closed)
                    testedInstance.Close();
            }
            
            // Act
            testedInstance.Close();

            // Assert
            var expectedExecutedCounter = onCloseActionExists
                                              ? 1
                                              : 0;
            
            Assert.AreEqual(expectedExecutedCounter, actualExecutedCounter);
        }
    }
}
