using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тесты проверяющие правильность выполнения запросов команды.</summary>
    [TestFixture]
    public sealed class CommandExecuteTests : ConnectedTestsBase
    {
        /// <summary>Установка окружения тестов.</summary>
        /// <remarks>Точка расширения для наследных классов.</remarks>
        protected override void InternalSetUp()
        {
            base.InternalSetUp();

            const string sql = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";

            _testCommand = new OneSCommand(Connection);
            _testCommand.CommandText = sql;
        }

        /// <summary>Очистка окружения тестов.</summary>
        /// <remarks>Точка расширения для наследных классов.</remarks>
        protected override void InternalTearDown()
        {
            _testCommand.Dispose();
            _testCommand = null;

            base.InternalTearDown();
        }

        /// <summary>Тестовая команда.</summary>
        private OneSCommand _testCommand;

        /// <summary>
        /// Тестирование выполнения простого запроса.
        /// </summary>
        [Test(Description="Тестирование выполнения простого запроса")]
        public void TestCommandSimpleExecute()
        {
            _testCommand.ExecuteReader();
        }

        /// <summary>
        /// Проверка того что поведение команды не по умолчанию не поддерживается.
        /// </summary>
        /// <param name="behavior"></param>
        [Test(Description="Проверка того что поведение команды не по умолчанию не поддерживается")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestNotSupportedNonDefaultCommandBehavior(
            [Values(CommandBehavior.SingleRow, CommandBehavior.SingleResult, CommandBehavior.SequentialAccess, 
                    CommandBehavior.SchemaOnly, CommandBehavior.KeyInfo, CommandBehavior.CloseConnection)]
            CommandBehavior behavior)
        {
            _testCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> равно <c>null</c>.
        /// </summary>
        [Test(Description="Тестирование того, что при не заданном подключении, будет исключение InvalidOperationException")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidOperationIfNullConnection()
        {
            _testCommand.Connection = null;
            _testCommand.ExecuteReader();
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> находится в закрытом состоянии,
        /// то есть <see cref="OneSConnection.State"/> равно <see cref="ConnectionState.Closed"/>.
        /// </summary>
        [Test(Description = "Тестирование факта выброса исключения InvalidOperationException в случае если соединение находится в закрытом состоянии")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidOperationIfClosedConnection()
        {
            _testCommand.Connection.Close();
            _testCommand.ExecuteReader();
        }
    }
}
