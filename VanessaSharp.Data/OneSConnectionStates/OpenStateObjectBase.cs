using System;
using System.Data;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Базовый класс открытого состояния.</summary>
        internal abstract class OpenStateObjectBase : StateObject
        {
            /// <summary>
            /// Используется ли экземпляр <see cref="IGlobalContext"/>
            /// между несколькими объектами.
            /// </summary>
            private bool _isSharedGlobalContext;

            /// <summary>Конструктор.</summary>
            /// <param name="parameters">Параметры подключения.</param>
            /// <param name="globalContext">Глобальный контекст 1C.</param>
            /// <param name="version">Версия 1С.</param>
            protected OpenStateObjectBase(
                ConnectionParameters parameters,
                IGlobalContext globalContext, 
                string version)
                : base(SafeGetConnectorFactory(parameters))
            {
                Contract.Requires<ArgumentNullException>(parameters != null);
                Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(parameters.ConnectionString));
                Contract.Requires<ArgumentNullException>(globalContext != null);

                _globalContext = globalContext;
                _connectionString = parameters.ConnectionString;
                _poolTimeout = parameters.PoolTimeout;
                _poolCapacity = parameters.PoolCapacity;
                _version = version;
            }

            /// <summary>Безопасное получение коннектора.</summary>
            /// <param name="parameters">Параметры.</param>
            private static IOneSConnectorFactory SafeGetConnectorFactory(ConnectionParameters parameters)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                return parameters.ConnectorFactory;
            }

            /// <summary>Объект глобального контекста 1С.</summary>
            public override IGlobalContext GlobalContext
            {
                get { return _globalContext; }
            }
            private readonly IGlobalContext _globalContext;

            /// <summary>Открытие соединение.</summary>
            /// <returns>Объект состояния открытого соединения.</returns>
            public sealed override StateObject OpenConnection(ConnectorCreationParams creationParams)
            {
                throw new InvalidOperationException("Соединение уже открыто.");
            }

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public override StateObject CloseConnection()
            {
                return new ClosedStateObject(ConnectorFactory)
                {
                    ConnectionString = ConnectionString,
                    PoolTimeout = PoolTimeout,
                    PoolCapacity = PoolCapacity
                };
            }

            /// <summary>Состояние соединения.</summary>
            public sealed override ConnectionState ConnectionState
            {
                get { return ConnectionState.Open; }
            }

            /// <summary>Строка подключения к 1С.</summary>
            public override string ConnectionString
            {
                get { return _connectionString; }
                set
                {
                    throw new InvalidOperationException("В состоянии Open соединения нельзя изменить свойство ConnectionString.");
                }
            }
            private readonly string _connectionString;

            /// <summary>Собственно освобождения ресурсов.</summary>
            protected sealed override void InternalDisposed()
            {
                if (!_isSharedGlobalContext)
                    _globalContext.Dispose();
            }

            /// <summary>Время ожидания соединения.</summary>
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

            /// <summary>Мощность пула соединения.</summary>
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

            /// <summary>Признак режима монопольного доступа.</summary>
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

            /// <summary>
            /// Маркировка того, что глобальный контекст будет использован и другими объектами.
            /// </summary>
            protected void UseGlobalContext()
            {
                _isSharedGlobalContext = true;
            }

            /// <summary>Версия 1С.</summary>
            public override string Version
            {
                get { return _version; }
            }
            private readonly string _version;
        }
    }
}