using System;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Интерфейс курсора данных 
    /// - инкапсулирует кэширование.
    /// </summary>
    internal interface IDataCursor : IDisposable
    {
        /// <summary>Переход на следующую запись.</summary>
        /// <returns>
        /// Возвращает <c>true</c>, если следующая запись была и переход состоялся.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        bool Next();

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="ordinal">
        /// Порядковый номер поля.
        /// </param>
        object GetValue(int ordinal);

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="name">
        /// Имя поля.
        /// </param>
        object GetValue(string name);
    }
}
