using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Базовый класс тестов.</summary>
    public class InternalTestBase
    {
        /// <summary>Настройки.</summary>
        internal Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        protected string ConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Settings.TestCatalog;
                builder.User = Settings.TestUser;

                return builder.ConnectionString;
            }
        }

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
            _connection = new OneSConnection(ConnectionString);
            _connection.Open();

            InternalSetUp();
        }

        /// <summary>Очистка тестового окружения.</summary>
        [TearDown]
        public void TearDown()
        {
            InternalTearDown();

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>Установка окружения тестов.</summary>
        /// <remarks>Точка расширения для наследных классов.</remarks>
        protected virtual void InternalSetUp()
        { }

        /// <summary>Очистка окружения тестов.</summary>
        /// <remarks>Точка расширения для наследных классов.</remarks>
        protected virtual void InternalTearDown()
        { }
    }
}
