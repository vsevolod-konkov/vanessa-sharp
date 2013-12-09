using System;
using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тестирование класса <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class ConnectionTests : TestsBase
    {
        /// <summary>Тестирование свойств соединения.</summary>
        [Test(Description="Тестирование общих свойств соединения")]
        public void TestCommonConnectionProperties()
        {
            using (var connection = new OneSConnection())
            {
                connection.ConnectionString = TestConnectionString;

                // Открываем соединение
                connection.Open();

                Assert.AreEqual(TestConnectionString, connection.ConnectionString);
                Assert.AreEqual(Constants.TestCatalog, connection.Database);
                Assert.AreEqual(Constants.TestCatalog, connection.DataSource);
                Assert.IsNotEmpty(connection.ServerVersion);
                Assert.AreEqual(0, connection.ConnectionTimeout);
            }
        }

        /// <summary>Тестирование специальных свойств соединения.</summary>
        [Test(Description="Тестирование специальных свойств соединения")]
        public void TestCustomProperties()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                connection.PoolTimeout = 1000;

                connection.Open();
                Assert.AreEqual(1000, connection.PoolTimeout);
                Assert.AreEqual(1000, connection.ConnectionTimeout);

                Assert.Throws<InvalidOperationException>(() =>
                    connection.PoolTimeout = 2000);
                Assert.AreEqual(1000, connection.PoolTimeout);
                Assert.AreEqual(1000, connection.ConnectionTimeout);
            }

            using (var connection = new OneSConnection(TestConnectionString))
            {
                connection.PoolCapacity = 20;

                connection.Open();
                Assert.AreEqual(20, connection.PoolCapacity);

                Assert.Throws<InvalidOperationException>(() =>
                    connection.PoolCapacity = 10);
                Assert.AreEqual(20, connection.PoolCapacity);
            }
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test(Description="Тестирование монопольного доступа")]
        public void TestIsExclusiveMode()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // Открываем соединение, свойство становится доступно.
                connection.Open();

                // По умолчанию немонопольный режим
                Assert.IsFalse(connection.IsExclusiveMode);
                connection.IsExclusiveMode = true;
                Assert.IsTrue(connection.IsExclusiveMode);

                var builder = new OneSConnectionStringBuilder();
                builder.ConnectionString = TestConnectionString;
                builder.User = "Петров";
                using (var connection2 = new OneSConnection(builder.ConnectionString))
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        connection2.Open();
                    });
                }

                // Монопольный режим сняли и второе соединение становится возможным.
                connection.IsExclusiveMode = false;
                var connection3 = new OneSConnection(builder.ConnectionString);
                connection3.Dispose();
            }
        }
    }
}
