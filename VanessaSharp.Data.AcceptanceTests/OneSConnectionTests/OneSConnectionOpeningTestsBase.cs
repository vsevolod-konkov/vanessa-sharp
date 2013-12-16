namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра <see cref="OneSConnection"/>
    /// с возможностью открытия соединения с тестовой БД 1С.
    /// </summary>
    public class OneSConnectionOpeningTestsBase : ConnectedTestsBase
    {
        /// <summary>Конструктор с передачей режима.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        protected OneSConnectionOpeningTestsBase(TestMode testMode)
            : base(testMode, false)
        {}

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSConnection TestedInstance
        {
            get { return base.Connection; }
        }

        /// <summary>Закрытие свойства.</summary>
        private new OneSConnection Connection
        {
            get { return base.Connection; }
        }
    }
}
