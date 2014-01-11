using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Типизированная обертка на объектом 1С
    /// коллекции колонок результата запроса.
    /// </summary>
    public sealed class OneSQueryResultColumnsCollection 
        : OneSContextBoundObject, IQueryResultColumnsCollection
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQueryResultColumnsCollection
            (object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext) 
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Количество колонок в коллекции.</summary>
        public int Count
        {
            get { return DynamicProxy.Count; }
        }

        /// <summary>Колонка.</summary>
        /// <param name="index">Индекс колонки.</param>
        public IQueryResultColumn Get(int index)
        {
            return DynamicProxy.Get(index);
        }

        /// <summary>Поиск колонки, по имени.</summary>
        /// <param name="columnName">Имя колонки.</param>
        public IQueryResultColumn Find(string columnName)
        {
            return DynamicProxy.Find(columnName);
        }

        /// <summary>Индекс колонки.</summary>
        /// <param name="column">Колонка</param>
        /// <returns>Если колонка не принадлежит данной коллекции возвращается -1.</returns>
        public int IndexOf(IQueryResultColumn column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            if (column.GlobalContext != GlobalContext)
            {
                throw new ArgumentException(
                    "Параметр - колонка результата запроса должен принадлежать тому же глобальному контексту, что и вызываемая коллекция.",
                    "column");
            }

            if (!(column is OneSQueryResultColumn))
            {
                throw new ArgumentException(
                    string.Format("Параметр должен быть экземпляром типа \"{0}\".", typeof(OneSQueryResultColumn)), 
                    "column");
            }

            return DynamicProxy.IndexOf(column);
        }
    }
}
