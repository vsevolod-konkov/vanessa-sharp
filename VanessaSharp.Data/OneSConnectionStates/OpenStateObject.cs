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
            public OpenStateObject(IGlobalContext globalContext, string connectionString, int poolTimeout, int poolCapacity, string version)
                : base(globalContext, connectionString, poolTimeout, poolCapacity, version)
            { }

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
                    return new OpenStateObject(globalContext, parameters.ConnectionString, parameters.PoolTimeout, parameters.PoolCapacity, version);
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

                var result = TransactionStateObject.Create(GlobalContext, ConnectionString, PoolTimeout, PoolCapacity, Version, connection);
                UseGlobalContext();
                return result;
            }
        }
    }
}