using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние открытого соединения.</summary>
        private sealed class OpenStateObject : OpenStateObjectBase
        {
            public OpenStateObject(IOneSConnectorFactory connectorFactory, IGlobalContext globalContext, string connectionString, int poolTimeout, int poolCapacity, string version)
                : base(connectorFactory, globalContext, connectionString, poolTimeout, poolCapacity, version)
            { }

            private static IGlobalContext Connect(ConnectionParameters parameters, out string version)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                var connectorFactory = parameters.ConnectorFactory ?? OneSConnectorFactory.Default;

                using (var connector = connectorFactory.Create(OneSConnectorFactory.DefaultVersion))
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
                    return new OpenStateObject(parameters.ConnectorFactory, globalContext, parameters.ConnectionString, parameters.PoolTimeout, parameters.PoolCapacity, version);
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

                var result = TransactionStateObject.Create(ConnectorFactory, GlobalContext, ConnectionString, PoolTimeout, PoolCapacity, Version, connection);
                UseGlobalContext();
                return result;
            }
        }
    }
}