using System;
using NUnit.Framework;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тестирование свойств экземпляра класса <see cref="OneSCommand"/>.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false, Description = "Тестирование команды запроса без открытия соединения в реальном режиме")]
    [TestFixture(TestMode.Real, true, Description = "Тестирование команды запроса с открытием соединения в реальном режиме")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false, Description = "Тестирование команды запроса без открытия соединения в изоляционном режиме")]
    [TestFixture(TestMode.Isolated, true, Description = "Тестирование команды запроса с открытием соединения в изоляционном режиме")]
    #endif
    public sealed class PropertiesTests : CommandTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        public PropertiesTests(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {}

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandText"/>.</summary>
        [Test(Description = "Тестирование свойства CommandText")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("SELECT Справочник.Валюты")]
        public void TestCommandTextProperty(string commandText)
        {
            TestedCommand.CommandText = commandText;
            Assert.AreEqual(commandText, TestedCommand.CommandText);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandType"/>.</summary>
        [Test(Description = "Тестирование значений свойства CommandType")]
        [TestCase(CommandType.Text, Result = CommandType.Text)]
        [TestCase(CommandType.TableDirect, ExpectedException = typeof(NotSupportedException))]
        [TestCase(CommandType.StoredProcedure, ExpectedException = typeof(NotSupportedException))]
        public CommandType TestCommandType(CommandType commandType)
        {
            TestedCommand.CommandType = commandType;
            return TestedCommand.CommandType;
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandTimeout"/>.</summary>
        [Test(Description = "Тестирование свойства CommandTimeout")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCommandTimeout([Random(0, 10000, 5)] int value)
        {
            TestedCommand.CommandTimeout = value;
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.Connection"/>.</summary>
        [Test(Description = "Тестирование свойства Connection")]
        public void TestConnection()
        {
            Assert.AreSame(Connection, TestedCommand.Connection);

            using (var testConnection = new OneSConnection())
            {
                TestedCommand.Connection = testConnection;
                Assert.AreSame(testConnection, TestedCommand.Connection);
            }

            TestedCommand.Connection = null;
            Assert.IsNull(TestedCommand.Connection);
        }

        /// <summary>Тестирование свойства <see cref="DbCommand.Connection"/>.</summary>
        [Test(Description = "Тестирование свойства DbCommand.Connection")]
        public void TestDbConnection()
        {
            DbCommand dbCommand = TestedCommand;

            Assert.AreSame(Connection, dbCommand.Connection);

            using (var testConnection = new OneSConnection())
            {
                dbCommand.Connection = testConnection;
                Assert.AreSame(testConnection, dbCommand.Connection);
            }

            dbCommand.Connection = null;
            Assert.IsNull(TestedCommand.Connection);

            using (var oleDbConnection = new OleDbConnection())
                Assert.Throws<ArgumentException>(() => dbCommand.Connection = oleDbConnection);
        }

        /// <summary>Тестирование свойства <see cref="DbCommand.Parameters"/>.</summary>
        [Test(Description = "Тестирование свойства DbCommand.Parameters")]
        public void TestDbParameters()
        {
            DbCommand dbCommand = TestedCommand;

            Assert.IsNotNull(dbCommand.Parameters);
            Assert.IsInstanceOf<OneSParameterCollection>(dbCommand.Parameters);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.DesignTimeVisible"/>.</summary>
        [Test(Description = "Тестирование свойства DesignTimeVisible")]
        public void TestDesignTimeVisible([Values(false, true)] bool value)
        {
            TestedCommand.DesignTimeVisible = value;
            Assert.AreEqual(value, TestedCommand.DesignTimeVisible);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.UpdatedRowSource"/>.</summary>
        [Test(Description = "Тестирование свойства UpdatedRowSource")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUpdatedRowSource(
            [Values(UpdateRowSource.None, UpdateRowSource.Both, UpdateRowSource.FirstReturnedRecord, UpdateRowSource.OutputParameters)] UpdateRowSource value)
        {
            TestedCommand.UpdatedRowSource = value;
        }
    }
}
