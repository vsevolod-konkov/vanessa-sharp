using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
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
        private readonly IDictionary<Type, Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>> _creators
            = new ConcurrentDictionary<Type, Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>>();

        /// <summary>
        /// Создатель замкнутого обощенного типа реализации.
        /// </summary>
        private sealed class ImplementationTypeMaker
        {
            /// <summary>
            /// Незамкнутый обобщенный тип реализации.
            /// </summary>
            private readonly Type _genericImplDefinition;

            /// <summary>
            /// Подстановка индексов типов-параметров класса реализации в индексы
            /// типов-параметров запрашиваемого интерфейса.
            /// </summary>
            private readonly ReadOnlyCollection<int> _substitution;

            /// <summary>Конструктор.</summary>
            private ImplementationTypeMaker(
                Type genericImplDefinition, ReadOnlyCollection<int> substitution)
            {
                _genericImplDefinition = genericImplDefinition;
                _substitution = substitution;
            }

            /// <summary>Создание экземпляра.</summary>
            /// <param name="interfaceType">Незамкнутый обобщенный интерфейс.</param>
            /// <param name="implementationType">
            /// Незамкнутый обобщенный класс,
            /// который должен реализовать интерфейс.
            /// </param>
            public static ImplementationTypeMaker Create(Type interfaceType, Type implementationType)
            {
                Contract.Requires<ArgumentNullException>(interfaceType != null);
                Contract.Requires<ArgumentException>(interfaceType.IsInterface);
                Contract.Requires<ArgumentException>(interfaceType.IsGenericTypeDefinition);
                
                Contract.Requires<ArgumentNullException>(implementationType != null);
                Contract.Requires<ArgumentException>(!implementationType.IsInterface);
                Contract.Requires<ArgumentException>(implementationType.IsGenericTypeDefinition);

                Contract.Ensures(Contract.Result<ImplementationTypeMaker>() != null);

                var interfaceInImplType = (
                                      from i in implementationType.GetInterfaces()
                                      where i.IsGenericType 
                                      && i.GetGenericTypeDefinition() == interfaceType
                                      && i.GetGenericArguments().All(p => p.IsGenericParameter)
                                      select i
                                  ).FirstOrDefault();

                if (interfaceInImplType == null)
                {
                    throw new ArgumentException(string.Format(
                        "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                         + " так как указанный тип обертки \"{1}\" этот интерфейс не поддерживает.",
                        interfaceType, implementationType));
                }

                var typeArgumentsInImplType = interfaceInImplType.GetGenericArguments();

                var typeParameters = implementationType.GetGenericArguments();
                var substituteParameters = new int[typeParameters.Length];
                for (var index = 0; index < typeParameters.Length; index++)
                {
                    var paramType = typeParameters[index];

                    var indexInImplInterface = Array.IndexOf(typeArgumentsInImplType, paramType);
                    if (indexInImplInterface == -1)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Не найден тип-параметр по номеру \"{0}\" в типе реализации \"{1}\" для подстановки из интерфейса \"{2}\"",
                            index + 1, implementationType, interfaceType));
                    }

                    substituteParameters[index] = indexInImplInterface;
                }

                return new ImplementationTypeMaker(
                    implementationType, new ReadOnlyCollection<int>(substituteParameters));
            }

            /// <summary>
            /// Создание замкнутого обобщенного типа реализации
            /// по интерфейсу, который он должен реализовать.
            /// </summary>
            /// <param name="interfaceType">
            /// Интерфейс, который должен реализовать создаваемый тип.
            /// </param>
            public Type MakeType(Type interfaceType)
            {
                Contract.Requires<ArgumentNullException>(interfaceType != null);
                Contract.Requires<ArgumentException>(interfaceType.IsGenericType);

                Contract.Ensures(Contract.Result<Type>() != null);

                var typeArgumentsForInterface = interfaceType.GetGenericArguments();
                var typeArgumentsForImplementation = _substitution
                    .Select(i => typeArgumentsForInterface[i])
                    .ToArray();

                return _genericImplDefinition
                    .MakeGenericType(typeArgumentsForImplementation);
            }
        }

        /// <summary>
        /// Карта соответствия незамкнутых обобщенных интерфесов создателям замкнутых типов реализаций.
        /// </summary>
        private readonly IDictionary<Type, ImplementationTypeMaker> _genericTypeMakerMappings
            = new ConcurrentDictionary<Type, ImplementationTypeMaker>();

        /// <summary>Имена объектов в 1С.</summary>
        private readonly Dictionary<Type, string> _oneSTypeNames = new Dictionary<Type, string>();

        /// <summary>Добавление маппинга.</summary>
        /// <param name="mapping">Добавляемый маппинг.</param>
        public void AddObjectMapping(OneSObjectMapping mapping)
        {
            Contract.Requires<ArgumentNullException>(mapping != null);

            if (mapping.InterfaceType.IsGenericTypeDefinition)
            {
                var creatorBuilder = CheckAndCreateTypeMaker(mapping.InterfaceType, mapping.WrapType);

                AddToMap(_genericTypeMakerMappings, mapping, creatorBuilder);
            }
            else
            {
                var creator = CheckAndBuildCreator(mapping.InterfaceType, mapping.WrapType);

                AddToMap(_creators, mapping, creator);
            }

            if (mapping.OneSTypeName != null)
                _oneSTypeNames.Add(mapping.InterfaceType, mapping.OneSTypeName);
        }

        /// <summary>
        /// Добавление маппинга в соответствующую карту.
        /// </summary>
        private static void AddToMap<TValue>(
            IDictionary<Type, TValue> map, OneSObjectMapping mapping, TValue value)
        {
            try
            {
                map.Add(mapping.InterfaceType, value);
            }
            catch (KeyNotFoundException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Невозможно добавить соответствие интерфейсу \"{0}\" реализации \"{1}\" так как уже для данного интерфейса соответствие добавлено.",
                    mapping.InterfaceType, mapping.WrapType), e);
            }
        }

        /// <summary>Проверка и создание делегата создания типа.</summary>
        /// <param name="interfaceType">Тип интерфейса.</param>
        /// <param name="wrapType">Тип обертки реализующей интерфейс.</param>
        internal static Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> 
            CheckAndBuildCreator(Type interfaceType, Type wrapType)
        {
            Contract.Requires<ArgumentNullException>(interfaceType != null);
            Contract.Requires<ArgumentException>(!interfaceType.IsGenericTypeDefinition);
            Contract.Requires<ArgumentNullException>(wrapType != null);
            Contract.Ensures(Contract.Result<Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject>>() != null);

            CheckPossibleCreator(interfaceType, wrapType);

            return BuildCreator(wrapType);
        }

        /// <summary>
        /// Проверка возможности генерации делегата создания.
        /// </summary>
        /// <param name="interfaceType">Тип интерфейса.</param>
        /// <param name="wrapType">Тип обертки.</param>
        private static void CheckPossibleCreator(Type interfaceType, Type wrapType)
        {
            CheckDeriveOneSContextBoundObject(interfaceType, wrapType);
            
            if (!interfaceType.IsAssignableFrom(wrapType))
            {
                throw new ArgumentException(string.Format(
                    "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                    + " так как указанный тип обертки \"{1}\" этот интерфейс не поддерживает.",
                    interfaceType, wrapType));
            }
        }

        /// <summary>
        /// Проверка возможности генерации и генерация создателя замкнутого обобщенного типа
        /// для реализации.
        /// </summary>
        /// <param name="interfaceType">Незамкнутый обобщенный интерфейс.</param>
        /// <param name="wrapType">Незамкнутый обобщенный тип реализации.</param>
        private static ImplementationTypeMaker CheckAndCreateTypeMaker(Type interfaceType, Type wrapType)
        {
            Contract.Requires<ArgumentException>(interfaceType.IsGenericTypeDefinition);

            CheckPossibleCreatorForGeneric(interfaceType, wrapType);

            return ImplementationTypeMaker.Create(interfaceType, wrapType);
        }

        /// <summary>
        /// Проверка возможности генерации делегата создания
        /// в случае обобщенного типа.
        /// </summary>
        /// <param name="interfaceType">Тип интерфейса.</param>
        /// <param name="wrapType">Тип обертки.</param>
        private static void CheckPossibleCreatorForGeneric(Type interfaceType, Type wrapType)
        {
            CheckDeriveOneSContextBoundObject(interfaceType, wrapType);

            FindMatchedConstructor(wrapType);
        }

        /// <summary>
        /// Проверка того, что <paramref name="wrapType"/>
        /// наследует от <see cref="OneSContextBoundObject"/>.
        /// </summary>
        private static void CheckDeriveOneSContextBoundObject(Type interfaceType, Type wrapType)
        {
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
            if (_creators.TryGetValue(type, out result))
                return result;

            if (TryGetObjectCreatorFromGenericBuilderMapping(type, out result))
                return result;

            return null;
        }

        /// <summary>
        /// Получение делегата создания обертки по запрашиваемому типу интерфейса
        /// из создателя замкнутых типов карты соответствия.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryGetObjectCreatorFromGenericBuilderMapping(
            Type type,
            out Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> result)
        {
            result = null;

            if (!type.IsGenericType)
                return false;

            ImplementationTypeMaker typeMaker;
            if (!_genericTypeMakerMappings.TryGetValue(type.GetGenericTypeDefinition(), out typeMaker))
                return false;

            var wrapType = typeMaker.MakeType(type);
            result = BuildCreator(wrapType);

            // Кэширование
            _creators[type] = result;

            return true;
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
