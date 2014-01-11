using System;
using Moq;
using NUnit.Framework;

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
            Assert.AreSame(QueryResultMock.Object, TestedInstance.QueryResult);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса данных пуст.
        /// </summary>
        [Test]
        public void TestReadWhenEmptyData()
        {
            // Arrange
            QueryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(true)
                .Verifiable();

            // Act & Assert
            Assert.IsFalse(TestedInstance.Read());

            // Assert
            QueryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>
        /// в случае если результат запроса имеет одну запись.
        /// </summary>
        [Test]
        public void TestReadWhenOneRow()
        {
            // Arrange
            var queryResultSelectionMock = CreateQueryResultSelectionMock();

            // Act & Assert
            // 1
            Assert.IsTrue(TestedInstance.Read());
            // 2
            Assert.IsFalse(TestedInstance.Read());

            // Assert
            QueryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
            QueryResultMock.Verify(qr => qr.Choose(), Times.Once());
            queryResultSelectionMock.Verify(qrs => qrs.Next(), Times.Exactly(2));
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
            QueryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(!expectedHasRows)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(expectedHasRows, TestedInstance.HasRows);

            QueryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
        }
    }
}
