using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тестирование получения данных в реальном режиме.
    /// </summary>
    #if REAL_MODE
    [TestFixture(false)]
    [TestFixture(true)]
    #endif
    public sealed class RealModeGettingDataRecordsTests : GettingDataRecordsTestsBase
    {
        public RealModeGettingDataRecordsTests(bool shouldBeOpen) 
            : base(TestMode.Real, shouldBeOpen)
        {
        }
    }
}
