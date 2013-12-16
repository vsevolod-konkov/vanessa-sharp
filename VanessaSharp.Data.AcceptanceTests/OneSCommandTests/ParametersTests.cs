using System.Data.Common;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тестирование параметров запроса (экземпляра класса <see cref="OneSCommand"/>).</summary>
    /// <summary>Тестирование свойств экземпляра класса <see cref="OneSCommand"/>.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false, Description = "Тестирование команды запроса без открытия соединения в реальном режиме")]
    [TestFixture(TestMode.Real, true, Description = "Тестирование команды запроса с открытием соединения в реальном режиме")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false, Description = "Тестирование команды запроса без открытия соединения в изоляционном режиме")]
    [TestFixture(TestMode.Isolated, true, Description = "Тестирование команды запроса с открытием соединения в изоляционном режиме")]
    #endif
    [Ignore("Реализация параметризованных тестов отложена")]
    public sealed class ParametersTests : CommandTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        public ParametersTests(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        { }

        /// <summary>Тестирование метода <see cref="OneSCommand.CreateParameter"/>.</summary>
        [Test(Description = "Тестирование метода CreateParameter")]
        public void TestCreateParameter()
        {
            Assert.IsNotNull(TestedCommand.CreateParameter());
        }

        /// <summary>Тестирование метода <see cref="DbCommand.CreateParameter"/>.</summary>
        [Test(Description = "Тестирование метода DbCommand.CreateParameter")]
        public void TestDbCommandCreateParameter()
        {
            DbCommand dbCommand = TestedCommand;
            var result = dbCommand.CreateParameter();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OneSParameter>(result);
        }
    }
}
