using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

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
        private static readonly OneSWrapFactory _default = new OneSWrapFactory(OneSObjectMappingProvider.GetObjectMappings());

        /// <summary>Карта соответствия типов и действий по из созданию.</summary>
        private readonly IOneSWrapMap _map;

        /// <summary>Конструктор принимающий коллекцию структур-описателей соответствия.</summary>
        /// <param name="objectMappings">Коллекция структур-описателей соответствия.</param>
        public OneSWrapFactory(IEnumerable<OneSObjectMapping> objectMappings)
            : this(CreateWrapMap(objectMappings))
        {
            Contract.Requires<ArgumentNullException>(objectMappings != null);
        }

        /// <summary>
        /// Создание карты оберток.
        /// </summary>
        private static IOneSWrapMap CreateWrapMap(IEnumerable<OneSObjectMapping> objectMappings)
        {
            Contract.Requires<ArgumentNullException>(objectMappings != null);

            var map = new OneSWrapMap();
            foreach (var mapping in objectMappings)
            {
                try
                {
                    map.AddObjectMapping(mapping);
                }
                catch (Exception e)
                {
                    //  TODO: перейти на log4net
                    Trace.Write(e);
                }
            }

            return map;
        }

        /// <summary>Конструктор принимающий карту соответствия.</summary>
        /// <param name="map">Карта.</param>
        internal OneSWrapFactory(IOneSWrapMap map)
        {
            Contract.Requires<ArgumentNullException>(map != null);

            _map = map;
        }

        /// <summary>Создание обертки.</summary>
        /// <param name="comObject">RCW-обертка над объектом 1С.</param>
        /// <param name="parameters">Параметры для создания обертки.</param>
        public OneSObject CreateWrap(object comObject, CreateWrapParameters parameters)
        {
            var creator = _map.GetObjectCreator(parameters.RequiredType);
            if (creator == null)
            {
                throw new NotSupportedException(string.Format(
                    "Для типа \"{0}\" невозможно создать типизированную обертку.",
                    parameters.RequiredType));
            }

            return creator(comObject, parameters.ProxyWrapper, parameters.GlobalContext);
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
            var typeName = _map.GetOneSObjectTypeName(requestedType);

            if (typeName == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Неизвестно имя типа в 1С поддерживающего интерфейс типа \"{0}\".",
                    requestedType));
            }

            return typeName;
        }
    }
}