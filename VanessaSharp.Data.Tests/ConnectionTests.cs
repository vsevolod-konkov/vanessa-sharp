using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тестирование класса <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class ConnectionTests : TestsBase
    {
        /// <summary>Тестирование инициализации соединения.</summary>
        [Test(Description = "Тестирование инициализации соединения")]
        public void TestInitConnection()
        {
            // По умолчанию строка равна null
            using (var connection = new OneSConnection())
            {
                Assert.IsNull(connection.ConnectionString);
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }
            
            // Проверка задания нормальной строки соединения
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = @"C:\1C";
                using(var connection = new OneSConnection(builder.ConnectionString))
                {
                   Assert.AreEqual(builder.ConnectionString, connection.ConnectionString);
                   Assert.AreEqual(ConnectionState.Closed, connection.State);
                }
            }

            // Допускается в конструкторе задать строку соединения равной null
            using (var connection = new OneSConnection(null))
            {
                Assert.IsNull(connection.ConnectionString);
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }

            // Допускается в конструкторе задать пустую строку
            using (var connection = new OneSConnection(string.Empty))
            {
                Assert.AreEqual(string.Empty, connection.ConnectionString);
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }

            // Вообще разрешается задать всякую чушь в строке соединения 
            using (var connection = new OneSConnection("белеберда"))
            {
                Assert.AreEqual("белеберда", connection.ConnectionString);
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.Open"/>.</summary>
        [Test(Description="Тестирование метода Open")]
        public void TestConnectionOpen()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                Assert.AreEqual(ConnectionState.Closed, connection.State);

                connection.Open();
                Assert.AreEqual(ConnectionState.Open, connection.State);

                connection.Close();
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }
        }

        /// <summary>Тестирование свойств соединения.</summary>
        [Test(Description="Тестирование общих свойств соединения")]
        public void TestCommonConnectionProperties()
        {
            using (var connection = new OneSConnection())
            {
                connection.ConnectionString = TestConnectionString;

                Assert.AreEqual(TestConnectionString, connection.ConnectionString);
                Assert.AreEqual(Constants.TestCatalog, connection.Database);
                Assert.AreEqual(Constants.TestCatalog, connection.DataSource);
                Assert.AreEqual(0, connection.ConnectionTimeout);
                
                // Открываем соединение
                connection.Open();

                Assert.AreEqual(TestConnectionString, connection.ConnectionString);
                Assert.AreEqual(Constants.TestCatalog, connection.Database);
                Assert.AreEqual(Constants.TestCatalog, connection.DataSource);
                Assert.IsNotEmpty(connection.ServerVersion);
                Assert.AreEqual(0, connection.ConnectionTimeout);
            }
        }

        /// <summary>Тестирование поведения соединения при задании не валидной строки соединения.</summary>
        [Test(Description = "Тестирование поведения соединения при задании не валидной строки соединения")]
        public void TestInvalidConnectionString()
        {
            using (var connection = new OneSConnection("белеберда"))
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    var catalog = connection.Database;
                });

                Assert.Throws<InvalidOperationException>(() =>
                {
                    var catalog = connection.DataSource;
                });

                Assert.Throws<InvalidOperationException>(connection.Open);
            }
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.ChangeDatabase"/>.</summary>
        [Test(Description="Тестирование метода ChangeDatabase")]
        public void TestChangeDatabase()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // пока не реализовано
                Assert.Throws<NotSupportedException>(() =>
                {
                    // Не поддерживается, даже если передать тот же самый каталог.
                    connection.ChangeDatabase(Constants.TestCatalog);
                });
            }
        }

        /// <summary>Тестирование специальных свойств соединения.</summary>
        [Test(Description="Тестирование специальных свойств соединения")]
        public void TestCustomProperties()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                connection.PoolTimeout = 1000;
                Assert.AreEqual(1000, connection.PoolTimeout);
                Assert.AreEqual(1000, connection.ConnectionTimeout);

                connection.Open();
                Assert.AreEqual(1000, connection.PoolTimeout);
                Assert.AreEqual(1000, connection.ConnectionTimeout);

                Assert.Throws<InvalidOperationException>(() =>
                    connection.PoolTimeout = 2000);
                Assert.AreEqual(1000, connection.PoolTimeout);
                Assert.AreEqual(1000, connection.ConnectionTimeout);

                connection.Close();
                connection.PoolTimeout = 2000;
                Assert.AreEqual(2000, connection.PoolTimeout);
                Assert.AreEqual(2000, connection.ConnectionTimeout);
            }

            using (var connection = new OneSConnection(TestConnectionString))
            {
                connection.PoolCapacity = 20;
                Assert.AreEqual(20, connection.PoolCapacity);

                connection.Open();
                Assert.AreEqual(20, connection.PoolCapacity);

                Assert.Throws<InvalidOperationException>(() =>
                    connection.PoolCapacity = 10);
                Assert.AreEqual(20, connection.PoolCapacity);

                connection.Close();
                connection.PoolCapacity = 10;
                Assert.AreEqual(10, connection.PoolCapacity);
            }
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test(Description="Тестирование монопольного доступа")]
        public void TestIsExclusiveMode()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // Если соединение не открыто, то свойство IsExclusiveMode недоступно
                Assert.Throws<InvalidOperationException>(() =>
                {
                    var value = connection.IsExclusiveMode;
                });

                Assert.Throws<InvalidOperationException>(() =>
                {
                    connection.IsExclusiveMode = true;
                });

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

        /// <summary>Тестирование метода <see cref="OneSConnection.CreateCommand"/>.</summary>
        [Test(Description = "Тестирование типизированного метода создания команды")]
        public void TestTypedCreateCommand()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                var command = connection.CreateCommand();
                Assert.IsNotNull(command);
                Assert.AreSame(connection, command.Connection);
            }
        }

        /// <summary>Тестирование метода <see cref="DbConnection.CreateCommand"/>.</summary>
        [Test(Description = "Тестирование реализации обобщенного метода создания команды")]
        public void TestUntypedCreateCommand()
        {
            using (DbConnection connection = new OneSConnection(TestConnectionString))
            {
                var command = connection.CreateCommand();
                Assert.IsNotNull(command);
                Assert.AreSame(connection, command.Connection);
            }
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.ToString()"/>.</summary>
        [Test(Description = "Тестирование перегрузки метода ToString")]
        public void TestToString()
        {
            using (var connection = new OneSConnection())
                Assert.AreEqual("Несвязанное соединение к 1С", connection.ToString());

            using (var connection = new OneSConnection(TestConnectionString))
                Assert.AreEqual(string.Format("Соединение к 1С: {0}", connection.ConnectionString), connection.ToString());
        }

        /// <summary>Тестирование того, что вызов метода <see cref="IDisposable.Dispose()"/> для открытого соединения приведет к его закрытию.</summary>
        [Test(Description = "Тестирование того, что вызов Dispose у открытого соединения, закроет его.")]
        public void TestCallDisposeCloseConnection()
        {
            using (var testConnection = new OneSConnection(TestConnectionString))
            {
                testConnection.Open();
                Assert.AreEqual(ConnectionState.Open, testConnection.State);

                testConnection.Dispose();
                Assert.AreEqual(ConnectionState.Closed, testConnection.State);
            }
        }
    }
}
