using NUnit.Framework;
using VanessaSharp.Data.AcceptanceTests.Mocks;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>Базовый класс тестов требующие подключения.</summary>
    public abstract class ConnectedTestsBase : TestsBase
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

        /// <summary>Следует ли открывать соединение.</summary>
        private readonly bool _shouldBeOpen;

        /// <summary>Конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        protected ConnectedTestsBase(TestMode testMode, bool shouldBeOpen)
        {
            _testMode = testMode;
            _shouldBeOpen = shouldBeOpen;
        }

        /// <summary>Конструктор.</summary>
        protected ConnectedTestsBase(TestMode testMode)
            : this(testMode, true)
        { }

        /// <summary>Тестовое соединение.</summary>
        protected OneSConnection Connection
        {
            get { return _connection; }
        }
        private OneSConnection _connection;

        /// <summary>Установка тестового окружения.</summary>
        [SetUp]
        public void SetUp()
        {
            _connection = (_testMode == TestMode.Real)
                ? new OneSConnection(TestConnectionString)
                : new OneSConnection(CreateOneSConnectorFactoryForIsolatedMode()) { ConnectionString = TestConnectionString };
            
            if (_shouldBeOpen)
                _connection.Open();
        }

        /// <summary>Очистка тестового окружения.</summary>
        [TearDown]
        public void TearDown()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        private IOneSConnectorFactory CreateOneSConnectorFactoryForIsolatedMode()
        {
            var result = new OneSConnectorFactoryMock();
            result.NewOneSObjectAsking += (sender, args) => OnNewOneSObjectAsking(args);

            return result;
        }

        /// <summary>
        /// Обработчик запроса на создание экземпляра объекта 1С.
        /// </summary>
        internal virtual void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {}
    }
}
