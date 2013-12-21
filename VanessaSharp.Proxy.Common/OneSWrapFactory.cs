using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Стандартная реализация 
    /// <see cref="IOneSWrapFactory"/>
    /// и
    /// <see cref="IOneSTypeResolver"/>.
    /// </summary>
    public sealed class OneSWrapFactory : IOneSWrapFactory, IOneSTypeResolver
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static OneSWrapFactory Default
        {
            get { return _default; }
        }
        private static readonly OneSWrapFactory _default = new OneSWrapFactory();
        
        /// <summary>Создание обертки.</summary>
        /// <param name="comObject">RCW-обертка над объектом 1С.</param>
        /// <param name="parameters">Параметры для создания обертки.</param>
        public OneSObject CreateWrap(object comObject, CreateWrapParameters parameters)
        {
            var type = parameters.RequiredType;

            // TODO: Пока делается по месту
            if (type == typeof(IQuery))
            {
                return new OneSQuery(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
            }

            if (type == typeof(IQueryResult))
            {
                return new OneSQueryResult(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
            }

            if (type == typeof(IQueryResultColumnsCollection))
            {
                return new OneSQueryResultColumnsCollection(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
            }

            if (type == typeof(IQueryResultColumn))
            {
                return new OneSQueryResultColumn(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
            }

            if (type == typeof(IValueType))
            {
                return new OneSValueType(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
            }

            return new OneSContextBoundObject(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
        }

        /// <summary>
        /// Возвращение имени типа объекта в 1С
        /// соответствующего типу CLR.
        /// </summary>
        /// <param name="requestedType">
        /// Тип CLR для которого ищется соответствие среди типов 1С.
        /// </param>
        public string GetTypeNameFor(Type requestedType)
        {
            // TODO: Пока делается по месту
            if (requestedType == typeof(IQuery))
                return "Query";

            throw new InvalidOperationException(string.Format(
                "Неизвестно имя типа в 1С поддерживающего интрефейс типа \"{0}\".",
                requestedType));
        }
    }
}