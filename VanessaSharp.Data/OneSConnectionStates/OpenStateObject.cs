using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние открытого соединения.</summary>
        internal sealed class OpenStateObject : OpenStateObjectBase
        {
            /// <summary>Конструктор.</summary>
            /// <param name="parameters">Параметры подключения.</param>
            /// <param name="globalContext">Глобальный контекст.</param>
            /// <param name="version">Версия 1С.</param>
            public OpenStateObject(ConnectionParameters parameters, IGlobalContext globalContext, string version)
                : base(parameters, globalContext, version)
            { }

            /// <summary>Подключение к базе 1С.</summary>
            /// <param name="creationParams">Рекомендуемые параметры создания коннектора к информационной базе 1С.</param>
            /// <param name="parameters">Параметры подключения.</param>
            /// <param name="version">Версия.</param>
            private static IGlobalContext Connect(ConnectorCreationParams creationParams, ConnectionParameters parameters, out string version)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                var connectorFactory = parameters.ConnectorFactory ?? OneSConnectorFactory.Default;

                using (var connector = connectorFactory.Create(creationParams))
                {
                    connector.Init(parameters.Initializer);
                    connector.PoolTimeout = (uint)parameters.PoolTimeout;
                    connector.PoolCapacity = (uint)parameters.PoolCapacity;
                    version = connector.Version;

                    return connector.Connect(parameters.ConnectionString);
                }
            }

            /// <summary>Создание экземпляра состояния.</summary>
            /// <param name="creationParams">Рекомендуемые параметры создания коннектора к информационной базе 1С.</param>
            /// <param name="parameters">Параметры подключения.</param>
            public static StateObject Create(ConnectorCreationParams creationParams, ConnectionParameters parameters)
            {
                Contract.Requires<ArgumentNullException>(parameters != null);

                string version;
                var globalContext = Connect(creationParams, parameters, out version);
                try
                {
                    return new OpenStateObject(parameters, globalContext, version);
                }
                catch
                {
                    globalContext.Dispose();
                    throw;
                }
            }

            /// <summary>Начало транзакции.</summary>
            public override StateObject BeginTransaction(OneSConnection connection)
            {
                var result = TransactionStateObject.Create(GetConnectionParameters(), GlobalContext, Version, connection);
                UseGlobalContext();
                
                return result;
            }
        }
    }
}