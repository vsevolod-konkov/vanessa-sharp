using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Utility;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Соединение с базой 1С.</summary>
    public sealed partial class OneSConnection : DbConnection, IGlobalContextProvider, ITransactionManager
    {
        #region Внутренние поля

        /// <summary>Состояние соединения.</summary>
        private StateObject _state;

        /// <summary>
        /// Рекомендуемые параметры создания коннектора к информационной базе 1С.
        /// </summary>
        private readonly ConnectorCreationParams _connectorCreationParams;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_state != null);
        }

        #endregion

        #region Внутренние интерфейсы

        /// <summary>Конструктор принимающий состояние соединения.</summary>
        /// <remarks>Для модульных тестов.</remarks>
        /// <param name="state">Состояние соединения</param>
        /// <param name="connectorCreationParams">Рекомендуемые параметры создания коннектора к информационной базе 1С.</param>
        internal OneSConnection(StateObject state, ConnectorCreationParams connectorCreationParams)
        {
            Contract.Requires<ArgumentNullException>(state != null);

            _state = state;
            _connectorCreationParams = connectorCreationParams;
        }

        /// <summary>Глобальный контекст.</summary>
        IGlobalContext IGlobalContextProvider.GlobalContext
        {
            get { return _state.GlobalContext; }
        }

        /// <summary>Принятие транзакции.</summary>
        void ITransactionManager.CommitTransaction()
        {
            ChangeState(_state.CommitTransaction());
        }

        /// <summary>Отмена транзакции.</summary>
        void ITransactionManager.RollbackTransaction()
        {
            ChangeState(_state.RollbackTransaction());
        }

        #endregion

        #region Интерфейс использования
        
        /// <summary>Конструктор принимающий фабрику подключений.</summary>
        /// <param name="connectorFactory">Фабрика подключений.</param>
        /// <param name="connectorCreationParams">Рекомендуемые параметры создания коннектора к информационной базе 1С.</param>
        public OneSConnection(
            IOneSConnectorFactory connectorFactory, ConnectorCreationParams connectorCreationParams)
            : this(StateObject.CreateDefault(connectorFactory), connectorCreationParams)
        {}

        /// <summary>Конструктор принимающий только параметры создания коннектора к 1С.</summary>
        /// <param name="connectorCreationParams">Рекомендуемые параметры создания коннектора к информационной базе 1С.</param>
        public OneSConnection(ConnectorCreationParams connectorCreationParams)
            : this((IOneSConnectorFactory)null, connectorCreationParams)
        { }

        /// <summary>Конструктор без аргументов.</summary>
        public OneSConnection()
            : this((ConnectorCreationParams)null)
        {}

        /// <summary>Конструктор.</summary>
        /// <param name="connectionString">
        /// Строка соединения с информационной базой 1С.
        /// Используется для установки свойства <see cref="ConnectionString"/>.
        /// </param>
        public OneSConnection(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        /// <summary>Начало транзакции.</summary>
        /// <param name="isolationLevel">
        /// Уровень изоляции.
        /// Поддерживается только уровень <see cref="IsolationLevel.Unspecified"/>.
        /// В остальных случаях выбрасывается исключение <see cref="NotSupportedException"/>.
        /// </param>
        /// <returns>
        /// Возвращает объект транзакции.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Выбрасывается если передается <paramref name="isolationLevel"/> отличный
        /// от <see cref="IsolationLevel.Unspecified"/>.
        /// </exception>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            if (isolationLevel != IsolationLevel.Unspecified)
            {
                throw new ArgumentException(
                    string.Format(
                        "Неподдерживается уровень изоляции транзакции \"{0}\".", 
                        isolationLevel));
            }

            ChangeState(_state.BeginTransaction(this));
            return _state.CurrentTransaction;
        }

        /// <summary>Начало транзакции.</summary>
        /// <returns>
        /// Возвращает объект транзакции.
        /// </returns>
        public new OneSTransaction BeginTransaction()
        {
            return (OneSTransaction)base.BeginTransaction();
        }

        /// <summary>Объект текущей транзакции.</summary>
        public OneSTransaction CurrentTransaction
        {
            get { return _state.CurrentTransaction; }
        }

        /// <summary>Изменение информационной базы 1С.</summary>
        /// <remarks>
        /// Не поддерживается. 
        /// Всегда выдается исключение <see cref="NotSupportedException"/>.
        /// </remarks>
        /// <param name="databaseName">Имя базы к которой надо сделать переподключение.</param>
        /// <exception cref="NotSupportedException">Выбрасывается при любом вызове.</exception>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException("Метод ChangeDatabase не поддерживается.");
        }

        /// <summary>Закрытие соединение с информационной базой 1С.</summary>
        public override void Close()
        {
            var newState = _state.CloseConnection();
            ChangeState(newState);
        }

        /// <summary>Освобождение ресурсов.</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Close();

            base.Dispose(disposing);
        }

        /// <summary>Строка соединения с информационной базой 1С.</summary>
        public override string ConnectionString
        {
            get { return _state.ConnectionString; }
            set { _state.ConnectionString = value; }
        }

        /// <summary>Создание команды для выполнения запроса к информационной базе 1С.</summary>
        /// <returns>Созданная команда.</returns>
        public new OneSCommand CreateCommand()
        {
            return new OneSCommand(this);
        }

        /// <summary>Создание команды для выполнения запроса к информационной базе 1С.</summary>
        /// <returns>Созданная команда.</returns>
        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        /// <summary>Каталог в котором находится информационная база 1С.</summary>
        public override string DataSource
        {
            get { return GetCatalogFromConnectionString(); }
        }

        /// <summary>Каталог в котором находится информационная база 1С.</summary>
        public override string Database
        {
            get { return GetCatalogFromConnectionString(); }
        }

        /// <summary>Открытие соединения с информационной базой 1С.</summary>
        /// <remarks>
        /// Если строка соединения задана неккоректно или произошла ошибка при соединении,
        /// то выбрасывается исключение <see cref="InvalidOperationException"/>.
        /// </remarks>
        public override void Open()
        {
            ChangeState(_state.OpenConnection(_connectorCreationParams));
        }

        /// <summary>Версия сервера.</summary>
        /// <remarks>
        /// Запрос свойства возможен, только если состояние соединения не равно
        /// <see cref="ConnectionState.Closed"/> или <see cref="ConnectionState.Broken"/>.
        /// </remarks>
        public override string ServerVersion
        {
            get { return _state.Version; }
        }

        /// <summary>Состояние соединения.</summary>
        public override ConnectionState State
        {
            get { return _state.ConnectionState; }
        }

        /// <summary>Время ожидания соединения.</summary>
        public override int ConnectionTimeout
        {
            get { return PoolTimeout; }
        }

        /// <summary>Инициализатор соединения.</summary>
        public object Initializer
        {
            get { return _state.Initializer; }
            set { _state.Initializer = value; }
        }

        /// <summary>Время ожидания соединения.</summary>
        public int PoolTimeout
        {
            get { return _state.PoolTimeout; }
            set { _state.PoolTimeout = value; }
        }

        /// <summary>Мощность пула соединений.</summary>
        public int PoolCapacity
        {
            get { return _state.PoolCapacity; }
            set { _state.PoolCapacity = value; }
        }

        /// <summary>Признак монопольного доступа к информационной базе 1С.</summary>
        public bool IsExclusiveMode
        {
            get { return _state.IsExclusiveMode; }
            set { _state.IsExclusiveMode = value; }
        }

        /// <summary>Строковое представление.</summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(ConnectionString)
                ? "Несвязанное соединение к 1С"
                : string.Format("Соединение к 1С: {0}", ConnectionString);
        }

        #endregion

        #region Внутренние методы

        private void ChangeState(StateObject newState)
        {
            Contract.Requires<ArgumentNullException>(newState != null);

            if (newState == _state)
                return;

            var oldState = _state;
            _state = newState;
            try
            {
                if (oldState.ConnectionState != newState.ConnectionState)
                {
                    var args = new StateChangeEventArgs(oldState.ConnectionState, newState.ConnectionState);
                    OnStateChange(args);
                }
            }
            finally
            {
                oldState.Dispose();
            }
        }

        /// <summary>Получение каталога из строки соединения с информационной базой 1С.</summary>
        /// <returns>Каталог.</returns>
        private string GetCatalogFromConnectionString()
        {
            return ConnectionStringHelper
                .GetCatalogFromConnectionString(ConnectionString);
        }

        #endregion

        #region Внутренние типы

        /// <summary>Параметры соединения с информационной базой 1С.</summary>
        internal sealed class ConnectionParameters
        {
            /// <summary>Фабрика подключений к 1С.</summary>
            public IOneSConnectorFactory ConnectorFactory { get; set; }
            
            /// <summary>Строка соединения.</summary>
            public string ConnectionString { get; set; }

            /// <summary>Инициализатор соединения.</summary>
            public object Initializer { get; set; }

            /// <summary>Время ожидания подключения.</summary>
            public int PoolTimeout { get; set; }

            /// <summary>Мощность подключения.</summary>
            public int PoolCapacity { get; set; }
        }

        #endregion
    }
}
