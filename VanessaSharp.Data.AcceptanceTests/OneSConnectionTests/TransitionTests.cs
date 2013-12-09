﻿using System;
using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тесты экземпляра <see cref="OneSConnection"/>
    /// на переходы между состояниями <see cref="OneSConnection.State"/>.
    /// .</summary>
    [TestFixture]
    public sealed class TransitionTests : OneSConnectionOpeningTestsBase
    {
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
