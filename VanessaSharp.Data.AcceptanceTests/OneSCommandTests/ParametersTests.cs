using System.Data.Common;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тестирование параметров запроса (экземпляра класса <see cref="OneSCommand"/>).</summary>
    [TestFixture(false, Description = "Тестирование команды запроса без открытия соединения")]
    [TestFixture(true, Description = "Тестирование команды запроса с открытием соединения")]
    [Ignore("Реализация параметризованных тестов отложена")]
    public sealed class ParametersTests : CommandTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        public ParametersTests(bool shouldBeOpen)
            : base(shouldBeOpen)
        {}

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
