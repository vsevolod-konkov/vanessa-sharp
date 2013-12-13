using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние, когда соединение находится в транзакции.</summary>
        internal sealed class TransactionStateObject : OpenStateObjectBase
        {
            /// <summary>Объект транзакции.</summary>
            private readonly OneSTransaction _transaction;

            private TransactionStateObject(ConnectionParameters parameters, IGlobalContext globalContext, string version, OneSTransaction transaction)
                : base(parameters, globalContext, version)
            {
                ChecksHelper.CheckArgumentNotNull(transaction, "transaction");

                _transaction = transaction;
            }

            /// <summary>Создание транзакицонного состояния.</summary>
            /// <param name="parameters">Параметры подключения.</param>
            /// <param name="globalContext">Глобальный контекст 1С.</param>
            /// <param name="version">Версия сервера.</param>
            /// <param name="connection">Соединение.</param>
            public static StateObject Create(ConnectionParameters parameters, IGlobalContext globalContext, string version, OneSConnection connection)
            {
                ChecksHelper.CheckArgumentNotNull(globalContext, "globalContext");
                ChecksHelper.CheckArgumentNotEmpty(version, "version");
                ChecksHelper.CheckArgumentNotNull(connection, "connection");

                globalContext.BeginTransaction();
                try
                {
                    return new TransactionStateObject(parameters, globalContext, version, new OneSTransaction(connection));
                }
                catch
                {
                    globalContext.RollbackTransaction();
                    throw;
                }
            }

            public override StateObject CloseConnection()
            {
                GlobalContext.RollbackTransaction();
                return base.CloseConnection();
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
                var result = new OpenStateObject(GetConnectionParameters(), GlobalContext, Version);
                UseGlobalContext();
                return result;
            }

            public override OneSTransaction CurrentTransaction
            {
                get { return _transaction; }
            }
        }
    }
}