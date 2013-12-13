using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тесты на случаи когда экземпляр 
    /// <see cref="OneSConnection"/> имеет 
    /// состояние <see cref="OneSConnection.State"/>
    /// в значении <see cref="ConnectionState.Closed"/>
    /// работающие только для реального тестового режима.
    /// </summary>
    #if REAL_MODE
    [TestFixture(false)]
    [TestFixture(true)]
    #endif
    public sealed class OnlyRealModeClosedStateTests
        : ClosedStateTestsBase
    {
        /// <summary>Параметрический конструктор.</summary>
        /// <param name="hadOpened">Было ли открыто соединение.</param>
        public OnlyRealModeClosedStateTests(bool hadOpened)
            : base(TestMode.Real, hadOpened)
        {}

        /// <summary>
        /// Тестирование метода <see cref="OneSConnection.Open"/>
        /// в случае передачи невалидной строки соединения.
        /// </summary>
        [Test]
        public void TestOpenWhenInvalidConnectionString()
        {
            TestActionWhenInvalidConnectionString(TestedInstance.Open);
        }
    }
}
