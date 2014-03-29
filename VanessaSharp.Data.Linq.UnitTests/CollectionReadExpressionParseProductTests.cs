using System;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="CollectionReadExpressionParseProduct{T}"/>.
    /// </summary>
    [TestFixture]
    public sealed class CollectionReadExpressionParseProductTests : TestsBase
    {
        private SqlCommand _sqlCommand;

        private Mock<IItemReaderFactory<OneSDataRecord>> _itemReaderFactoryMock; 

        private CollectionReadExpressionParseProduct<OneSDataRecord> _testedInstance;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _sqlCommand = new SqlCommand("SQL", SqlParameter.EmptyCollection);
            _itemReaderFactoryMock = new Mock<IItemReaderFactory<OneSDataRecord>>(MockBehavior.Strict);

            _testedInstance = new CollectionReadExpressionParseProduct<OneSDataRecord>(_sqlCommand, _itemReaderFactoryMock.Object);
        }

        private static OneSDataRecord ReadRecord(object[] values)
        {
            return new OneSDataRecord(
                new ReadOnlyCollection<string>(new string[0]),
                new ReadOnlyCollection<OneSValue>(new OneSValue[0])
                );
        }

        /// <summary>Тестирование инициализации.</summary>
        [Test]
        public void TestInit()
        {
            Assert.AreSame(_sqlCommand, _testedInstance.Command);
            Assert.AreSame(_itemReaderFactoryMock.Object, _testedInstance.ItemReaderFactory);
        }

        /// <summary>Тестирование <see cref="CollectionReadExpressionParseProduct{T}.Execute"/>.</summary>
        [Test]
        public void TestExecute()
        {
            // Arrange
            Func<object[], OneSDataRecord> recordReader = ReadRecord;

            _itemReaderFactoryMock
                .Setup(f => f.CreateItemReader(It.IsAny<ISqlResultReader>()))
                .Returns(recordReader)
                .Verifiable();
            
            var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            sqlResultReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(0)
                .Verifiable();
            
            var sqlCommandExecuterMock = new Mock<ISqlCommandExecuter>(MockBehavior.Strict);
            sqlCommandExecuterMock
                .Setup(e => e.ExecuteReader(It.IsAny<SqlCommand>()))
                .Returns(sqlResultReaderMock.Object)
                .Verifiable();

            // Act
            var result = _testedInstance.Execute(sqlCommandExecuterMock.Object);

            // Assert
            var enumerator = AssertAndCast<ItemEnumerator<OneSDataRecord>>(result);
            Assert.AreSame(sqlResultReaderMock.Object, enumerator.SqlReader);
            Assert.AreSame(recordReader, enumerator.ItemReader);

            sqlCommandExecuterMock.Verify(e => e.ExecuteReader(_sqlCommand), Times.Once());
            _itemReaderFactoryMock.Verify(f => f.CreateItemReader(sqlResultReaderMock.Object), Times.Once());
        }
    }
}
