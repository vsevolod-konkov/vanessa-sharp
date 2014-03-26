using System.Data;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тесты на <see cref="OneSDataContext"/>.
    /// </summary>
    #if REAL_MODE 
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public sealed class OneSDataContextTests : ConnectedTestsBase
    {
        public OneSDataContextTests(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {}

        /// <summary>
        /// Тест на инициализацию и освобождение ресурсов.
        /// </summary>
        [Test]
        public void TestCreateAndDispose()
        {
            var testedInstance = new OneSDataContext(Connection);
            Assert.AreSame(Connection, testedInstance.Connection);

            testedInstance.Dispose();
            Assert.AreSame(Connection, testedInstance.Connection);
            Assert.AreEqual(ConnectionState.Closed, Connection.State);
        }
    }
}
