using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>
    /// Приемочные тесты на <see cref="OneSDbProviderFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDbProviderFactoryTests
    {
        /// <summary>Тестирование свойства <see cref="OneSDbProviderFactory.Instance"/>.</summary>
        [Test]
        public void TestInstance()
        {
            Assert.IsInstanceOf<OneSDbProviderFactory>(OneSDbProviderFactory.Instance);
        }

        /// <summary>Тестирование метода <see cref="OneSDbProviderFactory.CreateConnectionStringBuilder"/>.</summary>
        [Test]
        public void TestCreateConnectionStringBuilder()
        {
            // Act
            var builder = OneSDbProviderFactory.Instance.CreateConnectionStringBuilder();

            // Assert
            Assert.IsInstanceOf<OneSConnectionStringBuilder>(builder);
        }
    }
}
