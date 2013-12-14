using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSConnectionStates
{
    /// <summary>
    /// Тесты на <see cref="OneSConnection.TransactionStateObject"/>.
    /// </summary>
    [TestFixture]
    public sealed class TransactionStateObjectTests : OpenStateObjectTestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        internal override OneSConnection.OpenStateObjectBase TestedInstance
        {
            get { return _testedInstance; }
        }
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnection.TransactionStateObject _testedInstance;

        /// <summary>Экземпляр транзакции.</summary>
        private OneSTransaction _transaction;

        /// <summary>Инициализация тестируемого экземпляра состояния.</summary>
        /// <param name="parameters">Параметры подключения.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        /// <param name="version">Версия.</param>
        internal override void InitTestedInstance(
            OneSConnection.ConnectionParameters parameters, IGlobalContext globalContext, string version)
        {
            _transaction = new OneSTransaction(new OneSConnection());

            _testedInstance = new OneSConnection.TransactionStateObject(parameters, globalContext, version, _transaction);
        }
        
        /// <summary>
        /// Тестирование <see cref="OneSConnection.StateObject.CloseConnection"/>.
        /// </summary>
        [Test]
        public override void TestCloseConnection()
        {
            // Arrange
            GlobalContextMock
                .Setup(ctx => ctx.RollbackTransaction())
                .Verifiable();
            
            // Arrange-Act-Assert
            base.TestCloseConnection();

            // Assert
            GlobalContextMock.Verify(ctx => ctx.RollbackTransaction(), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.BeginTransaction"/>.
        /// </summary>
        [Test]
        public void TestBeginTransaction()
        {
            var connection = new OneSConnection();
            Assert.Throws<InvalidOperationException>(() => _testedInstance.BeginTransaction(connection));
        }

        /// <summary>
        /// Тестирование возвращения из транзакции в обычное открытое состояние.
        /// </summary>
        /// <param name="globalContextAction">Действие возврата из транзакции в глобальном контексте.</param>
        /// <param name="testedAction">Тестируемый метод состояния.</param>
        private void TestReturnFromTransaction(
            Expression<Action<IGlobalContext>> globalContextAction, 
            Func<OneSConnection.TransactionStateObject, OneSConnection.StateObject> testedAction)
        {
            // Arrange
            GlobalContextMock
                .Setup(globalContextAction)
                .Verifiable();

            // Act
            var newState = testedAction(_testedInstance);

            // Assert
            Assert.IsNotNull(newState);
            Assert.IsInstanceOf<OneSConnection.OpenStateObject>(newState);
            AssertGlobalContext(newState);
            AssertConnectionParameters(newState);
            AssertVersion(newState);

            GlobalContextMock.Verify(globalContextAction, Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.CommitTransaction"/>.
        /// </summary>
        [Test]
        public void TestCommitTransaction()
        {
            TestReturnFromTransaction(
                ctx => ctx.CommitTransaction(),
                o => o.CommitTransaction());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.RollbackTransaction"/>.
        /// </summary>
        [Test]
        public void TestRollbackTransaction()
        {
            TestReturnFromTransaction(
                ctx => ctx.RollbackTransaction(),
                o => o.RollbackTransaction());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.CurrentTransaction"/>.
        /// </summary>
        [Test]
        public void TestCurrentTransaction()
        {
            Assert.AreSame(_transaction, _testedInstance.CurrentTransaction);
        }
    }
}
