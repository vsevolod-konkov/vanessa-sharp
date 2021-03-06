﻿using System;
using System.Data;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Базовый класс объекта состояния.</summary>
        [ContractClass(typeof(StateObjectContract))]
        internal abstract class StateObject : IDisposable
        {
            /// <summary>Конструктор принимающий фабрику подключений.</summary>
            /// <param name="connectorFactory">Фабрика подключений.</param>
            protected StateObject(IOneSConnectorFactory connectorFactory)
            {
                _connectorFactory = connectorFactory;
            }

            /// <summary>Фабрика подключений.</summary>
            protected IOneSConnectorFactory ConnectorFactory
            {
                get { return _connectorFactory; }
            }
            private readonly IOneSConnectorFactory _connectorFactory;
            
            /// <summary>Объект глобального контекста 1С.</summary>
            public virtual IGlobalContext GlobalContext
            {
                get
                {
                    Contract.Ensures(Contract.Result<IGlobalContext>() != null);
                    
                    throw new InvalidOperationException(
                        "Нельзя получить глобальный контекст при закрытом соединении.");
                }
            }

            /// <summary>Открытие соединение.</summary>
            /// <returns>Объект состояния открытого соединения.</returns>
            public abstract StateObject OpenConnection(ConnectorCreationParams creationParams);

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public abstract StateObject CloseConnection();

            /// <summary>Состояние соединения.</summary>
            public abstract ConnectionState ConnectionState { get; }

            /// <summary>Строка подключения к 1С.</summary>
            public abstract string ConnectionString { get; set; }

            /// <summary>Инициализатор подключения.</summary>
            /// <remarks>Объект с одноименными свойствами для инициализации коннектора 1С.</remarks>
            public object Initializer { get; set; }

            /// <summary>Время ожидания соединения.</summary>
            public abstract int PoolTimeout { get; set; }

            /// <summary>Мощность пула соединения.</summary>
            public abstract int PoolCapacity { get; set; }

            /// <summary>Признак режима монопольного доступа.</summary>
            public abstract bool IsExclusiveMode { get; set; }

            /// <summary>Начало транзакции.</summary>
            public abstract StateObject BeginTransaction(OneSConnection connection);

            /// <summary>Принятие транзакции.</summary>
            public virtual StateObject CommitTransaction()
            {
                Contract.Ensures(Contract.Result<StateObject>() != null);

                throw new InvalidOperationException(
                    "Зафиксировать транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Отмена транзакции.</summary>
            public virtual StateObject RollbackTransaction()
            {
                Contract.Ensures(Contract.Result<StateObject>() != null);
                
                throw new InvalidOperationException(
                    "Отменить транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Текущая транзакция.</summary>
            public virtual OneSTransaction CurrentTransaction
            {
                get { return null; }
            }

            /// <summary>Создание объекта состояния по умолчанию.</summary>
            public static StateObject CreateDefault(IOneSConnectorFactory connectorFactory)
            {
                return new ClosedStateObject(connectorFactory);
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

            /// <summary>Получение параметров соединения.</summary>
            protected ConnectionParameters GetConnectionParameters()
            {
                return new ConnectionParameters
                    {
                        ConnectorFactory = ConnectorFactory,
                        ConnectionString = ConnectionString,
                        Initializer = Initializer,
                        PoolCapacity = PoolCapacity,
                        PoolTimeout = PoolTimeout
                    };
            }
        }

        /// <summary>Класс контракта <see cref="StateObject"/>.</summary>
        [ContractClassFor(typeof(StateObject))]
        private abstract class StateObjectContract : StateObject
        {
            protected StateObjectContract(IOneSConnectorFactory connectorFactory)
                : base(connectorFactory)
            { }

            public override StateObject OpenConnection(ConnectorCreationParams creationParams)
            {
                Contract.Ensures(Contract.Result<StateObject>() != null);

                return default(StateObject);
            }

            public override StateObject CloseConnection()
            {
                Contract.Ensures(Contract.Result<StateObject>() != null);

                return default(StateObject);
            }

            public override StateObject BeginTransaction(OneSConnection connection)
            {
                Contract.Requires<ArgumentNullException>(connection != null);
                Contract.Ensures(Contract.Result<StateObject>() != null);

                return default(StateObject);
            }
        }
    }
}