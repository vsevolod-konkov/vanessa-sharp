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
    }
}