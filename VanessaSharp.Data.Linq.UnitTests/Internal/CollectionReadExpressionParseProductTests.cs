using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тестирование <see cref="CollectionReadExpressionParseProduct{T}"/>.
    /// </summary>
    [TestFixture]
    public sealed class CollectionReadExpressionParseProductTests
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
            _sqlCommand = new SqlCommand("SQL", Empty.ReadOnly<SqlParameter>());
            _itemReaderFactoryMock = new Mock<IItemReaderFactory<OneSDataRecord>>(MockBehavior.Strict);

            _testedInstance = new CollectionReadExpressionParseProduct<OneSDataRecord>(_sqlCommand, _itemReaderFactoryMock.Object);
        }

        private static OneSDataRecord ReadRecordForTest(object[] values)
        {
            throw new InvalidOperationException("Этот метод нельзя вызывать.");
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
            Func<object[], OneSDataRecord> recordReader = ReadRecordForTest;

            _itemReaderFactoryMock
                .Setup(f => f.CreateItemReader(It.IsAny<ISqlResultReader>()))
                .Returns(recordReader);
            
            var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            sqlResultReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(0);
            
            var sqlCommandExecuterMock = new Mock<ISqlCommandExecuter>(MockBehavior.Strict);
            sqlCommandExecuterMock
                .Setup(e => e.ExecuteReader(_sqlCommand))
                .Returns(sqlResultReaderMock.Object);

            // Act
            var result = _testedInstance.Execute(sqlCommandExecuterMock.Object);

            // Assert
            var enumerator = AssertEx.IsInstanceAndCastOf<ItemEnumerator<OneSDataRecord>>(result);
            Assert.IsTrue(enumerator.IsSameSqlResultReader(sqlResultReaderMock.Object));
            Assert.AreSame(recordReader, enumerator.ItemReader);

            sqlCommandExecuterMock.Verify(e => e.ExecuteReader(_sqlCommand), Times.Once());
            _itemReaderFactoryMock.Verify(f => f.CreateItemReader(sqlResultReaderMock.Object), Times.Once());
        }
    }
}
