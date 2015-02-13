using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="LazyDataReaderFieldInfoCollection"/>.
    /// </summary>
    [TestFixture]
    public sealed class LazyDataReaderFieldInfoCollectionTests
    {
        /// <summary>
        /// Тестирование инициализации.
        /// </summary>
        /// <remarks>
        /// Тестируется то, что делегат конструирования коллекции не вызывается в конструкторе.
        /// </remarks>
        [Test]
        public void TestInit()
        {
            // Arrange
            var callCounter = 0;
            Func<IDataReaderFieldInfoCollection> loadAction = () =>
                {
                    ++callCounter;
                    return null;
                };

            // Act
            var testedInstance = new LazyDataReaderFieldInfoCollection(loadAction);

            // Arrange
            Assert.AreEqual(0, callCounter);
        }
        
        /// <summary>Тестирование <see cref="LazyDataReaderFieldInfoCollection.Count"/>.</summary>
        [Test]
        public void TestCount()
        {
            // Arrange
            const int EXPECTED_COUNT = 5;

            var collectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);
            collectionMock
                .SetupGet(c => c.Count)
                .Returns(EXPECTED_COUNT);

            var testedInstance = new LazyDataReaderFieldInfoCollection(() => collectionMock.Object);

            // Act
            var actualResult = testedInstance.Count;

            // Assert
            Assert.AreEqual(EXPECTED_COUNT, actualResult);

            collectionMock
                .VerifyGet(c => c.Count, Times.Once());
        }

        /// <summary>Тестирование <see cref="LazyDataReaderFieldInfoCollection.Item"/>.</summary>
        [Test]
        public void TestItem()
        {
            // Arrange
            const int ARGUMENT_ORDINAL = 5;
            var expectedFieldInfo = new DataReaderFieldInfo("Field", typeof(int), null, null);

            var collectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);

            // Из-за контракта
            collectionMock
                .SetupGet(c => c.Count)
                .Returns(ARGUMENT_ORDINAL + 1);

            collectionMock
                .Setup(c => c[ARGUMENT_ORDINAL])
                .Returns(expectedFieldInfo);

            var testedInstance = new LazyDataReaderFieldInfoCollection(() => collectionMock.Object);

            // Act
            var actualResult = testedInstance[ARGUMENT_ORDINAL];

            // Assert
            Assert.AreEqual(expectedFieldInfo, actualResult);

            collectionMock
                .Verify(c => c[ARGUMENT_ORDINAL], Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="LazyDataReaderFieldInfoCollection.IndexOf"/>.
        /// </summary>
        [Test]
        public void TestIndexOf()
        {
            // Arrange
            const string ARGUMENT_NAME = "SpecialName";
            const int EXPECTED_RESULT = 4;

            var collectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);

            // Из-за контракта
            collectionMock
                .SetupGet(c => c.Count)
                .Returns(EXPECTED_RESULT + 1);

            collectionMock
                .Setup(c => c.IndexOf(ARGUMENT_NAME))
                .Returns(EXPECTED_RESULT);

            var testedInstance = new LazyDataReaderFieldInfoCollection(() => collectionMock.Object);

            // Act
            var actualResult = testedInstance.IndexOf(ARGUMENT_NAME);

            // Assert
            Assert.AreEqual(EXPECTED_RESULT, actualResult);

            collectionMock
                .Verify(c => c.IndexOf(ARGUMENT_NAME), Times.Once());
        }
    }
}
