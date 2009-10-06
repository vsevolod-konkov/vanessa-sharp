using System;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Базовый класс тестов требующие подключения.</summary>
    public abstract class ConnectedTestsBase : TestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        protected ConnectedTestsBase(bool shouldBeOpen)
        {
            _shouldBeOpen = shouldBeOpen;
        }

        /// <summary>Конструктор.</summary>
        protected ConnectedTestsBase() : this(true)
        { }

        /// <summary>Следует ли открывать соединение.</summary>
        private readonly bool _shouldBeOpen;
        
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
            _connection = new OneSConnection(TestConnectionString);
            if (_shouldBeOpen)
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
        {}

        /// <summary>Очистка окружения тестов.</summary>
        /// <remarks>Точка расширения для наследных классов.</remarks>
        protected virtual void InternalTearDown()
        {}
    }
}
