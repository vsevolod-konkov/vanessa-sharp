using System;
using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тестирование экземпляра класса <see cref="OneSConnection"/>
    /// когда он находится в открытом состоянии, т.е. значение его свойства
    /// <see cref="OneSConnection.State"/> равно <see cref="ConnectionState.Open"/>.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated)]
    #endif
    public sealed class OpenStateTests : OneSConnectionOpeningTestsBase
    {
        public OpenStateTests(TestMode testMode)
            : base(testMode)
        {}

        /// <summary>
        /// Значение устанавляиваемое для <see cref="OneSConnection.PoolTimeout"/>.
        /// </summary>
        private const int POOL_TIMEOUT = 1000;

        /// <summary>
        /// Значение устанавляиваемое для <see cref="OneSConnection.PoolCapacity"/>.
        /// </summary>
        private const int POOL_CAPACITY = 20;

        /// <summary>Действия выполняемые перед тестом.</summary>
        [SetUp]
        public void OnAfterInitConnection()
        {
            TestedInstance.PoolTimeout = POOL_TIMEOUT;
            TestedInstance.PoolCapacity = POOL_CAPACITY;

            TestedInstance.Open();
        }

        /// <summary>Тестирование свойств соединения.</summary>
        [Test]
        public void TestCommonConnectionProperties()
        {
            Assert.AreEqual(TestConnectionString, TestedInstance.ConnectionString);
            Assert.AreEqual(TestCatalog, TestedInstance.Database);
            Assert.AreEqual(TestCatalog, TestedInstance.DataSource);

            Assert.IsNotEmpty(TestedInstance.ServerVersion);

            Assert.IsFalse(TestedInstance.IsExclusiveMode);
        }

        /// <summary>Тестирование поведения свойства <see cref="OneSConnection.PoolTimeout"/>.</summary>
        [Test]
        public void TestPoolTimeout()
        {
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.PoolTimeout);
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.ConnectionTimeout);

            // Изменение свойства в открытом состоянии недопустимо
            Assert.Throws<InvalidOperationException>(() =>
                    TestedInstance.PoolTimeout = POOL_TIMEOUT + 1000);
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.PoolTimeout);
            Assert.AreEqual(POOL_TIMEOUT, TestedInstance.ConnectionTimeout);
        }

        /// <summary>Тестирование поведения свойства <see cref="OneSConnection.PoolCapacity"/>.</summary>
        [Test]
        public void TestPoolCapacity()
        {
            Assert.AreEqual(POOL_CAPACITY, TestedInstance.PoolCapacity);

            // Изменение свойства в открытом состоянии недопустимо
            Assert.Throws<InvalidOperationException>(() =>
                    TestedInstance.PoolCapacity = POOL_CAPACITY + 2);
            Assert.AreEqual(POOL_CAPACITY, TestedInstance.PoolCapacity);
        }

        // TODO: Исправить ошибку в тесте
        /// <summary>Тестирование свойства <see cref="isExclusiveMode"/>.</summary>
        /// <param name="isExclusiveMode">Значение свойства.</param>
        [Ignore("Тест выдает ошибку")]
        [Test]
        [TestCase(false, Description = "Допустимо открывать еще одно соединение.")]
        [TestCase(true, ExpectedException = typeof(InvalidOperationException), Description = "Недопустимо открытие еще одного соединения к той же БД")]
        public void TestIsExclusiveMode(bool isExclusiveMode)
        {
            TestedInstance.IsExclusiveMode = isExclusiveMode;
            Assert.AreEqual(isExclusiveMode, TestedInstance.IsExclusiveMode);

            using (var otherConnection = GetOtherConnection())
            {
                otherConnection.Open();
            }
        }

        /// <summary>
        /// Получение другого соединения.
        /// </summary>
        /// <returns></returns>
        private OneSConnection GetOtherConnection()
        {
            try
            {
                var connectionStringBuilder = new OneSConnectionStringBuilder { ConnectionString = TestConnectionString };
                connectionStringBuilder.User = "Петрова (гл. бухгалтер)";

                return new OneSConnection(connectionStringBuilder.ConnectionString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
