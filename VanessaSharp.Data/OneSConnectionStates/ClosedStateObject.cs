using System;
using System.Data;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние закрытого соединения.</summary>
        internal sealed class ClosedStateObject : StateObject
        {
            /// <summary>Конструктор.</summary>
            /// <param name="connectorFactory">
            /// Фабрика подключений к 1С.
            /// </param>
            public ClosedStateObject(IOneSConnectorFactory connectorFactory)
                    : base(connectorFactory)
            {}

            /// <summary>Открытие соединение.</summary>
            /// <returns>Объект состояния открытого соединения.</returns>
            public override StateObject OpenConnection(ConnectorCreationParams creationParams)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                    throw new InvalidOperationException("Строка соединения не задана.");

                return OpenStateObject.Create(creationParams,
                    GetConnectionParameters());
            }

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public override StateObject CloseConnection()
            {
                return this;
            }

            /// <summary>Состояние соединения.</summary>
            public override ConnectionState ConnectionState
            {
                get { return ConnectionState.Closed; }
            }

            /// <summary>Строка подключения к 1С.</summary>
            public override string ConnectionString { get; set; }

            /// <summary>Время ожидания соединения.</summary>
            public override int PoolTimeout { get; set; }

            /// <summary>Мощность пула соединения.</summary>
            public override int PoolCapacity { get; set; }

            /// <summary>Признак режима монопольного доступа.</summary>
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

            /// <summary>Начало транзакции.</summary>
            public override StateObject BeginTransaction(OneSConnection connection)
            {
                throw new InvalidOperationException(
                    "Нельзя начать транзакцию, если соединение не открыто.");
            }

            /// <summary>Версия 1С.</summary>
            public override string Version
            {
                get
                {
                    throw new InvalidOperationException(
                    "Нельзя получить версию сервера 1С в закрытом состоянии соединения.");
                }
            }
        }
    }
}