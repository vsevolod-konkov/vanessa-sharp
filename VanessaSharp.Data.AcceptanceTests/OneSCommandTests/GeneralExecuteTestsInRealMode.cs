using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тесты проверяющие правильность поведения выполнения запросов команды в реальном режиме.</summary>
    #if REAL_MODE
    [TestFixture(Description = "Тестирование метода Execute для реального режима.")]
    #endif
    public sealed class GeneralExecuteTestsInRealMode : GeneralExecuteTestsBase
    {
        public GeneralExecuteTestsInRealMode() : base(TestMode.Real)
        {}
    }
}
