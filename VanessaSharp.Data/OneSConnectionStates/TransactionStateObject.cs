using System;
using System.Diagnostics.Contracts;
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

            internal TransactionStateObject(ConnectionParameters parameters, IGlobalContext globalContext, string version, OneSTransaction transaction)
                : base(parameters, globalContext, version)
            {
                Contract.Requires<ArgumentNullException>(transaction != null);

                _transaction = transaction;
            }

            /// <summary>Создание транзакицонного состояния.</summary>
            /// <param name="parameters">Параметры подключения.</param>
            /// <param name="globalContext">Глобальный контекст 1С.</param>
            /// <param name="version">Версия сервера.</param>
            /// <param name="connection">Соединение.</param>
            public static StateObject Create(ConnectionParameters parameters, IGlobalContext globalContext, string version, OneSConnection connection)
            {
                Contract.Requires<ArgumentNullException>(globalContext != null);
                Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(version));
                Contract.Requires<ArgumentNullException>(connection != null);

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

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public override StateObject CloseConnection()
            {
                GlobalContext.RollbackTransaction();
                return base.CloseConnection();
            }

            /// <summary>Начало транзакции.</summary>
            public override StateObject BeginTransaction(OneSConnection connection)
            {
                throw new InvalidOperationException(
                    "Соединение уже находится в состоянии транзакции. 1С не поддерживает вложенные транзакции");
            }

            /// <summary>Принятие транзакции.</summary>
            public override StateObject CommitTransaction()
            {
                GlobalContext.CommitTransaction();
                return CreateOpenState();
            }

            /// <summary>Отмена транзакции.</summary>
            public override StateObject RollbackTransaction()
            {
                GlobalContext.RollbackTransaction();
                return CreateOpenState();
            }

            /// <summary>
            /// Создание состояния открытого соединения без транзакции.
            /// </summary>
            private StateObject CreateOpenState()
            {
                var result = new OpenStateObject(GetConnectionParameters(), GlobalContext, Version);
                UseGlobalContext();
                return result;
            }

            /// <summary>Текущая транзакция.</summary>
            public override OneSTransaction CurrentTransaction
            {
                get { return _transaction; }
            }
        }
    }
}