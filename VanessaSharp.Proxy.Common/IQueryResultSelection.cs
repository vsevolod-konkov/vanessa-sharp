namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс курсора результата запроса.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSQueryResultSelection))]
    public interface IQueryResultSelection : IGlobalContextBound
    {
        /// <summary>Чтение следующей записи.</summary>
        bool Next();

        /// <summary>
        /// Получение значения поля.
        /// </summary>
        /// <param name="index">Индекс поля.</param>
        object Get(int index);

        /// <summary>
        /// Получение значения поля по  имени поля.
        /// </summary>
        /// <param name="fieldName">Имя поля.</param>
        object Get(string fieldName);

        /// <summary>
        /// Уровень текущей записи.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Имя группы текущей записи.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// Тип текущей записи.
        /// </summary>
        SelectRecordType RecordType { get; }
    }
}