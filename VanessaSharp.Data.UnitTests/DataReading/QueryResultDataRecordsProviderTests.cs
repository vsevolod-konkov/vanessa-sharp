using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="QueryResultDataRecordsProvider"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryResultDataRecordsProviderTests
    {
        /// <summary>
        /// Тестовый экземпляр.
        /// </summary>
        private QueryResultDataRecordsProvider _testedInstance;

        private DisposableMock<IQueryResult> _queryResultMock;
        
        private IDataReaderFieldInfoCollection _fieldInfoCollection;

        private const QueryResultIteration QUERY_RESULT_ITERATION = QueryResultIteration.ByGroupsWithHierarchy;

        private Mock<IDataCursorFactory> _dataCursorFactoryMock;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _queryResultMock = new DisposableMock<IQueryResult>();
            _fieldInfoCollection = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict).Object;
            _dataCursorFactoryMock = new Mock<IDataCursorFactory>(MockBehavior.Strict);

            _testedInstance = new QueryResultDataRecordsProvider(
                _queryResultMock.Object,
                QUERY_RESULT_ITERATION,
                _fieldInfoCollection,
                _dataCursorFactoryMock.Object);
        }

        /// <summary>
        /// Тестирование инициализации.
        /// </summary>
        [Test]
        public void TestInit()
        {
            // Assert
            Assert.AreSame(_queryResultMock.Object, _testedInstance.QueryResult);
            Assert.AreEqual(QUERY_RESULT_ITERATION, _testedInstance.QueryResultIteration);
            Assert.AreSame(_fieldInfoCollection, _testedInstance.Fields);
        }

        /// <summary>
        /// Тестирование <see cref="QueryResultDataRecordsProvider.TryCreateCursor"/>
        /// в случае если результат запроса пуст.
        /// </summary>
        [Test]
        public void TestTryCreateCursorWhenResultIsEmpty()
        {
            // Arrange
            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(true);

            // Act
            IDataCursor dataCursor;
            var result = _testedInstance.TryCreateCursor(out dataCursor);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(dataCursor);

            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="QueryResultDataRecordsProvider.TryCreateCursor"/>
        /// в случае, если результат запроса не пуст.
        /// </summary>
        [Test]
        public void TestTryCreateCursorWhenResultIsNotEmpty()
        {
            // Arrange
            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(false);

            var queryResultSelection = new Mock<IQueryResultSelection>(MockBehavior.Strict).Object;
            _queryResultMock
                .Setup(qr => qr.Choose(QUERY_RESULT_ITERATION))
                .Returns(queryResultSelection);

            var expectedDataCursor = new Mock<IDataCursor>(MockBehavior.Strict).Object;

            _dataCursorFactoryMock
                .Setup(f => f.Create(_fieldInfoCollection, queryResultSelection))
                .Returns(expectedDataCursor);

            // Act
            IDataCursor actualDataCursor;
            var result = _testedInstance.TryCreateCursor(out actualDataCursor);

            // Assert
            Assert.IsTrue(result);
            Assert.AreSame(expectedDataCursor, actualDataCursor);

            _queryResultMock.Verify(qr => qr.IsEmpty(), Times.Once());
            _queryResultMock.Verify(qr => qr.Choose(QUERY_RESULT_ITERATION), Times.Once());
            _dataCursorFactoryMock.Verify(f => f.Create(_fieldInfoCollection, queryResultSelection), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="QueryResultDataRecordsProvider.HasRecords"/>.
        /// </summary>
        [Test]
        public void TestHasRecords([Values(false, true)] bool isEmpty)
        {
            var expectedResult = !isEmpty;

            // Arrange
            _queryResultMock
                .Setup(qr => qr.IsEmpty())
                .Returns(isEmpty);

            // Act
            var actualResult = _testedInstance.HasRecords;

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        /// <summary>
        /// Тестирование <see cref="QueryResultDataRecordsProvider.Dispose"/>.
        /// </summary>
        [Test]
        public void TestDispose()
        {
            // Act
            _testedInstance.Dispose();

            // Assert
            _queryResultMock.VerifyDispose();
        }
    }
}
