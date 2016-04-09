using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тестирование <see cref="ItemEnumerable"/>
    /// </summary>
    [TestFixture]
    public sealed class ItemEnumerableTests
    {
        /// <summary>
        /// Тестирование реализации <see cref="ItemEnumerable{T}.GetEnumerator"/>.
        /// </summary>
        [Test]
        public void TestGetEnumerator()
        {
            // Arrange
            var valueConverter = new Mock<IValueConverter>(MockBehavior.Strict).Object;

            var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            sqlResultReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(0);
            sqlResultReaderMock
                .SetupGet(r => r.ValueConverter)
                .Returns(valueConverter);

            var sqlResultReader = sqlResultReaderMock.Object;

            Func<object[], OneSDataRecord> recordReader = values => null;

            var itemReaderFactoryMock = new Mock<IItemReaderFactory<OneSDataRecord>>(MockBehavior.Strict);
            itemReaderFactoryMock
                .Setup(f => f.CreateItemReader(sqlResultReader))
                .Returns(recordReader);

            var testedInstance = new ItemEnumerable<OneSDataRecord>(sqlResultReader, itemReaderFactoryMock.Object);
            Assert.AreSame(sqlResultReader, testedInstance.SqlResultReader);
            Assert.AreSame(itemReaderFactoryMock.Object, testedInstance.ItemReaderFactory);

            // Act
            var result = testedInstance.GetEnumerator();

            // Assert
            var itemEnumerator = AssertEx.IsInstanceAndCastOf<ItemEnumerator<OneSDataRecord>>(result);
            Assert.IsTrue(itemEnumerator.IsSameSqlResultReader(sqlResultReader));

            Assert.AreSame(recordReader, itemEnumerator.ItemReader);

            itemReaderFactoryMock
                .Verify(f => f.CreateItemReader(sqlResultReader), Times.Once());
        }
    }
}
