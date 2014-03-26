using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тестирование получения данных в изолированном режиме.
    /// </summary>
    //#if ISOLATED_MODE
    //[TestFixture(false)]
    //[TestFixture(true)]
    //#endif
    public sealed class IsolatedModeGettingDataRecordsTests : GettingDataRecordsTestsBase
    {
        public IsolatedModeGettingDataRecordsTests(bool shouldBeOpen) 
            : base(TestMode.Isolated, shouldBeOpen)
        {}
    }
}
