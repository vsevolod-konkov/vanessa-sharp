using System;
using System.Diagnostics.Contracts;

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

        /// <summary>Поиск колонки, по имени.</summary>
        /// <param name="columnName">Имя колонки.</param>
        IQueryResultColumn Find(string columnName);

        /// <summary>Индекс колонки.</summary>
        /// <param name="column">Колонка</param>
        /// <returns>Если колонка не принадлежит данной коллекции возвращается -1.</returns>
        int IndexOf(IQueryResultColumn column);
    }
}