using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Реализация <see cref="IOneSWrapMap"/>.</summary>
    internal sealed class OneSWrapMap : IOneSWrapMap
    {
        /// <summary>Делегаты создания объектов.</summary>
        private readonly Dictionary<Type, Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>> _creators
            = new Dictionary<Type, Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>>();

        /// <summary>Имена объектов в 1С.</summary>
        private readonly Dictionary<Type, string> _oneSTypeNames = new Dictionary<Type, string>();

        /// <summary>Добавление маппинга.</summary>
        /// <param name="mapping">Добавляемый маппинг.</param>
        public void AddObjectMapping(OneSObjectMapping mapping)
        {
            Contract.Requires<ArgumentNullException>(mapping != null);

            var creator = CheckAndBuildCreator(mapping.InterfaceType, mapping.WrapType);
            // TODO: Сделать обработку в случае наличия типа интерфейса в словаре
            _creators.Add(mapping.InterfaceType, creator);

            if (mapping.OneSTypeName != null)
                _oneSTypeNames.Add(mapping.InterfaceType, mapping.OneSTypeName);
        }

        /// <summary>Проверка и создание делегата создания типа.</summary>
        /// <param name="interfaceType">Тип интерфейса.</param>
        /// <param name="wrapType">Тип обертки реализующей интерфейс.</param>
        internal static Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> 
            CheckAndBuildCreator(Type interfaceType, Type wrapType)
        {
            Contract.Requires<ArgumentNullException>(interfaceType != null);
            Contract.Requires<ArgumentNullException>(wrapType != null);
            Contract.Ensures(Contract.Result<Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>>() != null);

            CheckPossibleCreator(interfaceType, wrapType);

            return BuildCreator(wrapType);
        }

        /// <summary>
        /// Проверка возможности делегата создания.
        /// </summary>
        /// <param name="interfaceType">Тип интерфейса.</param>
        /// <param name="wrapType">Тип обертки.</param>
        private static void CheckPossibleCreator(Type interfaceType, Type wrapType)
        {
            if (!interfaceType.IsAssignableFrom(wrapType))
            {
                throw new ArgumentException(string.Format(
                    "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                    + " так как указанный тип обертки \"{1}\" этот интерфейс не поддерживает.",
                    interfaceType, wrapType));
            }
            
            if (!typeof(OneSContextBoundObject).IsAssignableFrom(wrapType))
            {
                throw new ArgumentException(
                    string.Format(
                        "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                        + " так как указанный тип обертки \"{1}\" не наследуется от \"{2}\"",
                        interfaceType, wrapType, typeof(OneSContextBoundObject)));
            }
        }

        /// <summary>Построение делегата создания.</summary>
        /// <param name="wrapType">Тип создаваемой обертки.</param>
        private static Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> BuildCreator(Type wrapType)
        {
            var matchedConstructor = FindMatchedConstructor(wrapType);

            var parameterComObject = Expression.Parameter(typeof(object), "comObject");
            var parameterProxyWrapper = Expression.Parameter(typeof(IOneSProxyWrapper), "proxyWrapper");
            var parameterGlobalContext = Expression.Parameter(typeof(OneSGlobalContext), "globalContext");

            var parameters = new[] {parameterComObject, parameterProxyWrapper, parameterGlobalContext};
            var parametersMapByType = parameters.ToDictionary(p => p.Type);

            var parametersForCallConstructor = (
                                                   from p in matchedConstructor.GetParameters()
                                                   select (Expression)parametersMapByType[p.ParameterType]
                                               )
                                               .ToArray();
            var creatorBody = Expression.Convert(
                                Expression.New(matchedConstructor, parametersForCallConstructor),
                                typeof(OneSObject));

            var creatorLambda = Expression.Lambda<Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>>(
                creatorBody, parameters);

            return creatorLambda.Compile();
        }

        /// <summary>Поиск подходящего конструктора.</summary>
        /// <param name="wrapType">Тип обертки.</param>
        private static ConstructorInfo FindMatchedConstructor(Type wrapType)
        {
            var argTypes = new HashSet<Type> {typeof(object), typeof(IOneSProxyWrapper), typeof(OneSGlobalContext)};

            var candidates = from constructor in wrapType.GetConstructors(BindingFlags.Public | BindingFlags.Instance) 
                             let parameters = constructor.GetParameters() 
                             where parameters.Length <= argTypes.Count 
                             let parameterTypes = parameters.Select(p => p.ParameterType)
                             where argTypes.IsSupersetOf(parameterTypes)
                             select constructor;

            ConstructorInfo result;
            try
            {
                result = candidates.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(
                    string.Format(
                        "Невозможно создать экземпляр типизированной обертки над объектом 1С"
                        + " так как указанный тип обертки \"{0}\" имеет несколько подходящих конструкторов."
                        +
                        " Тип должен иметь только один публичный конструктор у которого кроме аргументов с типами: \"{1}\", \"{2}\", \"{3}\" других не имеется.",
                        wrapType,
                        typeof(object),
                        typeof(IOneSProxyWrapper),
                        typeof(OneSGlobalContext)));
            }

            if (result == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Невозможно создать экземпляр типизированной обертки над объектом 1С"
                        + " так как указанный тип обертки \"{0}\" не имеет подходящего конструктора."
                        +
                        " Тип должен иметь публичный конструктор у которого кроме аргументов с типами: \"{1}\", \"{2}\", \"{3}\" других не имеется.",
                        wrapType,
                        typeof(object),
                        typeof(IOneSProxyWrapper),
                        typeof(OneSGlobalContext)));
            }

            return result;
        }

        /// <summary>
        /// Получение делегата создания обертки по запрашиваемому типу.
        /// </summary>
        /// <param name="type">Запрашиваемый тип обертки.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет создателя.</returns>
        public Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> GetObjectCreator(Type type)
        {
            Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> result;
            _creators.TryGetValue(type, out result);

            return result;
        }

        /// <summary>
        /// Получение имени типа в 1С соответствующего типу CLR.
        /// </summary>
        /// <param name="type">Запрашиваемый тип CLR.</param>
        /// <returns>Возвращает <c>null</c> если для запрашиваемого типа нет соответствующего имени.</returns>
        public string GetOneSObjectTypeName(Type type)
        {
            string result;
            _oneSTypeNames.TryGetValue(type, out result);

            return result;
        }
    }
}
