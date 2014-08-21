using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataCursor"/>.
    /// </summary>
    internal sealed class DataCursor : IDataCursor
    {
        /// <summary>Выборка из результата запроса.</summary>
        private readonly IQueryResultSelection _queryResultSelection;
        
        /// <summary>Конструктор.</summary>
        /// <param name="queryResultSelection">Выборка из результата запроса.</param>
        public DataCursor(IQueryResultSelection queryResultSelection)
        {
            Contract.Requires<ArgumentNullException>(queryResultSelection != null);

            _queryResultSelection = queryResultSelection;
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _queryResultSelection.Dispose();
        }

        /// <summary>Переход на следующую запись.</summary>
        /// <returns>
        /// Возвращает <c>true</c>, если следующая запись была и переход состоялся.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public bool Next()
        {
            return _queryResultSelection.Next();
        }

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="ordinal">
        /// Порядковый номер поля.
        /// </param>
        public object GetValue(int ordinal)
        {
            return _queryResultSelection.Get(ordinal);
        }

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="name">
        /// Имя поля.
        /// </param>
        public object GetValue(string name)
        {
            return _queryResultSelection.Get(name);
        }
    }
}
