using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра <see cref="OneSConnection"/>
    /// с возможностью открытия соединения с тестовой БД 1С.
    /// </summary>
    public class OneSConnectionTestsBase : ConnectedTestsBase
    {
        /// <summary>Конструктор с передачей режима.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        protected OneSConnectionTestsBase(TestMode testMode)
            : base(testMode, false)
        {}

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSConnection TestedInstance
        {
            get { return base.Connection; }
        }
    }
}
