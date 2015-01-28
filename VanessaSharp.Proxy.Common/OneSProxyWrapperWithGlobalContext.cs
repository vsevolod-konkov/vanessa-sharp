using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Обертка с глобальным контекстом.</summary>
    internal sealed class OneSProxyWrapperWithGlobalContext : OneSProxyWrapper
    {
        /// <summary>Список типов для которых создается реализация по умолчанию.</summary>
        private static readonly ISet<Type> _defaultTypes = new HashSet<Type>
            {
                typeof(object),
                typeof(IDisposable),
                typeof(IGlobalContextBound),
            };
        
        /// <summary>Глобальный контекст.</summary>
        private readonly OneSGlobalContext _globalContext;

        /// <summary>Фабрика оберток.</summary>
        private readonly IOneSWrapFactory _wrapFactory;

        /// <summary>Конвертер перечислений.</summary>
        private readonly IOneSEnumMapper _enumMapper;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_globalContext != null);
            Contract.Invariant(_wrapFactory != null);
            Contract.Invariant(_enumMapper != null);
        }

        /// <summary>Конструктор для тестирования.</summary>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        /// <param name="wrapFactory">Фабрика оберток</param>
        /// <param name="enumMapper">Конвертер перечислений.</param>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        internal OneSProxyWrapperWithGlobalContext(
            OneSGlobalContext globalContext, IOneSWrapFactory wrapFactory, IOneSEnumMapper enumMapper, IOneSObjectDefiner oneSObjectDefiner)
            : base(oneSObjectDefiner)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);
            Contract.Requires<ArgumentNullException>(wrapFactory != null);
            Contract.Requires<ArgumentNullException>(enumMapper != null);

            _globalContext = globalContext;
            _wrapFactory = wrapFactory;
            _enumMapper = enumMapper;
        }

        /// <summary>Конструктор для использования.</summary>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        /// <param name="wrapFactory">Фабрика оберток</param>
        /// <param name="enumMapper">Конвертер перечислений.</param>
        internal OneSProxyWrapperWithGlobalContext(OneSGlobalContext globalContext, IOneSWrapFactory wrapFactory, IOneSEnumMapper enumMapper)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);
            Contract.Requires<ArgumentNullException>(wrapFactory != null);
            Contract.Requires<ArgumentNullException>(enumMapper != null);

            _globalContext = globalContext;
            _wrapFactory = wrapFactory;
            _enumMapper = enumMapper;
        }

        /// <summary>Конструктор.</summary>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        public OneSProxyWrapperWithGlobalContext(OneSGlobalContext globalContext)
            : this(globalContext, OneSWrapFactory.Default, globalContext.EnumMapper)
        {}

        /// <summary>Обертывание 1С-объекта.</summary>
        /// <param name="comObj">Обертываемый объект.</param>
        /// <param name="type">Тип к которому можно привести возвращаемую обертку.</param>
        protected override OneSObject WrapOneSObject(object comObj, Type type)
        {
            if (_defaultTypes.Contains(type))
                return new OneSContextBoundObject(comObj, this, _globalContext);

            try
            {
                return _wrapFactory.CreateWrap(comObj,
                            new CreateWrapParameters(type, this, _globalContext));
            }
            catch (NotSupportedException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Невозможно создание обертки над объектом 1С поддерживающего тип \"{0}\", так как данный тип не поддерживается.",
                    type), e);
            }
        }

        /// <summary>
        /// Конвертация 1С-объекта в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемый COM-объект.</param>
        /// <param name="enumType">Перечислимый тип.</param>
        protected override object ConvertToEnum(object comObj, Type enumType)
        {
            return _enumMapper.ConvertComObjectToEnum(comObj, enumType);
        }
    }
}
