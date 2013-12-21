namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс коллекции колонок результата запроса.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSQueryResultColumnsCollection))]
    public interface IQueryResultColumnsCollection : IGlobalContextBound
    {
        /// <summary>Количество колонок в коллекции.</summary>
        int Count { get; }

        /// <summary>Колонка.</summary>
        /// <param name="index">Индекс колонки.</param>
        IQueryResultColumn Get(int index);
    }
}