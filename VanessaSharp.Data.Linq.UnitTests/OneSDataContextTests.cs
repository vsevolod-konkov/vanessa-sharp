using System.Linq;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тесты на <see cref="OneSDataContext"/>.
    /// </summary>
    [TestFixture]
    internal sealed class OneSDataContextTests
    {
        /// <summary>Мок LINQ-провайдера к 1С.</summary>
        private Mock<IOneSQueryProvider> _queryProviderMock; 
        
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSDataContext _testedInstance;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _queryProviderMock = new Mock<IOneSQueryProvider>(MockBehavior.Strict);
            _testedInstance = new OneSDataContext(_queryProviderMock.Object);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataContext.Connection"/>.
        /// </summary>
        [Test]
        public void TestConnection()
        {
            // Arrange
            var connection = new OneSConnection();
            _queryProviderMock
                .SetupGet(provider => provider.Connection)
                .Returns(connection)
                .Verifiable();
            
            // Assert
            Assert.AreSame(connection, _testedInstance.Connection);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataContext.GetRecords"/>.
        /// </summary>
        [Test]
        public void TestGetRecords()
        {
            const string SOURCE_NAME = "AnySource";
            
            // Arrange
            var expectedQuery = new Mock<IQueryable<OneSDataRecord>>(MockBehavior.Strict).Object;

            _queryProviderMock
                .Setup(provider => provider.CreateGetRecordsQuery(It.IsAny<string>()))
                .Returns(expectedQuery)
                .Verifiable();

            // Act
            var result = _testedInstance.GetRecords(SOURCE_NAME);

            // Assert
            Assert.AreSame(expectedQuery, result);
            _queryProviderMock.Verify(provider => provider.CreateGetRecordsQuery(SOURCE_NAME), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataContext.Get{T}"/>.
        /// </summary>
        [Test]
        public void TestGetOf()
        {
            // Arrange
            var expectedQuery = new Mock<IQueryable<AnyData>>(MockBehavior.Strict).Object;

            _queryProviderMock
                .Setup(provider => provider.CreateQueryOf<AnyData>())
                .Returns(expectedQuery)
                .Verifiable();

            // Act
            var result = _testedInstance.Get<AnyData>();

            // Assert
            Assert.AreSame(expectedQuery, result);
            _queryProviderMock.Verify(provider => provider.CreateQueryOf<AnyData>(), Times.Once());
        }

        /// <summary>
        /// Тестовый тип.
        /// </summary>
        public struct AnyData
        { }    
    }
}
