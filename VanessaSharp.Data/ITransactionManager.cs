namespace VanessaSharp.Data
{
    /// <summary>Контракт для управления транзакциями 1С.</summary>
    internal interface ITransactionManager
    {
        /// <summary>Принятие транзакции.</summary>
        void CommitTransaction();

        /// <summary>Отмена транзакции.</summary>
        void RollbackTransaction();
    }
}
