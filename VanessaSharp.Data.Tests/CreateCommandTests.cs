using System;
using NUnit.Framework;
using System.Data;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тестирование создания экземпляра класса <see cref="OneSCommand"/>.</summary>
    [TestFixture(Description="Тестирование создания экземпляра класса команды запроса")]
    public sealed class CreateCommandTests : ConnectedTestsBase
    {
        /// <summary>Проверка значений свойств по умолчанию.</summary>
        /// <param name="command">Тестируемая команда.</param>
        private void AssertDefaultProperties(OneSCommand command)
        {
            Assert.AreEqual(0, command.CommandTimeout);
            Assert.AreEqual(CommandType.Text, command.CommandType);
            Assert.AreEqual(false, command.DesignTimeVisible);
            Assert.IsNotNull(command.Parameters);
            Assert.AreEqual(null, command.Transaction);
            Assert.AreEqual(UpdateRowSource.None, command.UpdatedRowSource);
        }

        /// <summary>Тестирование конструктора без аргументов.</summary>
        [Test(Description = "Тестирование конструктора без аргументов")]
        public void TestConstructorWithoutArgs()
        {
            var command = new OneSCommand();

            Assert.AreEqual(null, command.Connection);
            Assert.AreEqual(null, command.CommandText);
            AssertDefaultProperties(command);
        }

        /// <summary>Тестирование конструктора <see cref="OneSCommand(OneSConnection)"/>.</summary>
        [Test(Description = "Тестирование конструктора принимающего соединение")]
        public void TestConstructorWithConnection()
        {
            var connections = new OneSConnection[] { null, Connection };

            foreach (var connection in connections)
            {
                using (var command = new OneSCommand(connection))
                {
                    Assert.AreEqual(connection, command.Connection);
                    Assert.AreEqual(null, command.CommandText);
                    AssertDefaultProperties(command);
                }
            }
        }

        /// <summary>Тестирование конструктора <see cref="OneSCommand(OneSConnection, string)"/>.</summary>
        [Test(Description = "Тестирование конструктора принимающего два аргумента")]
        public void TestConstructorWith2Parameters()
        {
            var connections = new OneSConnection[] { null, Connection };
            var commandTexts = new string[] { null, string.Empty, "SELECT Справочник.Валюты" };

            foreach (var connection in connections)
            {
                foreach (var commandText in commandTexts)
                {
                    using (var command = new OneSCommand(connection, commandText))
                    {
                        Assert.AreEqual(connection, command.Connection);
                        Assert.AreEqual(commandText, command.CommandText);
                        AssertDefaultProperties(command);
                    }
                }
            }
        }
    }
}