using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестирование создания экземпляра класса <see cref="OneSCommand"/>.</summary>
    [TestFixture(false, Description = "Тестирование команды запроса без открытия соединения")]
    [TestFixture(true, Description = "Тестирование команды запроса с открытием соединения")]
    public sealed class CommandTests : ConnectedTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        public CommandTests(bool shouldBeOpen) : base(shouldBeOpen)
        {}

        /// <summary>Тестовый экземпляр команды.</summary>
        private OneSCommand TestCommand
        {
            get { return _testCommand; }
        }
        private OneSCommand _testCommand;

        /// <summary>Установка тестового окружения.</summary>
        /// <remarks>Создание тестовой команды <see cref="TestCommand"/>.</remarks>
        protected override void InternalSetUp()
        {
            base.InternalSetUp();

            _testCommand = new OneSCommand(Connection);
        }

        /// <summary>Очистка окружения тестов.</summary>
        /// <remarks>Очистка тестовой команды <see cref="TestCommand"/>.</remarks>
        protected override void InternalTearDown()
        {
            if (_testCommand != null)
                _testCommand.Dispose();

            base.InternalTearDown();
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandText"/>.</summary>
        [Test(Description = "Тестирование свойства CommandText")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("SELECT Справочник.Валюты")]
        public void TestCommandTextProperty(string commandText)
        {
            TestCommand.CommandText = commandText;
            Assert.AreEqual(commandText, TestCommand.CommandText);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandType"/>.</summary>
        [Test(Description = "Тестирование значений свойства CommandType")]
        [TestCase(CommandType.Text, Result = CommandType.Text)]
        [TestCase(CommandType.TableDirect, ExpectedException = typeof(NotSupportedException))]
        [TestCase(CommandType.StoredProcedure, ExpectedException = typeof(NotSupportedException))]
        public CommandType TestCommandType(CommandType commandType)
        {
            TestCommand.CommandType = commandType;
            return TestCommand.CommandType;
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.CommandTimeout"/>.</summary>
        [Test(Description = "Тестирование свойства CommandTimeout")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCommandTimeout([Random(0, 10000, 5)] int value)
        {
            TestCommand.CommandTimeout = value;
        }

        /// <summary>Тестирование метода <see cref="OneSCommand.CreateParameter"/>.</summary>
        [Test(Description = "Тестирование метода CreateParameter")]
        [Ignore("Реализация параметризованных тестов отложена")]
        public void TestCreateParameter()
        {
            Assert.IsNotNull(TestCommand.CreateParameter());
        }

        /// <summary>Тестирование метода <see cref="DbCommand.CreateParameter"/>.</summary>
        [Test(Description = "Тестирование метода DbCommand.CreateParameter")]
        [Ignore("Реализация параметризованных тестов отложена")]
        public void TestDbCommandCreateParameter()
        {
            DbCommand dbCommand = TestCommand;
            var result = dbCommand.CreateParameter();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OneSParameter>(result);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.Connection"/>.</summary>
        [Test(Description = "Тестирование свойства Connection")]
        public void TestConnection()
        {
            Assert.AreSame(Connection, TestCommand.Connection);

            using (var testConnection = new OneSConnection())
            {
                TestCommand.Connection = testConnection;
                Assert.AreSame(testConnection, TestCommand.Connection);
            }

            TestCommand.Connection = null;
            Assert.IsNull(TestCommand.Connection);
        }

        /// <summary>Тестирование свойства <see cref="DbCommand.Connection"/>.</summary>
        [Test(Description = "Тестирование свойства DbCommand.Connection")]
        public void TestDbConnection()
        {
            DbCommand dbCommand = TestCommand;

            Assert.AreSame(Connection, dbCommand.Connection);

            using (var testConnection = new OneSConnection())
            {
                dbCommand.Connection = testConnection;
                Assert.AreSame(testConnection, dbCommand.Connection);
            }

            dbCommand.Connection = null;
            Assert.IsNull(TestCommand.Connection);

            using (var oleDbConnection = new OleDbConnection())
                Assert.Throws<ArgumentException>(() => dbCommand.Connection = oleDbConnection);
        }

        /// <summary>Тестирование свойства <see cref="DbCommand.Parameters"/>.</summary>
        [Test(Description = "Тестирование свойства DbCommand.Parameters")]
        public void TestDbParameters()
        {
            DbCommand dbCommand = TestCommand;

            Assert.IsNotNull(dbCommand.Parameters);
            Assert.IsInstanceOf<OneSParameterCollection>(dbCommand.Parameters);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.DesignTimeVisible"/>.</summary>
        [Test(Description = "Тестирование свойства DesignTimeVisible")]
        public void TestDesignTimeVisible([Values(false, true)] bool value)
        {
            TestCommand.DesignTimeVisible = value;
            Assert.AreEqual(value, TestCommand.DesignTimeVisible);
        }

        /// <summary>Тестирование свойства <see cref="OneSCommand.UpdatedRowSource"/>.</summary>
        [Test(Description = "Тестирование свойства UpdatedRowSource")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUpdatedRowSource(
            [Values(UpdateRowSource.None, UpdateRowSource.Both, UpdateRowSource.FirstReturnedRecord, UpdateRowSource.OutputParameters)] UpdateRowSource value)
        {
            TestCommand.UpdatedRowSource = value;
        }
    }
}
