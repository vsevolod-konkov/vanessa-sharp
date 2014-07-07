using System;
using System.Data;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSConnectionStates
{
    /// <summary>
    /// Базовый класс для тестирования состояний соединений
    /// соответствующих открытому соединению.
    /// </summary>
    public abstract class OpenStateObjectTestsBase
    {
        private const string TEST_CONNECTION_STRING = @"File=C:\1C\Db";
        private const int TEST_POOL_TIMEOUT = 4000;
        private const int TEST_POOL_CAPACITY = 20;
        private const string TEST_VERSION = "10.X";

        /// <summary>Ссылка на мок глобального контекста.</summary>
        protected Mock<IGlobalContext> GlobalContextMock { get; private set; }

        /// <summary>Ссылка на глобальный контекст.</summary>
        private IGlobalContext _globalContext;

        /// <summary>Тестируемый экземпляр.</summary>
        internal abstract OneSConnection.OpenStateObjectBase TestedInstance { get; }

        /// <summary>Инициализация тестируемого экземпляра состояния.</summary>
        /// <param name="parameters">Параметры подключения.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        /// <param name="version">Версия.</param>
        internal abstract void InitTestedInstance(
            OneSConnection.ConnectionParameters parameters, IGlobalContext globalContext, string version);

        /// <summary>Проверка параметров подключения у состояния.</summary>
        internal static void AssertConnectionParameters(OneSConnection.StateObject state)
        {
            Assert.AreEqual(TEST_CONNECTION_STRING, state.ConnectionString);
            Assert.AreEqual(TEST_POOL_TIMEOUT, state.PoolTimeout);
            Assert.AreEqual(TEST_POOL_CAPACITY, state.PoolCapacity);
        }

        /// <summary>Проверка версии у состояния.</summary>
        internal static void AssertVersion(OneSConnection.StateObject state)
        {
            Assert.AreEqual(TEST_VERSION, state.Version);
        }

        /// <summary>Проверка глобального контекста состояния.</summary>
        internal void AssertGlobalContext(OneSConnection.StateObject state)
        {
            Assert.AreSame(_globalContext, state.GlobalContext);
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            GlobalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);
            _globalContext = GlobalContextMock.Object;

            var parameters = new OneSConnection.ConnectionParameters
            {
                ConnectorFactory = null,
                ConnectionString = TEST_CONNECTION_STRING,
                PoolTimeout = TEST_POOL_TIMEOUT,
                PoolCapacity = TEST_POOL_CAPACITY
            };

            InitTestedInstance(parameters, _globalContext, TEST_VERSION);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.GlobalContext"/>.
        /// </summary>
        [Test]
        public void TestGlobalContext()
        {
            Assert.AreSame(_globalContext, TestedInstance.GlobalContext);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.OpenConnection"/>.
        /// </summary>
        [Test]
        public void TestOpenConnection()
        {
            Assert.Throws<InvalidOperationException>(() => TestedInstance.OpenConnection(new ConnectorCreationParams()));
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.CloseConnection"/>.
        /// </summary>
        [Test]
        public virtual void TestCloseConnection()
        {
            // Act
            var closedState = TestedInstance.CloseConnection();

            // Assert
            Assert.IsNotNull(closedState);
            Assert.IsInstanceOf<OneSConnection.ClosedStateObject>(closedState);
            AssertConnectionParameters(closedState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.ConnectionState"/>
        /// </summary>
        [Test]
        public void TestConnectionState()
        {
            Assert.AreEqual(ConnectionState.Open, TestedInstance.ConnectionState);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.ConnectionString"/>.
        /// </summary>
        [Test]
        public void TestConnectionString()
        {
            Assert.AreEqual(TEST_CONNECTION_STRING, TestedInstance.ConnectionString);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.PoolTimeout"/>.
        /// </summary>
        [Test]
        public void TestPoolTimeout()
        {
            Assert.AreEqual(TEST_POOL_TIMEOUT, TestedInstance.PoolTimeout);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.PoolCapacity"/>.
        /// </summary>
        [Test]
        public void TestPoolCapacity()
        {
            Assert.AreEqual(TEST_POOL_CAPACITY, TestedInstance.PoolCapacity);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.get_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestGetIsExclusiveMode([Values(false, true)] bool exclusiveMode)
        {
            // Arrange
            GlobalContextMock
                .Setup(ctx => ctx.ExclusiveMode())
                .Returns(exclusiveMode)
                .Verifiable();

            // Act & Assert
            Assert.AreEqual(exclusiveMode, TestedInstance.IsExclusiveMode);
            GlobalContextMock.Verify(ctx => ctx.ExclusiveMode(), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.set_IsExclusiveMode"/>.
        /// </summary>
        [Test]
        public void TestSetIsExclusiveMode([Values(false, true)] bool isExclusiveMode)
        {
            // Arrange
            GlobalContextMock
                .Setup(ctx => ctx.SetExclusiveMode(It.IsAny<bool>()))
                .Verifiable();

            // Act
            TestedInstance.IsExclusiveMode = isExclusiveMode;

            // Assert
            GlobalContextMock.Verify(ctx => ctx.SetExclusiveMode(isExclusiveMode), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.ClosedStateObject.get_Version"/>.
        /// </summary>
        [Test]
        public void TestGetVersion()
        {
            Assert.AreEqual(TEST_VERSION, TestedInstance.Version);
        }
    }
}
