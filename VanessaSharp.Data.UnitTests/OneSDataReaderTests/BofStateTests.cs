using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataReader"/>
    /// в случае когда экземпляр находится в состоянии начала чтения данных.
    /// </summary>
    [TestFixture]
    public sealed class BofStateTests : OpenStateTestBase
    {
        /// <summary>
        /// Тестирование инициализации.
        /// </summary>
        [Test]
        public void TestInit()
        {
            Assert.AreSame(DataRecordsProviderMock.Object, TestedInstance.DataRecordsProvider);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса данных пуст.
        /// </summary>
        [Test]
        public void TestReadWhenEmptyData()
        {
            // Arrange
            IDataCursor dataCursor;
            DataRecordsProviderMock
                .Setup(d => d.TryCreateCursor(out dataCursor))
                .Returns(false);

            // Act
            var result = TestedInstance.Read();

            // Assert
            Assert.IsFalse(result);
            DataRecordsProviderMock.Verify(d => d.TryCreateCursor(out dataCursor), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса имеет одну запись.
        /// </summary>
        [Test]
        public void TestReadWhenOneRow()
        {
            // Arrange
            var dataCursorMock = CreateDataCursorMock();

            // Act & Assert
            // 1
            Assert.IsTrue(TestedInstance.Read());
            // 2
            Assert.IsFalse(TestedInstance.Read());

            // Assert
            IDataCursor dataCursor;
            DataRecordsProviderMock.Verify(d => d.TryCreateCursor(out dataCursor), Times.Once());
            dataCursorMock.Verify(qrs => qrs.Next(), Times.Exactly(2));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            var result = TestedInstance.GetValues(new object[10]);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>
        /// в случае когда нет строк в результате запроса.
        /// </summary>
        [Test]
        public void TestHasRows([Values(false, true)] bool expectedHasRows)
        {
            // Arrange
            DataRecordsProviderMock
                .Setup(d => d.HasRecords)
                .Returns(expectedHasRows)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(expectedHasRows, TestedInstance.HasRows);

            DataRecordsProviderMock.Verify(d => d.HasRecords, Times.Once());
        }
    }
}
