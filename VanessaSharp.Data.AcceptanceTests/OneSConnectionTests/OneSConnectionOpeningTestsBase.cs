using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра <see cref="OneSConnection"/>
    /// с возможностью открытия соединения с тестовой БД 1С.
    /// </summary>
    public class OneSConnectionOpeningTestsBase : OneSConnectionTestsBase
    {
        /// <summary>
        /// Флаг указывающий, 
        /// что тесты будут выполняться изоляционном режиме.
        /// </summary>
        /// <remarks>
        /// Изоляционный режим теста это режим при котором подключение
        /// к реальной БД 1С не делается, а ведется работа с моковой реализацией контрактов с 1С
        /// в памяти.
        /// </remarks>
        private readonly TestMode _testMode;

        /// <summary>Конструктор с передачей режима.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        protected OneSConnectionOpeningTestsBase(TestMode testMode)
        {
            _testMode = testMode;
        }

        /// <summary>Инициализация тестового окружения.</summary>
        [SetUp]
        public void SetUp()
        {
            TestedInstance = _testMode == TestMode.Real
                ? new OneSConnection(TestConnectionString)
                : new OneSConnection(new OneSConnectorFactoryMock()) { ConnectionString = TestConnectionString };
        }

        /// <summary>Тестируемый экземпляр.</summary>
        protected new OneSConnection TestedInstance
        {
            get { return base.TestedInstance; }

            private set { base.TestedInstance = value; }
        }
    }
}
