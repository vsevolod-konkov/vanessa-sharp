using System.Data;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>Тестирование <see cref="OneSTransaction"/>.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated)]
    #endif
    public sealed class OneSTransactionTests : ConnectedTestsBase
    {
        public OneSTransactionTests(TestMode testMode) : base(testMode)
        {}
        
        /// <summary>Тестирование свойств экземпляра <see cref="OneSTransaction"/>.</summary>
        [Test]
        public void TestTransactionProperties()
        {
            var testedInstance = Connection.BeginTransaction();

            Assert.IsNotNull(testedInstance);
            Assert.AreEqual(IsolationLevel.Unspecified, testedInstance.IsolationLevel);
            Assert.IsNotNull(Connection.CurrentTransaction);
            Assert.AreSame(testedInstance, Connection.CurrentTransaction);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSConnection.CurrentTransaction"/>
        /// в случае внезапного закрытия соединения.
        /// </summary>
        [Test]
        public void TestCurrentTransactionWhenCloseConnection()
        {
            Connection.BeginTransaction();
            Assert.IsNotNull(Connection.CurrentTransaction);

            Connection.Close();
            Assert.IsNull(Connection.CurrentTransaction);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSTransaction.Rollback"/>.
        /// </summary>
        [Test]
        public void TestRollback()
        {
            var testedInstance = Connection.BeginTransaction();

            testedInstance.Rollback();
            Assert.IsNull(Connection.CurrentTransaction);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSTransaction.Commit"/>.
        /// </summary>
        [Test]
        public void TestCommit()
        {
            var testedInstance = Connection.BeginTransaction();

            testedInstance.Commit();
            Assert.IsNull(Connection.CurrentTransaction);
        }
    }
}
