﻿using System;
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

        /// <summary>Сервисные функции глобального контекста.</summary>
        private readonly IGlobalContextService _globalContextService;

        /// <summary>Фабрика оберток.</summary>
        private readonly IOneSWrapFactory _wrapFactory;

        /// <summary>Конвертер перечислений.</summary>
        private readonly IOneSEnumMapper _enumMapper;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_globalContext != null);
            Contract.Invariant(_globalContextService != null);
            Contract.Invariant(_wrapFactory != null);
            Contract.Invariant(_enumMapper != null);
        }

        /// <summary>Конструктор для тестирования.</summary>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        /// <param name="globalContextService">Сервисные функции глобального контекста.</param>
        /// <param name="wrapFactory">Фабрика оберток</param>
        /// <param name="enumMapper">Конвертер перечислений.</param>
        internal OneSProxyWrapperWithGlobalContext(
            IOneSObjectDefiner oneSObjectDefiner,
            OneSGlobalContext globalContext,
            IGlobalContextService globalContextService,
            IOneSWrapFactory wrapFactory,
            IOneSEnumMapper enumMapper)
            : base(oneSObjectDefiner)
        {
            Contract.Requires<ArgumentNullException>(globalContext != null);
            Contract.Requires<ArgumentNullException>(globalContextService != null);
            Contract.Requires<ArgumentNullException>(wrapFactory != null);
            Contract.Requires<ArgumentNullException>(enumMapper != null);

            _globalContext = globalContext;
            _globalContextService = globalContextService;
            _wrapFactory = wrapFactory;
            _enumMapper = enumMapper;
        }

        /// <summary>Конструктор.</summary>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        /// <param name="globalContext">Ссылка на глобальный контекст 1С.</param>
        public OneSProxyWrapperWithGlobalContext(IOneSObjectDefiner oneSObjectDefiner, OneSGlobalContext globalContext)
            : this(oneSObjectDefiner, globalContext, globalContext, OneSWrapFactory.Default, new OneSEnumMapper(globalContext))
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
        protected override Enum ConvertToEnum(object comObj, Type enumType)
        {
            return _enumMapper.ConvertComObjectToEnum(comObj, enumType);
        }

        /// <summary>
        /// Конвертация 1С-объекта в <see cref="Guid"/>.
        /// </summary>
        /// <param name="comObj">Конвертируемый COM-объект.</param>
        protected override Guid ConvertToGuid(object comObj)
        {
            var objectString = _globalContextService.String(comObj);
            Guid result;
            if (Guid.TryParse(objectString, out result))
                return result;

            throw new InvalidCastException(string.Format(
                "1С объект имеющий строковое представление \"{0}\" не может быть приведен к типу \"{1}\".",
                objectString, typeof(Guid)));
        }

        /// <summary>
        /// Конвертация аргумента для 1С.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        public override object ConvertToOneS(object value)
        {
            if (value != null)
            {
                var valueType = value.GetType();

                if (valueType == typeof(Guid))
                {
                    return _globalContextService.NewUuid(value.ToString());
                }

                if (valueType.IsEnum)
                {
                    OneSObject result;
                    if (_enumMapper.TryConvertEnumToOneSObject((Enum)value, out result))
                        return result;
                }    
            }

            return base.ConvertToOneS(value);
        }
    }
}
