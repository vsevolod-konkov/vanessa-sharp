using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Тесты на свойства экзеипляра <see cref="OneSCommand"/>
    /// </summary>
    [TestFixture]
    public sealed class OneSCommandTests
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSCommand _testedInstance;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new OneSCommand();
        }

        /// <summary>Тестирование <see cref="OneSCommand.Cancel"/>.</summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCancel()
        {
            _testedInstance.Cancel();
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.CommandText"/>.
        /// </summary>
        [Test]
        public void TestCommandText()
        {
            const string TEST_COMMAND = "ЗАПРОС";

            _testedInstance.CommandText = TEST_COMMAND;
            Assert.AreEqual(TEST_COMMAND, _testedInstance.CommandText);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.set_CommandTimeout"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetCommandTimeout()
        {
            _testedInstance.CommandTimeout = 1000;
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.get_CommandTimeout"/>.
        /// </summary>
        [Test]
        public void TestGetCommandTimeout()
        {
            Assert.AreEqual(0, _testedInstance.CommandTimeout);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.set_CommandType"/>.
        /// </summary>
        [Test]
        [TestCase(CommandType.StoredProcedure, ExpectedException = typeof(NotSupportedException))]
        [TestCase(CommandType.TableDirect, ExpectedException = typeof(NotSupportedException))]
        [TestCase(CommandType.Text)]
        public void TestSetCommandType(CommandType commandType)
        {
            _testedInstance.CommandType = commandType;
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.get_CommandType"/>.
        /// </summary>
        [Test]
        public void TestGetCommandType()
        {
            Assert.AreEqual(CommandType.Text, _testedInstance.CommandType);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.Connection"/>.
        /// </summary>
        [Test]
        public void TestConnection()
        {
            DbCommand dbCommand = _testedInstance;

            var connection = new OneSConnection();

            _testedInstance.Connection = connection;

            Assert.AreSame(connection, _testedInstance.Connection);
            Assert.AreSame(connection, dbCommand.Connection);
        }

        /// <summary>
        /// Тестирование <see cref="DbCommand.Connection"/>.
        /// </summary>
        [Test]
        [TestCase(typeof(OneSConnection))]
        [TestCase(typeof(OleDbConnection), ExpectedException = typeof(ArgumentException))]
        public void TestDbConnection(Type connectionType)
        {
            DbCommand dbCommand = _testedInstance;
            var connection = (DbConnection)Activator.CreateInstance(connectionType);

            dbCommand.Connection = connection;

            Assert.AreSame(connection, _testedInstance.Connection);
            Assert.AreSame(connection, dbCommand.Connection);
        }

        /// <summary>
        /// Тестирование <see cref="DbCommand.Parameters"/>.
        /// </summary>
        [Test]
        public void TestDbParameters()
        {
            DbCommand dbCommand = _testedInstance;

            Assert.IsNotNull(dbCommand.Parameters);
            Assert.AreSame(_testedInstance.Parameters, dbCommand.Parameters);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.Transaction"/>
        /// когда значение свойства <see cref="OneSCommand.Connection"/>
        /// не равно <c>null</c>.
        /// </summary>
        [Test]
        public void TestTransactionWhenConnectionIsNotNull([Values(false, true)] bool isNullTransaction)
        {
            // Arrange
            var stateMock = new Mock<MockConnectionState>(MockBehavior.Strict);
            var connection = new OneSConnection(stateMock.Object);
            var transaction = (isNullTransaction)
                ? null
                : new OneSTransaction(connection);

            stateMock
                .SetupGet(s => s.CurrentTransaction)
                .Returns(transaction)
                .Verifiable();

            _testedInstance.Connection = connection;
            DbCommand dbCommand = _testedInstance;

            // Act & Assert
            Assert.AreSame(transaction, _testedInstance.Transaction);
            Assert.AreSame(transaction, dbCommand.Transaction);

            stateMock.VerifyGet(s => s.CurrentTransaction, Times.Exactly(2));
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.Transaction"/>
        /// когда значение свойства <see cref="OneSCommand.Connection"/>
        /// равно <c>null</c>.
        /// </summary>
        [Test]
        public void TestTransactionWhenConnectionIsNull()
        {
            DbCommand dbCommand = _testedInstance;

            Assert.IsNull(_testedInstance.Transaction);
            Assert.IsNull(dbCommand.Transaction);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.DesignTimeVisible"/>.
        /// </summary>
        [Test]
        public void TestDesignTimeVisible([Values(false, true)] bool value)
        {
            _testedInstance.DesignTimeVisible = value;

            Assert.AreEqual(value, _testedInstance.DesignTimeVisible);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.set_UpdatedRowSource"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetUpdatedRowSource([Values(
            UpdateRowSource.None, UpdateRowSource.FirstReturnedRecord, UpdateRowSource.OutputParameters, UpdateRowSource.Both
            )] UpdateRowSource value)
        {
            _testedInstance.UpdatedRowSource = value;
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.get_UpdatedRowSource"/>.
        /// </summary>
        [Test]
        public void TestGetUpdatedRowSource()
        {
            Assert.AreEqual(UpdateRowSource.None, _testedInstance.UpdatedRowSource);
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.Prepare"/>,
        /// что ничего не произойдет.
        /// </summary>
        [Test]
        public void TestPrepare()
        {
            _testedInstance.Prepare();
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSCommand.ExecuteNonQuery"/>,
        /// что при вызове будет выкинуто исключение
        /// <see cref="NotSupportedException"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestExecuteNonQuery()
        {
            _testedInstance.ExecuteNonQuery();
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSCommand.ExecuteScalar"/>,
        /// что при вызове будет выкинуто исключение
        /// <see cref="NotImplementedException"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestExecuteScalar()
        {
            _testedInstance.ExecuteScalar();
        }
    }
}
