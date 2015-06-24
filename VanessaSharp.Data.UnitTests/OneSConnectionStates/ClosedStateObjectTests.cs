using System;
using System.Data;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSConnectionStates
{
    /// <summary>
    /// Тесты на <see cref="OneSConnection.ClosedStateObject"/>.
    /// </summary>
    [TestFixture]
    public sealed class ClosedStateObjectTests
    {
        /// <summary>
        /// Мок <see cref="IOneSConnectorFactory"/>.
        /// </summary>
        private Mock<IOneSConnectorFactory> _connectorFactoryMock;

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnection.ClosedStateObject _testedInstance;

        /// <summary>Тестирование свойства.</summary>
        /// <typeparam name="TProperty">Тип значения свойства.</typeparam>
        /// <param name="value">Новое значение свойства.</param>
        /// <param name="propertyGetter">Получатель значения свойства.</param>
        /// <param name="propertySetter">Установщик значения свойства.</param>
        private void TestProperty<TProperty>(
            TProperty value,
            Func<OneSConnection.ClosedStateObject, TProperty> propertyGetter,
            Action<OneSConnection.ClosedStateObject, TProperty> propertySetter)
        {
            Assert.AreEqual(default(TProperty), propertyGetter(_testedInstance));

            propertySetter(_testedInstance, value);

            Assert.AreEqual(value, propertyGetter(_testedInstance));
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _connectorFactoryMock = new Mock<IOneSConnectorFactory>(MockBehavior.Strict);

            _testedInstance = new OneSConnection.ClosedStateObject(_connectorFactoryMock.Object);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.GlobalContext"/>.
        /// </summary>
        [Test]
        public void TestGlobalContext()
        {
            Assert.Throws<InvalidOperationException>(() => { var ctx = _testedInstance.GlobalContext; });
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.OpenConnection"/>.
        /// </summary>
        [Test]
        public void TestOpenConnection()
        {
            const string TEST_CONNECTION_STRING = @"File=C:\1C\Db";
            const int TEST_POOL_TIMEOUT = 5000;
            const int TEST_POOL_CAPACITY = 40;
            const string TEST_VERSION = "X.X";

            // Arrange
            var creationParams = new ConnectorCreationParams();

            var globalContext = new Mock<IGlobalContext>(MockBehavior.Strict).Object;
            
            var connectorMock = new Mock<IOneSConnector>(MockBehavior.Strict);
            connectorMock
                .SetupSet(c => c.PoolTimeout = It.IsAny<uint>())
                .Verifiable();
            connectorMock
                .SetupSet(c => c.PoolCapacity = It.IsAny<uint>())
                .Verifiable();
            connectorMock
                .Setup(c => c.Init(null))
                .Verifiable();
            connectorMock
                .Setup(c => c.Connect(It.IsAny<string>()))
                .Returns(globalContext)
                .Verifiable();
            connectorMock
                .SetupGet(c => c.Version)
                .Returns(TEST_VERSION)
                .Verifiable();
            connectorMock
                .Setup(c => c.Dispose())
                .Verifiable();

            _connectorFactoryMock
                .Setup(f => f.Create(creationParams))
                .Returns(connectorMock.Object)
                .Verifiable();

            _testedInstance.ConnectionString = TEST_CONNECTION_STRING;
            _testedInstance.PoolTimeout = TEST_POOL_TIMEOUT;
            _testedInstance.PoolCapacity = TEST_POOL_CAPACITY;

            // Act
            var openState = _testedInstance.OpenConnection(creationParams);

            // Assert
            _connectorFactoryMock.Verify(f => f.Create(creationParams), Times.Once());

            connectorMock.VerifySet(f => f.PoolTimeout = (uint)TEST_POOL_TIMEOUT, Times.Once());
            connectorMock.VerifySet(f => f.PoolCapacity = (uint)TEST_POOL_CAPACITY, Times.Once());
            connectorMock.Verify(f => f.Init(null), Times.Once());
            connectorMock.Verify(f => f.Connect(TEST_CONNECTION_STRING), Times.Once());
            connectorMock.VerifyGet(f => f.Version, Times.Once());
            connectorMock.Verify(f => f.Dispose(), Times.Once());
            
            Assert.IsNotNull(openState);
            Assert.IsInstanceOf<OneSConnection.OpenStateObject>(openState);
            Assert.AreEqual(TEST_CONNECTION_STRING, openState.ConnectionString);
            Assert.AreEqual(TEST_POOL_TIMEOUT, openState.PoolTimeout);
            Assert.AreEqual(TEST_POOL_CAPACITY, openState.PoolCapacity);
            Assert.AreEqual(TEST_VERSION, openState.Version);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.CloseConnection"/>.
        /// </summary>
        [Test]
        public void TestCloseConnection()
        {
            // Act
            var newState = _testedInstance.CloseConnection();

            // Assert
            Assert.AreSame(_testedInstance, newState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.ConnectionState"/>
        /// </summary>
        [Test]
        public void TestConnectionState()
        {
            Assert.AreEqual(ConnectionState.Closed, _testedInstance.ConnectionState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.ConnectionString"/>.
        /// </summary>
        [Test]
        public void TestConnectionString()
        {
            TestProperty(@"File=C:\1C", s => s.ConnectionString, (s, v) => s.ConnectionString = v);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.PoolTimeout"/>.
        /// </summary>
        [Test]
        public void TestPoolTimeout()
        {
            TestProperty(1000, s => s.PoolTimeout, (s, v) => s.PoolTimeout = v);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.PoolCapacity"/>.
        /// </summary>
        [Test]
        public void TestPoolCapacity()
        {
            TestProperty(20, s => s.PoolCapacity, (s, v) => s.PoolCapacity = v);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.get_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestGetIsExclusiveMode()
        {
            Assert.Throws<InvalidOperationException>(() => { var mode = _testedInstance.IsExclusiveMode; });
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.set_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestSetIsExclusiveMode()
        {
            Assert.Throws<InvalidOperationException>(() => { _testedInstance.IsExclusiveMode = true; });
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.BeginTransaction"/>.
        /// </summary>
        [Test]
        public void TestBeginTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => _testedInstance.BeginTransaction(new OneSConnection()));
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
            Assert.Throws<InvalidOperationException>(() => { var version = _testedInstance.Version; });
        }
    }
}
