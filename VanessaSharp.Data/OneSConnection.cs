using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Соединение с базой 1С.</summary>
    public sealed class OneSConnection : DbConnection
    {
        #region Внутренние поля

        /// <summary>Строка соединения.</summary>
        private string _connectionString;

        /// <summary>Состояние соединения.</summary>
        private StateObject _state;

        #endregion

        #region Интерфейс использования

        /// <summary>Конструктор.</summary>
        public OneSConnection()
        {
            _state = StateObject.CreateDefault();
        }

        /// <summary>Конструктор.</summary>
        /// <param name="connectionString">
        /// Строка соединения с информационной базой 1С.
        /// Используется для устанавливки свойства <see cref="ConnectionString"/>.
        /// </param>
        public OneSConnection(string connectionString)
            : this()
        {
            ConnectionString = connectionString;
        }

        /// <summary>Глобальный контекст.</summary>
        internal IGlobalContext GlobalContext
        {
            get { return _state.GlobalContext; }
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

        /// <summary>Принятие транзакции.</summary>
        internal void CommitTransaction()
        {
            ChangeState(_state.CommitTransaction());
        }

        /// <summary>Отмена транзакции.</summary>
        internal void RollbackTransaction()
        {
            ChangeState(_state.RollbackTransaction());
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
            ChangeState(_state.CloseConnection(GetParameters()));
        }

        /// <summary>Строка соединения с информационной базой 1С.</summary>
        public override string ConnectionString
        {
            get 
            { 
                return _connectionString; 
            }
            
            set
            {
                _state.CheckCanChangeConnectionString();
                _connectionString = value;
            }
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
            if (string.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("Строка соединения не задана.");

            ChangeState(_state.OpenConnection(GetParameters()));
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

        /// <summary>Получение параметров соединения.</summary>
        private ConnectionParameters GetParameters()
        {
            return new ConnectionParameters
            {
                ConnectionString = ConnectionString,
                PoolTimeout = PoolTimeout,
                PoolCapacity = PoolCapacity
            };
        }

        private void ChangeState(StateObject newState)
        {
            ChecksHelper.CheckArgumentNotNull(newState, "newState");

            if (!newState.Equals(_state))
            {
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
        }

        /// <summary>Получение каталога из строки соединения с информационной базой 1С.</summary>
        /// <returns>Каталог.</returns>
        private string GetCatalogFromConnectionString()
        {
            var builder = new OneSConnectionStringBuilder();
            try
            {
                builder.ConnectionString = ConnectionString;
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Строка соединения \"{0}\" не является валидной.",
                    ConnectionString), e);
            }

            return builder.Catalog;
        }

        /// <summary>Строковое представление.</summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(ConnectionString)
                ? "Несвязанное соединение к 1С"
                : string.Format("Соединение к 1С: {0}", ConnectionString);
        }

        /// <summary>Освобождение ресурсов.</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Close();
            
            base.Dispose(disposing);
        }

        #endregion

        #region Внутренние типы

        /// <summary>Параметры соединения с информационной базой 1С.</summary>
        private sealed class ConnectionParameters
        {
            /// <summary>Строка соединения.</summary>
            public string ConnectionString { get; set; }

            /// <summary>Время ожидания подключения.</summary>
            public int PoolTimeout { get; set; }

            /// <summary>Мощность подключения.</summary>
            public int PoolCapacity { get; set; }
        }

        /// <summary>Базовый класс объекта состояния.</summary>
        private abstract class StateObject : IDisposable
        {
            /// <summary>Объект глобального контекста 1С.</summary>
            public virtual IGlobalContext GlobalContext
            {
                get
                {
                    throw new InvalidOperationException(
                        "Нельзя получить глобальный контекст при закрытом соединении.");
                }
            }
            
            /// <summary>Открытие соединение.</summary>
            /// <param name="parameters">Параметры соединения.</param>
            /// <returns>Объект состояния открытого соединения.</returns>
            public abstract StateObject OpenConnection(ConnectionParameters parameters);

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public abstract StateObject CloseConnection(ConnectionParameters parameters);

            /// <summary>Состояние соединения.</summary>
            public abstract ConnectionState ConnectionState { get; }

            /// <summary>проверка возможности изменения строки соединения.</summary>
            public abstract void CheckCanChangeConnectionString();

            /// <summary>Время ожидания соединения.</summary>
            public abstract int PoolTimeout { get; set; }

            /// <summary>Мощность пула соединения.</summary>
            public abstract int PoolCapacity { get; set; }

            /// <summary>Признак режима монопольного доступа.</summary>
            public abstract bool IsExclusiveMode { get; set; }

            public bool Equals(StateObject other)
            {
                if (other == null)
                    return false;

                return GetType() == other.GetType();
            }

            /// <summary>Начало транзакции.</summary>
            public abstract StateObject BeginTransaction(OneSConnection connection);

            /// <summary>Принятие транзакции.</summary>
            public virtual StateObject CommitTransaction()
            {
                throw new InvalidOperationException(
                    "Зафиксировать транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Отмена транзакции.</summary>
            public virtual StateObject RollbackTransaction()
            {
                throw new InvalidOperationException(
                    "Отменить транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Текущая транзакция.</summary>
            public virtual OneSTransaction CurrentTransaction
            {
                get { return null; }
            }

            /// <summary>Создание объекта состояния по умолчанию.</summary>
            public static StateObject CreateDefault()
            {
                return new ClosedStateObject();
            }

            /// <summary>Освобождение ресурсов, удерживаемых объектом-состоянием.</summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    InternalDisposed();
                    _disposed = true;
                }
            }

            /// <summary>Признак, того что ресурсы были освобождены.</summary>
            private bool _disposed;

            /// <summary>Собственно освобождения ресурсов.</summary>
            protected virtual void InternalDisposed()
            {}

            /// <summary>Версия 1С.</summary>
            public abstract string Version { get; }
        }

        /// <summary>Состояние закрытого соединения.</summary>
        private sealed class ClosedStateObject : StateObject
        {
            public override StateObject OpenConnection(ConnectionParameters parameters)
            {
                return OpenStateObject.Create(parameters);
            }

            public override StateObject CloseConnection(ConnectionParameters parameters)
            {
                return this;
            }

            public override ConnectionState ConnectionState
            {
                get { return ConnectionState.Closed; }
            }

            public override void CheckCanChangeConnectionString()
            {}

            public override int PoolTimeout
            {
                get; set;
            }

            public override int PoolCapacity
            {
                get; set;
            }

            public override bool IsExclusiveMode
            {
                get
                {
                    throw new InvalidOperationException(
                        "Свойство IsExclusiveMode недоступно при закрытом соединении.");
                }
                set
                {
                    throw new InvalidOperationException(
                        "Свойство IsExclusiveMode недоступно при закрытом соединении.");
                }
            }

            public override StateObject BeginTransaction(OneSConnection connection)
            {
                throw new InvalidOperationException(
                    "Нельзя начать транзакцию, если соединение не открыто."); 
            }

            public override string Version
            {
                get
                {
                    throw new InvalidOperationException(
                    "Нельзя получить версию сервера 1С в закрытом состоянии соединения."); 
                }
            }
        }

        /// <summary>Базовый класс открытого состояния.</summary>
        private abstract class OpenStateObjectBase : StateObject
        {
            private bool _sharedGlobalContext;

            protected OpenStateObjectBase(IGlobalContext globalContext, int poolTimeout, int poolCapacity, string version)
            {
                ChecksHelper.CheckArgumentNotNull(globalContext, "globalContext");
                ChecksHelper.CheckArgumentNotEmpty(version, "version");
                
                _globalContext = globalContext;
                _poolTimeout = poolTimeout;
                _poolCapacity = poolCapacity;
                _version = version;
            }

            public override IGlobalContext GlobalContext
            {
                get { return _globalContext; }
            }
            private readonly IGlobalContext _globalContext;

            public sealed override StateObject OpenConnection(ConnectionParameters parameters)
            {
                throw new InvalidOperationException("Соединение уже открыто.");
            }

            public override StateObject CloseConnection(ConnectionParameters parameters)
            {
                var result = new ClosedStateObject();
                result.PoolTimeout = parameters.PoolTimeout;
                result.PoolCapacity = parameters.PoolCapacity;

                return result;
            }

            public sealed override ConnectionState ConnectionState
            {
                get { return ConnectionState.Open; }
            }

            public sealed override void CheckCanChangeConnectionString()
            {
                throw new InvalidOperationException("Нельзя менять строку соединения, когда оно открыто.");
            }

            protected sealed override void InternalDisposed()
            {
                if (!_sharedGlobalContext)
                    _globalContext.Dispose();
            }

            public sealed override int PoolTimeout
            {
                get { return _poolTimeout; }
                set
                {
                    if (PoolTimeout != value)
                    {
                        throw new InvalidOperationException(
                            "В состоянии Open соединения нельзя изменить свойство PoolTimeout.");
                    }
                }
            }
            private readonly int _poolTimeout;

            public sealed override int PoolCapacity
            {
                get { return _poolCapacity; }

                set
                {
                    if (PoolCapacity != value)
                    {
                        throw new InvalidOperationException(
                            "В состоянии Open соединения нельзя изменить свойство PoolCapacity.");
                    }
                }
            }
            private readonly int _poolCapacity;

            public sealed override bool IsExclusiveMode
            {
                get
                {
                    return _globalContext.ExclusiveMode();
                }

                set
                {
                    _globalContext.SetExclusiveMode(value);
                }
            }

            protected void UseGlobalContext()
            {
                _sharedGlobalContext = true;
            }

            public override string Version
            {
                get { return _version; }
            }
            private readonly string _version;
        }

        /// <summary>Состояние открытого соединения.</summary>
        private sealed class OpenStateObject : OpenStateObjectBase
        {
            public OpenStateObject(IGlobalContext globalContext, int poolTimeout, int poolCapacity, string version)
                : base(globalContext, poolTimeout, poolCapacity, version)
            {}

            private static IGlobalContext Connect(ConnectionParameters parameters, out string version)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                using (var connector = OneSConnectorFactory.Create())
                {
                    connector.PoolTimeout = (uint)parameters.PoolTimeout;
                    connector.PoolCapacity = (uint)parameters.PoolCapacity;
                    version = connector.Version;

                    return connector.Connect(parameters.ConnectionString);
                }
            }

            public static StateObject Create(ConnectionParameters parameters)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                string version;
                var globalContext = Connect(parameters, out version);
                try
                {
                    return new OpenStateObject(globalContext, parameters.PoolTimeout, parameters.PoolCapacity, version);
                }
                catch
                {
                    globalContext.Dispose();
                    throw;
                }
            }

            public override StateObject BeginTransaction(OneSConnection connection)
            {
                ChecksHelper.CheckArgumentNotNull(connection, "connection");

                var result = TransactionStateObject.Create(GlobalContext, PoolTimeout, PoolCapacity, Version, connection);
                UseGlobalContext();
                return result;
            }
        }

        /// <summary>Состояние, когда соединение находится в транзакции.</summary>
        private sealed class TransactionStateObject : OpenStateObjectBase
        {
            /// <summary>Объект транзакции.</summary>
            private readonly OneSTransaction _transaction;

            private TransactionStateObject(IGlobalContext globalContext, int poolTimeout, int poolCapacity, string version, OneSTransaction transaction)
                : base(globalContext, poolTimeout, poolCapacity, version)
            {
                ChecksHelper.CheckArgumentNotNull(transaction, "transaction");
                
                _transaction = transaction;
            }

            /// <summary>Создание транзакицонного состояния.</summary>
            /// <param name="globalContext">Глобальный контекст 1С.</param>
            /// <param name="connection">Соединение.</param>
            public static StateObject Create(IGlobalContext globalContext, int poolTimeout, int poolCapacity, string version, OneSConnection connection)
            {
                ChecksHelper.CheckArgumentNotNull(globalContext, "globalContext");
                ChecksHelper.CheckArgumentNotEmpty(version, "version");
                ChecksHelper.CheckArgumentNotNull(connection, "connection");    
                
                globalContext.BeginTransaction();
                try
                {
                    return new TransactionStateObject(globalContext, poolTimeout, poolCapacity, version, new OneSTransaction(connection));
                }
                catch
                {
                    globalContext.RollbackTransaction();
                    throw;
                }
            }

            public override StateObject CloseConnection(ConnectionParameters parameters)
            {
                GlobalContext.RollbackTransaction();
                return base.CloseConnection(parameters);
            }

            public override StateObject BeginTransaction(OneSConnection connection)
            {
                throw new InvalidOperationException(
                    "Соединение уже находится в состоянии транзакции. 1С не поддерживает вложенные транзакции");
            }

            public override StateObject CommitTransaction()
            {
                GlobalContext.CommitTransaction();
                return CreateOpenState();
            }

            public override StateObject RollbackTransaction()
            {
                GlobalContext.RollbackTransaction();
                return CreateOpenState();
            }

            private StateObject CreateOpenState()
            {
                var result = new OpenStateObject(GlobalContext, PoolTimeout, PoolCapacity, Version);
                UseGlobalContext();
                return result;
            }

            public override OneSTransaction CurrentTransaction
            {
                get { return _transaction; }
            }
        }

        #endregion
    }
}
