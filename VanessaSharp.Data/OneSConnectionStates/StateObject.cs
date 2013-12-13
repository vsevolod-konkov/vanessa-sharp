using System;
using System.Data;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Базовый класс объекта состояния.</summary>
        internal abstract class StateObject : IDisposable
        {
            /// <summary>Объект глобального контекста 1С.</summary>
            public virtual IGlobalContext GlobalContext
            {
                get
                {
                    throw new InvalidOperationException(
                        "Нельзя получить глобальный контекст при закрытом соединении.");
                }
            }

            /// <summary>Открытие соединение.</summary>
            /// <returns>Объект состояния открытого соединения.</returns>
            public abstract StateObject OpenConnection();

            /// <summary>Закрытие соединения.</summary>
            /// <returns>Объект закрытого состояния.</returns>
            public abstract StateObject CloseConnection();

            /// <summary>Состояние соединения.</summary>
            public abstract ConnectionState ConnectionState { get; }

            /// <summary>Строка подключения к 1С.</summary>
            public abstract string ConnectionString { get; set; }

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
                throw new InvalidOperationException(
                    "Зафиксировать транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Отмена транзакции.</summary>
            public virtual StateObject RollbackTransaction()
            {
                throw new InvalidOperationException(
                    "Отменить транзакцию нельзя, так как соединение не находится в состоянии транзакции.");
            }

            /// <summary>Текущая транзакция.</summary>
            public virtual OneSTransaction CurrentTransaction
            {
                get { return null; }
            }

            /// <summary>Создание объекта состояния по умолчанию.</summary>
            public static StateObject CreateDefault()
            {
                return new ClosedStateObject();
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
        }
    }
}