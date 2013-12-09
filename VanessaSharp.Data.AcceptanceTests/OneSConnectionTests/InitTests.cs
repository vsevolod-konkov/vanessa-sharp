using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>Приемочные тесты инициализации класса <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class InitTests : OneSConnectionTestsBase
    {
        private const string DEFAULT_STRING_VIEW_OF_CONNECTION = "Несвязанное соединение к 1С";
        
        /// <summary>
        /// Проверка состояния соединения <see cref="OneSConnection.State"/> после инициализации.
        /// Свойство должно быть равным <see cref="ConnectionState.Closed"/>.
        /// </summary>
        private void AssertConnectionStateAfterInitMustbeClosed()
        {
            Assert.AreEqual(ConnectionState.Closed, TestedInstance.State);
        }

        /// <summary>Тестирование <see cref="OneSConnection()"/>.</summary>
        [Test]
        public void TestConstructorWithoutArgs()
        {
            TestedInstance  = new OneSConnection();
            
            Assert.IsNull(TestedInstance.ConnectionString);
            AssertConnectionStateAfterInitMustbeClosed();
            Assert.AreEqual(DEFAULT_STRING_VIEW_OF_CONNECTION, TestedInstance.ToString());
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnection(string)"/>
        /// с валидной строкой соединения.
        /// </summary>
        [Test]
        [TestCase(null, Description = "Передача null допустима")]
        [TestCase("", Description = "Передача пустой строки допустима")]
        [TestCase(@"File=C:\1С", Description = "Передача валидной строки соединения допустима")]
        [TestCase("белеберда", Description = "Передача невалидной строки соединения допустима")]
        public void TestConstructorWithConnectionStringArg(string connectionString)
        {
            TestedInstance = new OneSConnection(connectionString);

            Assert.AreEqual(connectionString, TestedInstance.ConnectionString);
            AssertConnectionStateAfterInitMustbeClosed();

            var expectedToString = string.IsNullOrEmpty(connectionString)
                                       ? DEFAULT_STRING_VIEW_OF_CONNECTION
                                       : string.Format("Соединение к 1С: {0}", connectionString);
            Assert.AreEqual(expectedToString, TestedInstance.ToString());
        }
    }
}
