using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataRecordReaderFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDataRecordReaderFactoryTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSDataRecordReaderFactory.CreateItemReader"/>.
        /// </summary>
        [Test]
        public void TestCreateItemReader()
        {
            // Arrange
            var expectedFields = new[]
                {
                    "ID", "Name", "Value"
                };

            var valueConverter = new Mock<IValueConverter>(MockBehavior.Strict).Object;

            var sqlReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            sqlReaderMock
                .SetupGet(r => r.ValueConverter)
                .Returns(valueConverter)
                .Verifiable();
            sqlReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(expectedFields.Length)
                .Verifiable();
            sqlReaderMock
                .Setup(r => r.GetFieldName(It.IsAny<int>()))
                .Returns<int>(i => expectedFields[i])
                .Verifiable();

            // Act
            var result = OneSDataRecordReaderFactory.Default.CreateItemReader(sqlReaderMock.Object);
            var record = result(new object[expectedFields.Length]);

            // Assert
            CollectionAssert.AreEqual(expectedFields, record.Fields);
            sqlReaderMock
                .VerifyGet(r => r.ValueConverter, Times.Once());
            sqlReaderMock
                .VerifyGet(r => r.FieldCount, Times.AtLeastOnce());
            sqlReaderMock
                .Verify(r => r.GetFieldName(It.IsAny<int>()), Times.Exactly(expectedFields.Length));
        }
    }
}
