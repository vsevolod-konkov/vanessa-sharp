using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace VsevolodKonkov.OneSSharp.Data
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
            throw new System.NotImplementedException();
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
        protected override DbCommand CreateDbCommand()
        {
            throw new System.NotImplementedException();
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
            get { return "8"; }
        }

        /// <summary>Состояние соединения.</summary>
        public override ConnectionState State
        {
            get { return _state.ConnectionState; }
        }

        /// <summary>Время ожидания соединения.</summary>
        public override int ConnectionTimeout
        {
            get { return PoolTimeout ?? 0; }
        }

        /// <summary>Время ожидания соединения.</summary>
        public int? PoolTimeout
        {
            get { return _state.PoolTimeout; }
            set { _state.PoolTimeout = value; }
        }

        /// <summary>Мощность пула соединений.</summary>
        public int? PoolCapacity
        {
            get { return _state.PoolCapacity; }
            set { _state.PoolCapacity = value; }
        }

        /// <summary>Получение параметров соединения.</summary>
        private Proxies.ConnectionParameters GetParameters()
        {
            var result = new Proxies.ConnectionParameters();
            result.ConnectionString = ConnectionString;
            result.PoolTimeout = PoolTimeout;
            result.PoolCapacity = PoolCapacity;

            return result;
        }

        private void ChangeState(StateObject newState)
        {
            ChecksHelper.CheckArgumentNotNull(newState, "newState");

            if (!newState.Equals(_state))
            {
                var oldState = _state;
                _state = newState;
                var args = new StateChangeEventArgs(oldState.ConnectionState, newState.ConnectionState);
                oldState.Dispose();

                OnStateChange(args);
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

        #endregion

        #region Внутренние типы

        /// <summary>Базовый класс объекта состояния.</summary>
        private abstract class StateObject : IDisposable
        {
            /// <summary>Открытие соединение.</summary>
            /// <param name="parameters">Параметры соединения.</param>
            /// <returns>Объект состояния открытого соединения.</returns>
            public abstract StateObject OpenConnection(Proxies.ConnectionParameters parameters);

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public abstract StateObject CloseConnection(Proxies.ConnectionParameters parameters);

            /// <summary>Состояние соединения.</summary>
            public abstract ConnectionState ConnectionState { get; }

            /// <summary>проверка возможности изменения строки соединения.</summary>
            public abstract void CheckCanChangeConnectionString();

            /// <summary>Время ожидания соединения.</summary>
            public abstract int? PoolTimeout { get; set; }

            /// <summary>Мощность пула соединения.</summary>
            public abstract int? PoolCapacity { get; set; }

            public bool Equals(StateObject other)
            {
                if (other == null)
                    return false;

                return GetType() == other.GetType();
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
        }

        /// <summary>Состояние закрытого соединения.</summary>
        private sealed class ClosedStateObject : StateObject
        {
            public override StateObject OpenConnection(Proxies.ConnectionParameters parameters)
            {
                return OpenStateObject.Create(parameters);
            }

            public override StateObject CloseConnection(Proxies.ConnectionParameters parameters)
            {
                return this;
            }

            public override ConnectionState ConnectionState
            {
                get { return ConnectionState.Closed; }
            }

            public override void CheckCanChangeConnectionString()
            {}

            public override int? PoolTimeout
            {
                get; set;
            }

            public override int? PoolCapacity
            {
                get; set;
            }
        }

        /// <summary>Состояние открытого соединения.</summary>
        private sealed class OpenStateObject : StateObject
        {
            private readonly Proxies.GlobalContext _global;

            private OpenStateObject(Proxies.GlobalContext globalCtx)
            {
                _global = globalCtx;
            }

            public static StateObject Create(Proxies.ConnectionParameters parameters)
            {
                var globalCtx = Proxies.GlobalContext.Connect(parameters);
                try
                {
                    return new OpenStateObject(globalCtx);
                }
                catch
                {
                    globalCtx.Dispose();
                    throw;
                }
            }

            public override StateObject OpenConnection(Proxies.ConnectionParameters parameters)
            {
                throw new InvalidOperationException("Соединение уже открыто.");
            }

            public override StateObject CloseConnection(Proxies.ConnectionParameters parameters)
            {
                var result = new ClosedStateObject();
                result.PoolTimeout = parameters.PoolTimeout;
                result.PoolCapacity = parameters.PoolCapacity;

                return result;
            }

            public override ConnectionState ConnectionState
            {
                get { return ConnectionState.Open; }
            }

            public override void CheckCanChangeConnectionString()
            {
                throw new InvalidOperationException("Нельзя менять строку соединения, когда оно открыто.");
            }

            protected override void InternalDisposed()
            {
                _global.Dispose();
            }

            public override int? PoolTimeout
            {
                get
                {
                    return _global.PoolTimeout;
                }
                set
                {
                    if (value.HasValue)
                        _global.PoolTimeout = value.Value;
                }
            }

            public override int? PoolCapacity
            {
                get 
                {
                    return _global.PoolCapacity;
                }

                set
                {
                    if (value.HasValue)
                        _global.PoolCapacity = value.Value;
                }
            }
        }

        #endregion
    }
}
