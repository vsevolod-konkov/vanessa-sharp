using System;
using System.Data;
using System.Data.Common;

namespace VanessaSharp.Data
{
    /// <summary>Объект транзакции к информационной базе 1С.</summary>
    public sealed class OneSTransaction : DbTransaction
    {
        /// <summary>Соединение с информационной базой 1С.</summary>
        private readonly OneSConnection _connection;
        
        /// <summary>Конструктор.</summary>
        /// <param name="connection">Соединение с информационной базой 1С.</param>
        internal OneSTransaction(OneSConnection connection)
        {
            _connection = connection;
        }
        
        /// <summary>Принять транзакцию.</summary>
        public override void Commit()
        {
            _connection.CommitTransaction();
        }

        /// <summary>Объект соединения с информационной базой 1С.</summary>
        protected override DbConnection DbConnection
        {
            get { return _connection; }
        }

        /// <summary>Объект соединения с информационной базой 1С.</summary>
        public new OneSConnection Connection
        {
            get { return (OneSConnection)base.Connection; }
        }

        /// <summary>Уровень изоляции транзакции.</summary>
        /// <remarks>
        /// Так как в 1С существует только один уровень изоляции,
        /// то свойство всегда равно <see cref="System.Data.IsolationLevel.Unspecified"/>.
        /// </remarks>
        public override IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.Unspecified; }
        }

        /// <summary>Отмена транзакции.</summary>
        public override void Rollback()
        {
            _connection.RollbackTransaction();
        }
    }
}
