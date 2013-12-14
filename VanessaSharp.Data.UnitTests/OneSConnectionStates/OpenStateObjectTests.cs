using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSConnectionStates
{
    /// <summary>
    /// Тесты на <see cref="OneSConnection.OpenStateObject"/>.
    /// </summary>
    [TestFixture]
    public sealed class OpenStateObjectTests : OpenStateObjectTestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        internal override OneSConnection.OpenStateObjectBase TestedInstance
        {
            get { return _testedInstance; }
        }
        private OneSConnection.OpenStateObject _testedInstance;

        /// <summary>Инициализация тестируемого экземпляра состояния.</summary>
        /// <param name="parameters">Параметры подключения.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        /// <param name="version">Версия.</param>
        internal override void InitTestedInstance(
            OneSConnection.ConnectionParameters parameters, IGlobalContext globalContext, string version)
        {
            _testedInstance = new OneSConnection.OpenStateObject(
                parameters, globalContext, version);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.BeginTransaction"/>.
        /// </summary>
        [Test]
        public void TestBeginTransaction()
        {
            // Arrange
            var connection = new OneSConnection();

            GlobalContextMock
                .Setup(ctx => ctx.BeginTransaction())
                .Verifiable();

            // Act
            var newState = _testedInstance.BeginTransaction(connection);

            // Assert
            Assert.IsNotNull(newState);
            Assert.IsInstanceOf<OneSConnection.TransactionStateObject>(newState);
            AssertGlobalContext(newState);
            AssertConnectionParameters(newState);
            AssertVersion(newState);

            GlobalContextMock.Verify(ctx => ctx.BeginTransaction(), Times.Once());
        }

        /// <summary>
        /// Тестирование
        /// <see cref="OneSConnection.StateObject.CommitTransaction"/>.
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
    }
}
