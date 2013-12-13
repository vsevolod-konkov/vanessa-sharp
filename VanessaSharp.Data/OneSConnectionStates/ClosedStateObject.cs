using System;
using System.Data;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние закрытого соединения.</summary>
        private sealed class ClosedStateObject : StateObject
        {
            public ClosedStateObject(IOneSConnectorFactory connectorFactory)
                    : base(connectorFactory)
            {}
            
            public override StateObject OpenConnection()
            {
                if (string.IsNullOrEmpty(ConnectionString))
                    throw new InvalidOperationException("Строка соединения не задана.");

                return OpenStateObject.Create(
                    new ConnectionParameters
                        {
                            ConnectorFactory = ConnectorFactory,
                            ConnectionString = ConnectionString,
                            PoolCapacity = PoolCapacity,
                            PoolTimeout = PoolTimeout
                        });
            }

            public override StateObject CloseConnection()
            {
                return this;
            }

            public override ConnectionState ConnectionState
            {
                get { return ConnectionState.Closed; }
            }

            public override string ConnectionString
            {
                get;
                set;
            }

            public override int PoolTimeout
            {
                get;
                set;
            }

            public override int PoolCapacity
            {
                get;
                set;
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
    }
}