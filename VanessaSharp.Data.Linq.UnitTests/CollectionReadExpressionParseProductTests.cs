using System;
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

        private Func<ISqlResultReader, OneSDataRecord> _itemReader; 

        private CollectionReadExpressionParseProduct<OneSDataRecord> _testedInstance;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _sqlCommand = new SqlCommand("SQL", SqlParameter.EmptyCollection);
            _itemReader = ReadRecord;

            _testedInstance = new CollectionReadExpressionParseProduct<OneSDataRecord>(_sqlCommand, _itemReader);
        }

        private static OneSDataRecord ReadRecord(ISqlResultReader resultReader)
        {
            return new OneSDataRecord();
        }

        /// <summary>Тестирование инициализации.</summary>
        [Test]
        public void TestInit()
        {
            Assert.AreSame(_sqlCommand, _testedInstance.Command);
            Assert.AreSame(_itemReader, _testedInstance.ItemReader);
        }

        /// <summary>Тестирование <see cref="CollectionReadExpressionParseProduct{T}.Execute"/>.</summary>
        [Test]
        public void TestExecute()
        {
            // Arrange
            var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            
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
            Assert.AreSame(_itemReader, enumerator.ItemReader);

            sqlCommandExecuterMock.Verify(e => e.ExecuteReader(_sqlCommand), Times.Once());
        }
    }
}
