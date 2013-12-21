namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс результата запроса.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSQueryResult))]
    public interface IQueryResult : IGlobalContextBound
    {
        /// <summary>Коллекция колонок результата запроса.</summary>
        IQueryResultColumnsCollection Columns { get; }

        /// <summary>Результат запроса пуст.</summary>
        bool IsEmpty();

        /// <summary>Выбрать результат запроса в курсор.</summary>
        IQueryResultSelection Choose();
    }
}