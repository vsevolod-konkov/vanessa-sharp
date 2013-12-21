namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Интерфейс колонки результат запроса данных 1С.
    /// </summary>
    [OneSObjectMapping(WrapType = typeof(OneSQueryResultColumn))]
    public interface IQueryResultColumn : IGlobalContextBound
    {
        /// <summary>Наименование колонки.</summary>
        string Name { get; }

        /// <summary>Тип колонки.</summary>
        IValueType ValueType { get; }
    }
}
