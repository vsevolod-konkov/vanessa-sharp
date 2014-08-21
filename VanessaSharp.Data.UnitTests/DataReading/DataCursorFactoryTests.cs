using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="DataCursorFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class DataCursorFactoryTests
    {
        /// <summary>
        /// Тестирование метода <see cref="DataCursorFactory.Create"/>.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            // Arrange
            var fieldInfoCollectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);
            fieldInfoCollectionMock
                .Setup(c => c.Count)
                .Returns(5);

            var queryResultSelection = new Mock<IQueryResultSelection>(MockBehavior.Strict).Object;
            var testedInstance = new DataCursorFactory();

            // Act
            var result = testedInstance.Create(fieldInfoCollectionMock.Object, queryResultSelection);

            // Assert
            Assert.IsInstanceOf<DataCursor>(result);
            
            var dataCursor = (DataCursor)result;
            Assert.AreSame(fieldInfoCollectionMock.Object, dataCursor.DataReaderFieldInfoCollection);
            Assert.AreSame(queryResultSelection, dataCursor.QueryResultSelection);
        }
    }
}
