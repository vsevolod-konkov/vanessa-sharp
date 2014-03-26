using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Приемочные тесты на <see cref="OneSDataReader"/> в реальном режиме.</summary>
    #if REAL_MODE
    [TestFixture(Description = "Тестирование для реального режима.")]
    #endif
    public sealed class TestsInRealMode : TestsBase
    {
        public TestsInRealMode() : base(TestMode.Real)
        {}
    }
}
