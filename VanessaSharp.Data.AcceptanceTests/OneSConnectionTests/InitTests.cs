using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>Приемочные тесты инициализации класса <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class InitTests
    {
        private const string DEFAULT_STRING_VIEW_OF_CONNECTION = "Несвязанное соединение к 1С";

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnection _testedInstance;

        /// <summary>Очистка ресурсов теста.</summary>
        [TearDown]
        public void TearDown()
        {
            if (_testedInstance != null)
            {
                _testedInstance.Dispose();
                _testedInstance = null;
            }
        }

        /// <summary>
        /// Проверка состояния соединения <see cref="OneSConnection.State"/> после инициализации.
        /// Свойство должно быть равным <see cref="ConnectionState.Closed"/>.
        /// </summary>
        private void AssertConnectionStateAfterInitMustbeClosed()
        {
            Assert.AreEqual(ConnectionState.Closed, _testedInstance.State);
        }

        /// <summary>Тестирование <see cref="OneSConnection()"/>.</summary>
        [Test]
        public void TestConstructorWithoutArgs()
        {
            _testedInstance  = new OneSConnection();
            
            Assert.IsNull(_testedInstance.ConnectionString);
            AssertConnectionStateAfterInitMustbeClosed();
            Assert.AreEqual(DEFAULT_STRING_VIEW_OF_CONNECTION, _testedInstance.ToString());
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
            _testedInstance = new OneSConnection(connectionString);

            Assert.AreEqual(connectionString, _testedInstance.ConnectionString);
            AssertConnectionStateAfterInitMustbeClosed();

            var expectedToString = string.IsNullOrEmpty(connectionString)
                                       ? DEFAULT_STRING_VIEW_OF_CONNECTION
                                       : string.Format("Соединение к 1С: {0}", connectionString);
            Assert.AreEqual(expectedToString, _testedInstance.ToString());
        }
    }
}
