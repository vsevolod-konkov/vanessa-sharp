using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Состояние, когда соединение находится в транзакции.</summary>
        private sealed class TransactionStateObject : OpenStateObjectBase
        {
            /// <summary>Объект транзакции.</summary>
            private readonly OneSTransaction _transaction;

            private TransactionStateObject(IGlobalContext globalContext, string connectionString, int poolTimeout, int poolCapacity, string version, OneSTransaction transaction)
                : base(globalContext, connectionString, poolTimeout, poolCapacity, version)
            {
                ChecksHelper.CheckArgumentNotNull(transaction, "transaction");

                _transaction = transaction;
            }

            /// <summary>Создание транзакицонного состояния.</summary>
            /// <param name="globalContext">Глобальный контекст 1С.</param>
            /// <param name="version">Версия сервера.</param>
            /// <param name="connection">Соединение.</param>
            /// <param name="connectionString">Строка подключения к 1С.</param>
            /// <param name="poolTimeout">Время ожидания ответа от соединения.</param>
            /// <param name="poolCapacity">Мощность пула соединения.</param>
            public static StateObject Create(IGlobalContext globalContext, string connectionString, int poolTimeout, int poolCapacity, string version, OneSConnection connection)
            {
                ChecksHelper.CheckArgumentNotNull(globalContext, "globalContext");
                ChecksHelper.CheckArgumentNotEmpty(version, "version");
                ChecksHelper.CheckArgumentNotNull(connection, "connection");

                globalContext.BeginTransaction();
                try
                {
                    return new TransactionStateObject(globalContext, connectionString, poolTimeout, poolCapacity, version, new OneSTransaction(connection));
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
                var result = new OpenStateObject(GlobalContext, ConnectionString, PoolTimeout, PoolCapacity, Version);
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