using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестирование класса <see cref="OneSConnection"/>.</summary>
    [TestFixture]
    public sealed class ConnectionTests
    {
        /// <summary>Тестирование инициализации соединения.</summary>
        [Test(Description = "Тестирование инициализации соединения")]
        public void TestInitConnection()
        {
            const string catalog = @"C:\1C";

            // По умолчанию строка равна null
            using (var connection = new OneSConnection())
            {
                Trace.WriteLine(connection.ConnectionString);
                
                Assert.IsNull(connection.ConnectionString);
                Assert.AreEqual(ConnectionState.Closed, connection.State);
            }
            
            // Проверка задания нормальной строки соединения
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = catalog;
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
                Assert.AreEqual(Settings.TestCatalog, connection.Database);
                Assert.AreEqual(Settings.TestCatalog, connection.DataSource);
                Assert.IsNotEmpty(connection.ServerVersion);
                Assert.AreEqual(0, connection.ConnectionTimeout);
                
                // Открываем соединение
                connection.Open();

                Assert.AreEqual(TestConnectionString, connection.ConnectionString);
                Assert.AreEqual(Settings.TestCatalog, connection.Database);
                Assert.AreEqual(Settings.TestCatalog, connection.DataSource);
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
                ChecksHelper.AssertException<InvalidOperationException>(() =>
                {
                    var catalog = connection.Database;
                });

                ChecksHelper.AssertException<InvalidOperationException>(() =>
                {
                    var catalog = connection.DataSource;
                });

                ChecksHelper.AssertException<InvalidOperationException>(connection.Open);
            }
        }

        /// <summary>Тестирование метода <see cref="DbConnection.BeginTransaction()"/>.</summary>
        [Test(Description="Тестирование метода BeginTransaction")]
        public void TestBeginTransaction()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // пока не реализовано
                ChecksHelper.AssertException<NotImplementedException>(() =>
                {
                    var transaction = connection.BeginTransaction();
                });
            }
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.ChangeDatabase"/>.</summary>
        [Test(Description="Тестирование метода ChangeDatabase")]
        public void TestChangeDatabase()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // пока не реализовано
                ChecksHelper.AssertException<NotSupportedException>(() =>
                {
                    // Не поддерживается, даже если передать тот же самый каталог.
                    connection.ChangeDatabase(Settings.TestCatalog);
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

                connection.PoolTimeout = 2000;
                Assert.AreEqual(2000, connection.PoolTimeout);
                Assert.AreEqual(2000, connection.ConnectionTimeout);

                connection.PoolTimeout = null;
                Assert.AreEqual(2000, connection.PoolTimeout);
                Assert.AreEqual(2000, connection.ConnectionTimeout);

                connection.Close();
                Assert.AreEqual(2000, connection.PoolTimeout);
                Assert.AreEqual(2000, connection.ConnectionTimeout);

                connection.PoolTimeout = null;
                Assert.IsNull(connection.PoolTimeout);
                Assert.AreEqual(0, connection.ConnectionTimeout);
            }

            using (var connection = new OneSConnection(TestConnectionString))
            {
                connection.PoolCapacity = 20;
                Assert.AreEqual(20, connection.PoolCapacity);

                connection.Open();
                Assert.AreEqual(20, connection.PoolCapacity);

                connection.PoolCapacity = 10;
                Assert.AreEqual(10, connection.PoolCapacity);

                connection.PoolCapacity = null;
                Assert.AreEqual(10, connection.PoolCapacity);
            
                connection.Close();
                Assert.AreEqual(10, connection.PoolCapacity);

                connection.PoolCapacity = null;
                Assert.IsNull(connection.PoolCapacity);
            }
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test(Description="Тестирование монопольного доступа")]
        public void TestIsExclusiveMode()
        {
            using (var connection = new OneSConnection(TestConnectionString))
            {
                // Если соединение не открыто, то свойство IsExclusiveMode недоступно
                ChecksHelper.AssertException<InvalidOperationException>(() =>
                {
                    var value = connection.IsExclusiveMode;
                });

                ChecksHelper.AssertException<InvalidOperationException>(() =>
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
                    ChecksHelper.AssertException<InvalidOperationException>(() =>
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

        /// <summary>Настройки.</summary>
        private Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        private string TestConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Settings.TestCatalog;
                builder.User = Settings.TestUser;

                return builder.ConnectionString;
            }
        }
    }
}
