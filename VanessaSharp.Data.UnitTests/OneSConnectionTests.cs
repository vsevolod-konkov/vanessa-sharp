using System;
using System.Data;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>Тесты на <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class OneSConnectionTests
    {
        private const string TEST_CONNECTION_STRING = @"File=C:\1С";
        private const int TEST_POOL_TIMEOUT = 1000;
        private const int TEST_POOL_CAPACITY = 5;

        /// <summary>Мок состояния тестируемого соединения.</summary>
        private Mock<MockConnectionState> _stateMock;

        /// <summary>Тестируемый экземпляр соединения.</summary>
        private OneSConnection _testedInstance;

        private ConnectorCreationParams _connectorCreationParams;

        /// <summary>Тестирование получения свойства.</summary>
        /// <typeparam name="TProperty">Тип свойства.</typeparam>
        /// <param name="propertyValue">Значение тестируемого свойства.</param>
        /// <param name="stateGetPropertyAction">Действие получения значения свойства у состояния соединения.</param>
        /// <param name="testedGetPropertyAction">Действие получение значения свойства у тестируемого соединения.</param>
        private void TestGetProperty<TProperty>(
            TProperty propertyValue,
            Expression<Func<MockConnectionState, TProperty>> stateGetPropertyAction,
            Func<OneSConnection, TProperty> testedGetPropertyAction)
        {
            // Arrange
            _stateMock
                .SetupGet(stateGetPropertyAction)
                .Returns(propertyValue)
                .Verifiable();

            // Act
            var actualPropertyValue = testedGetPropertyAction(_testedInstance);

            // Assert
            Assert.AreEqual(propertyValue, actualPropertyValue);
            _stateMock.VerifyGet(stateGetPropertyAction, Times.Once());
        }

        /// <summary>Тестирование установки свойства <see cref="OneSConnection.PoolTimeout"/>.</summary>
        private void TestSetProperty<TProperty>(
            TProperty propertyValue,
            Action<MockConnectionState, TProperty> stateSetPropertyAction,
            Action<OneSConnection, TProperty> connectionSetPropertyAction)
        {
            // Arrange
            _stateMock
                .SetupSet(o => stateSetPropertyAction(o, It.IsAny<TProperty>()))
                .Verifiable();

            // Act
            connectionSetPropertyAction(_testedInstance, propertyValue);

            // Assert
            _stateMock.VerifySet(o => stateSetPropertyAction(o, propertyValue), Times.Once());
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _stateMock = new Mock<MockConnectionState>(MockBehavior.Strict);
            _connectorCreationParams = new ConnectorCreationParams();
            _testedInstance = new OneSConnection(_stateMock.Object, _connectorCreationParams);
        }

        /// <summary>Тестирование получения свойства <see cref="OneSConnection.ConnectionString"/>.</summary>
        [Test]
        public void TestGetConnectionString()
        {
            TestGetProperty(
                TEST_CONNECTION_STRING,
                o => o.ConnectionString,
                c => c.ConnectionString);
        }

        /// <summary>Тестирование установки свойства <see cref="OneSConnection.ConnectionString"/>.</summary>
        [Test]
        public void TestSetConnectionString()
        {
            TestSetProperty(
                TEST_CONNECTION_STRING,
                (o, v) => o.ConnectionString = v,
                (c, v) => c.ConnectionString = v);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.ChangeDatabase"/>.</summary>
        /// <remarks>Не поддерживается.</remarks>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestChangeDatabase()
        {
            // Act
            _testedInstance.ChangeDatabase(@"C:\1C2");
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.State"/>.</summary>
        [Test]
        public void TestState()
        {
            TestGetProperty(
                ConnectionState.Broken,
                o => o.ConnectionState,
                c => c.State);
        }

        /// <summary>Тестирование изменения состояния соединения.</summary>
        /// <param name="startState">Исходное состояние.</param>
        /// <param name="endState">Конечное состояние.</param>
        /// <param name="testedConnectionAction">
        /// Тестируемое действие соединения приводящее к изменению состояния.
        /// </param>
        /// <param name="changeStateAction">
        /// Действие вызываемое у объекта состояния.
        /// </param>
        private void TestChangeState(
            ConnectionState startState, ConnectionState endState,
            Action<OneSConnection> testedConnectionAction,
            Expression<Func<MockConnectionState, OneSConnection.StateObject>> changeStateAction)
        {
            // Arrange
            var endStateMock = new Mock<MockConnectionState>(MockBehavior.Strict);
            endStateMock
                .SetupGet(o => o.ConnectionState)
                .Returns(endState);

            _stateMock
                .SetupGet(o => o.ConnectionState)
                .Returns(startState);
            _stateMock
                .Setup(changeStateAction)
                .Returns(endStateMock.Object);

            // Act
            testedConnectionAction(_testedInstance);

            // Assert
            _stateMock.Verify(changeStateAction, Times.Once());
            Assert.AreEqual(endState, _testedInstance.State);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.Close"/>.</summary>
        [Test]
        public void TestClose()
        {
            TestChangeState(
                ConnectionState.Open,
                ConnectionState.Closed,
                c => c.Close(),
                o => o.CloseConnection()
                );
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.Open"/>.</summary>
        [Test]
        public void TestOpen()
        {
            TestChangeState(
                ConnectionState.Closed,
                ConnectionState.Open,
                c => c.Open(),
                o => o.OpenConnection(_connectorCreationParams)
                );
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.ServerVersion"/>.</summary>
        [Test]
        public void TestServerVersion()
        {
            TestGetProperty(
                "3.0",
                o => o.Version,
                c => c.ServerVersion);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.ConnectionTimeout"/>.</summary>
        [Test]
        public void TestConnectionTimeout()
        {
            TestGetProperty(
                TEST_POOL_TIMEOUT,
                o => o.PoolTimeout,
                c => c.ConnectionTimeout);
        }

        /// <summary>Тестирование получения свойства <see cref="OneSConnection.PoolTimeout"/>.</summary>
        [Test]
        public void TestGetPoolTimeout()
        {
            TestGetProperty(
                TEST_POOL_TIMEOUT,
                o => o.PoolTimeout,
                c => c.PoolTimeout);
        }

        /// <summary>Тестирование установки свойства <see cref="OneSConnection.PoolTimeout"/>.</summary>
        [Test]
        public void TestSetPoolTimeout()
        {
            TestSetProperty(
                TEST_POOL_TIMEOUT,
                (o, v) => o.PoolTimeout = v,
                (c, v) => c.PoolTimeout = v);
        }

        /// <summary>Тестирование получения свойства <see cref="OneSConnection.PoolCapacity"/>.</summary>
        [Test]
        public void TestGetPoolCapacity()
        {
            TestGetProperty(
                TEST_POOL_CAPACITY,
                o => o.PoolCapacity,
                c => c.PoolCapacity);
        }

        /// <summary>Тестирование установки свойства <see cref="OneSConnection.PoolCapacity"/>.</summary>
        [Test]
        public void TestSetPoolCapacity()
        {
            TestSetProperty(
                TEST_POOL_CAPACITY,
                (o, v) => o.PoolCapacity = v,
                (c, v) => c.PoolCapacity = v);
        }

        /// <summary>Тестирование получения свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test]
        public void TestGetIsExclusiveMode()
        {
            TestGetProperty(
                true,
                o => o.IsExclusiveMode,
                c => c.IsExclusiveMode);
        }

        /// <summary>Тестирование установки свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test]
        public void TestSetIsExclusiveMode()
        {
            TestSetProperty(
                true,
                (o, v) => o.IsExclusiveMode = v,
                (c, v) => c.IsExclusiveMode = v);
        }

        /// <summary>
        /// Тестирование реализации свойства <see cref="IGlobalContextProvider.GlobalContext"/>.
        /// </summary>
        [Test]
        public void TestGlobalContext()
        {
            TestGetProperty(
                new Mock<IGlobalContext>().Object,
                o => o.GlobalContext,
                c =>
                    {
                        IGlobalContextProvider provider = c;
                        return provider.GlobalContext;
                    });
        }

        /// <summary>Тестирование получения свойства <see cref="OneSConnection.CurrentTransaction"/>.</summary>
        [Test]
        public void TestCurrentTransaction()
        {
            TestGetProperty(
                new OneSTransaction(_testedInstance),
                o => o.CurrentTransaction,
                c => c.CurrentTransaction);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.BeginTransaction(IsolationLevel)"/>.</summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBeginSpecifiedLevelTransaction(
            [Values(
                IsolationLevel.Serializable, 
                IsolationLevel.RepeatableRead,
                IsolationLevel.ReadCommitted,
                IsolationLevel.ReadUncommitted,
                IsolationLevel.Snapshot,
                IsolationLevel.Chaos)] 
            IsolationLevel level)
        {
            _testedInstance.BeginTransaction(level);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.BeginTransaction()"/>.</summary>
        [Test]
        public void TestBeginUnpecifiedLevelTransaction()
        {
            // Arrange
            var transaction = new OneSTransaction(_testedInstance);

            var endStateMock = new Mock<MockConnectionState>(MockBehavior.Strict);
            InitStateMockForBeginTransactionTest(endStateMock);
            endStateMock
                .SetupGet(o => o.CurrentTransaction)
                .Returns(transaction)
                .Verifiable();

            InitStateMockForBeginTransactionTest(_stateMock);
            _stateMock
                .Setup(o => o.BeginTransaction(_testedInstance))
                .Returns(endStateMock.Object)
                .Verifiable();

            // Act
            var actualTransaction = _testedInstance.BeginTransaction();

            // Assert
            Assert.AreEqual(transaction, actualTransaction);
            _stateMock.Verify(o => o.BeginTransaction(_testedInstance), Times.Once());
            Assert.AreEqual(transaction, _testedInstance.CurrentTransaction);
        }

        private static void InitStateMockForBeginTransactionTest(Mock<MockConnectionState> stateMock)
        {
            stateMock
                .SetupGet(o => o.ConnectionState)
                .Returns(ConnectionState.Open);
            stateMock
                .SetupGet(o => o.ConnectionString)
                .Returns(TEST_CONNECTION_STRING);
        }

        /// <summary>
        /// Тестирование реализации <see cref="ITransactionManager.CommitTransaction"/>.
        /// </summary>
        [Test]
        public void TestCommitTransaction()
        {
            TestChangeState(
                ConnectionState.Open,
                ConnectionState.Open,
                c => { ITransactionManager tm = c; tm.CommitTransaction(); },
                o => o.CommitTransaction()
                );
        }

        /// <summary>
        /// Тестирование реализации <see cref="ITransactionManager.RollbackTransaction"/>.
        /// </summary>
        [Test]
        public void TestRollbackTransaction()
        {
            TestChangeState(
                ConnectionState.Open,
                ConnectionState.Open,
                c => { ITransactionManager tm = c; tm.RollbackTransaction(); },
                o => o.RollbackTransaction()
                );
        }
    }
}
