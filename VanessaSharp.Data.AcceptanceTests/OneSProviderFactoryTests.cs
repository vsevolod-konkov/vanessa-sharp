using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>
    /// Приемочные тесты на <see cref="OneSProviderFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSProviderFactoryTests
    {
        /// <summary>Тестирование свойства <see cref="OneSProviderFactory.Instance"/>.</summary>
        [Test]
        public void TestInstance()
        {
            Assert.IsInstanceOf<OneSProviderFactory>(OneSProviderFactory.Instance);
        }

        /// <summary>Тестирование метода <see cref="OneSProviderFactory.CreateConnectionStringBuilder"/>.</summary>
        [Test]
        public void TestCreateConnectionStringBuilder()
        {
            // Act
            var builder = OneSProviderFactory.Instance.CreateConnectionStringBuilder();

            // Assert
            Assert.IsInstanceOf<OneSConnectionStringBuilder>(builder);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProviderFactory.CreateConnection"/>.
        /// </summary>
        [Test]
        public void TestCreateConnection()
        {
            // Act
            var connection = OneSProviderFactory.Instance.CreateConnection();

            // Assert
            Assert.IsInstanceOf<OneSConnection>(connection);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProviderFactory.CreateCommand"/>.
        /// </summary>
        [Test]
        public void TestCreateCommand()
        {
            // Act
            var command = OneSProviderFactory.Instance.CreateCommand();

            // Assert
            Assert.IsInstanceOf<OneSCommand>(command);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProviderFactory.CreateDataAdapter"/>.
        /// </summary>
        [Test]
        public void TestCreateDataAdapter()
        {
            // Act
            var dataAdapter = OneSProviderFactory.Instance.CreateDataAdapter();

            // Assert
            Assert.IsInstanceOf<OneSDataAdapter>(dataAdapter);
        }
    }
}
