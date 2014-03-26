using System;
using System.Data;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тесты на случаи когда экземпляр 
    /// <see cref="OneSConnection"/> имеет 
    /// состояние <see cref="OneSConnection.State"/>
    /// в значении <see cref="ConnectionState.Closed"/>
    /// в любом тестовом режиме.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public sealed class ClosedStateTests : ClosedStateTestsBase
    {
        /// <summary>Параметрический конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        /// <param name="hadOpened">Было ли открыто соединение.</param>
        public ClosedStateTests(TestMode testMode, bool hadOpened)
            : base(testMode, hadOpened)
        {}

        /// <summary>
        /// Тесты на общие свойства.
        /// </summary>
        [Test]
        public void TestGeneralProperties()
        {
            Assert.AreEqual(TestConnectionString, TestedInstance.ConnectionString);
            Assert.AreEqual(TestCatalog, TestedInstance.Database);
            Assert.AreEqual(TestCatalog, TestedInstance.DataSource);
            Assert.AreEqual(0, TestedInstance.ConnectionTimeout);
        }

        /// <summary>
        /// Тесты на общие свойства строка соединения пуста или <c>null</c>.
        /// </summary>
        [Test]
        public void TestGeneralPropertiesWhenConnectionStringNullOrEmpty(
            [Values(null, "")] string connectionString)
        {
            TestedInstance.ConnectionString = connectionString;

            Assert.AreEqual(connectionString, TestedInstance.ConnectionString);
            Assert.AreEqual(string.Empty, TestedInstance.Database);
            Assert.AreEqual(string.Empty, TestedInstance.DataSource);
            Assert.AreEqual(0, TestedInstance.ConnectionTimeout);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.Open"/>.</summary>
        [Test]
        public void TestOpen()
        {
            TestedInstance.Open();

            Assert.AreEqual(ConnectionState.Open, TestedInstance.State);
        }

        /// <summary>
        /// Тестирование получения свойства,
        /// в случае передачи невалидной строки соединения.
        /// </summary>
        /// <param name="propertyGetter">Получатель свойства соединения</param>
        private void TestGetPropertyWhenInvalidConnectionString(
            Func<OneSConnection, string> propertyGetter)
        {
            TestActionWhenInvalidConnectionString(() => 
                Assert.AreEqual(string.Empty, propertyGetter(TestedInstance)));
        }

        /// <summary>
        /// Тестирование получения свойства <see cref="OneSConnection.Database"/>
        /// в случае передачи невалидной строки соединения.
        /// </summary>
        [Test]
        public void TestDatabaseWhenInvalidConnectionString()
        {
            TestGetPropertyWhenInvalidConnectionString(c => c.Database);
        }

        /// <summary>
        /// Тестирование получения свойства <see cref="OneSConnection.DataSource"/>
        /// в случае передачи невалидной строки соединения.
        /// </summary>
        [Test]
        public void TestDataSourceWhenInvalidConnectionString()
        {
            TestGetPropertyWhenInvalidConnectionString(c => c.DataSource);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.PoolTimeout"/>.</summary>
        [Test]
        public void TestPoolTimeout()
        {
            const int POOL_TIMEOUT = 1000;
            TestedInstance.PoolTimeout = POOL_TIMEOUT;
            
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.PoolTimeout);
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.ConnectionTimeout);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.PoolCapacity"/>.</summary>
        [Test]
        public void TestPoolCapacity()
        {
            const int POOL_CAPACITY = 20;
            TestedInstance.PoolCapacity = POOL_CAPACITY;
            
            Assert.AreEqual(20, TestedInstance.PoolCapacity);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnection.IsExclusiveMode"/>.</summary>
        [Test]
        public void TestIsExclusiveMode()
        {
            // Если соединение не открыто, то свойство IsExclusiveMode недоступно
            Assert.Throws<InvalidOperationException>(() => 
                Assert.IsFalse(TestedInstance.IsExclusiveMode));

            Assert.Throws<InvalidOperationException>(() =>
            {
                TestedInstance.IsExclusiveMode = true;
            });
        }
    }
}
