using System;
using System.Data;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тесты экземпляра <see cref="OneSConnection"/>
    /// на переходы между состояниями <see cref="OneSConnection.State"/>.
    /// .</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated)]
    #endif
    public sealed class TransitionTests : OneSConnectionTestsBase
    {
        public TransitionTests(TestMode testMode)
            : base(testMode)
        {}

        /// <summary>
        /// Тестирование метода <see cref="OneSConnection.Open"/>
        /// после инициализации.
        /// </summary>
        [Test]
        public void TestOpenAfterInit()
        {
            TestedInstance.Open();

            Assert.AreEqual(ConnectionState.Open, TestedInstance.State);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSConnection.Close"/>,
        /// после открытия соединиея.
        /// </summary>
        [Test]
        public void TestCloseAfterOpen()
        {
            TestedInstance.Open();
            TestedInstance.Close();

            Assert.AreEqual(ConnectionState.Closed, TestedInstance.State);
        }

        /// <summary>Тестирование того, что вызов метода <see cref="IDisposable.Dispose()"/> для открытого соединения приведет к его закрытию.</summary>
        [Test(Description = "Тестирование того, что вызов Dispose у открытого соединения, закроет его.")]
        public void TestCallDisposeCloseConnection()
        {
            TestedInstance.Open();
            TestedInstance.Dispose();

            Assert.AreEqual(ConnectionState.Closed, TestedInstance.State);
        }
    }
}
