using System;
using System.Data;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSConnectionStates
{
    /// <summary>
    /// Тесты на <see cref="OneSConnection.OpenStateObject"/>.
    /// </summary>
    [TestFixture]
    public sealed class OpenStateObjectTests
    {
        private const string TEST_CONNECTION_STRING = @"File=C:\1C\Db";
        private const int TEST_POOL_TIMEOUT = 4000;
        private const int TEST_POOL_CAPACITY = 20;
        private const string TEST_VERSION = "10.X";

        /// <summary>Ссылка на мок глобального контекста.</summary>
        private Mock<IGlobalContext> _globalContextMock; 

        /// <summary>Ссылка на глобальный контекст.</summary>
        private IGlobalContext _globalContext;

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnection.OpenStateObject _testedInstance;

        /// <summary>Проверка параметров подключения у состояния.</summary>
        private static void AssertConnectionParameters(OneSConnection.StateObject state)
        {
            Assert.AreEqual(TEST_CONNECTION_STRING, state.ConnectionString);
            Assert.AreEqual(TEST_POOL_TIMEOUT, state.PoolTimeout);
            Assert.AreEqual(TEST_POOL_CAPACITY, state.PoolCapacity);
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);
            _globalContext = _globalContextMock.Object;

            var parameters = new OneSConnection.ConnectionParameters
                {
                    ConnectorFactory = null,
                    ConnectionString = TEST_CONNECTION_STRING,
                    PoolTimeout = TEST_POOL_TIMEOUT,
                    PoolCapacity = TEST_POOL_CAPACITY
                };

            _testedInstance = new OneSConnection.OpenStateObject(
                parameters, _globalContext, TEST_VERSION);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.GlobalContext"/>.
        /// </summary>
        [Test]
        public void TestGlobalContext()
        {
            Assert.AreSame(_globalContext, _testedInstance.GlobalContext);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.OpenConnection"/>.
        /// </summary>
        [Test]
        public void TestOpenConnection()
        {
            Assert.Throws<InvalidOperationException>(() => _testedInstance.OpenConnection());
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.CloseConnection"/>.
        /// </summary>
        [Test]
        public void TestCloseConnection()
        {
            // Act
            var closedState = _testedInstance.CloseConnection();

            // Assert
            Assert.IsNotNull(closedState);
            Assert.IsInstanceOf<OneSConnection.ClosedStateObject>(closedState);
            AssertConnectionParameters(closedState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.ConnectionState"/>
        /// </summary>
        [Test]
        public void TestConnectionState()
        {
            Assert.AreEqual(ConnectionState.Open, _testedInstance.ConnectionState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.ConnectionString"/>.
        /// </summary>
        [Test]
        public void TestConnectionString()
        {
            Assert.AreEqual(TEST_CONNECTION_STRING, _testedInstance.ConnectionString);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.PoolTimeout"/>.
        /// </summary>
        [Test]
        public void TestPoolTimeout()
        {
            Assert.AreEqual(TEST_POOL_TIMEOUT, _testedInstance.PoolTimeout);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.PoolCapacity"/>.
        /// </summary>
        [Test]
        public void TestPoolCapacity()
        {
            Assert.AreEqual(TEST_POOL_CAPACITY, _testedInstance.PoolCapacity);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.get_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestGetIsExclusiveMode([Values(false, true)] bool exclusiveMode)
        {
            // Arrange
            _globalContextMock
                .Setup(ctx => ctx.ExclusiveMode())
                .Returns(exclusiveMode)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(exclusiveMode, _testedInstance.IsExclusiveMode);
            _globalContextMock.Verify(ctx => ctx.ExclusiveMode(), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.set_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestSetIsExclusiveMode([Values(false, true)] bool isExclusiveMode)
        {
            // Arrange
            _globalContextMock
                .Setup(ctx => ctx.SetExclusiveMode(It.IsAny<bool>()))
                .Verifiable();

            // Act
            _testedInstance.IsExclusiveMode = isExclusiveMode;

            // Assert
            _globalContextMock.Verify(ctx => ctx.SetExclusiveMode(isExclusiveMode), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.BeginTransaction"/>.
        /// </summary>
        [Test]
        public void TestBeginTransaction()
        {
            // Arrange
            var connection = new OneSConnection();

            _globalContextMock
                .Setup(ctx => ctx.BeginTransaction())
                .Verifiable();

            // Act
            var newState = _testedInstance.BeginTransaction(connection);

            // Assert
            Assert.IsNotNull(newState);
            Assert.IsInstanceOf<OneSConnection.TransactionStateObject>(newState);
            Assert.AreSame(_globalContext, newState.GlobalContext);
            AssertConnectionParameters(newState);
            Assert.AreEqual(TEST_VERSION, newState.Version);

            _globalContextMock.Verify(ctx => ctx.BeginTransaction(), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.CommitTransaction"/>.
        /// </summary>
        [Test]
        public void TestCommitTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => _testedInstance.CommitTransaction());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.RollbackTransaction"/>.
        /// </summary>
        [Test]
        public void TestRollbackTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => _testedInstance.RollbackTransaction());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.CurrentTransaction"/>.
        /// </summary>
        [Test]
        public void TestCurrentTransaction()
        {
            Assert.IsNull(_testedInstance.CurrentTransaction);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.get_Version"/>.
        /// </summary>
        [Test]
        public void TestGetVersion()
        {
            Assert.AreEqual(TEST_VERSION, _testedInstance.Version);
        }
    }
}
