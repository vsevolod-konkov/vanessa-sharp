﻿using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Прокси к глобальному контексту 1С.
    /// </summary>
    public sealed class OneSGlobalContext : OneSObject, IGlobalContext, IGlobalContextService
    {
        // TODO: Нужен Рефакторинг. Убрать после внедрения параметрических запросов. Нужна просто фабрика объектов. Думаю нужен еще один класс использующий нетипизированный метод.
        /// <summary>Определитель типов 1С.</summary>
        private readonly IOneSTypeResolver _oneSTypeResolver;

        private readonly DisposableTable _disposableTable = new DisposableTable();

        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        public OneSGlobalContext(object comObject)
            : this(comObject, OneSObjectDefiner.Default, OneSWrapFactory.Default)
        {}

        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        /// <param name="oneSTypeResolver">Определитель типов 1С.</param>
        internal OneSGlobalContext(object comObject, IOneSObjectDefiner oneSObjectDefiner, IOneSTypeResolver oneSTypeResolver)
            : base(o => CreateOneSProxy(comObject, (OneSGlobalContext)o, oneSObjectDefiner))
        {
            Contract.Requires<ArgumentNullException>(oneSTypeResolver != null);

            _oneSTypeResolver = oneSTypeResolver;
        }

        /// <summary>Создание прокси для глобального контекста.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="globalContext">Ссылка на глобальный контекст.</param>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        private static OneSProxy CreateOneSProxy(object comObject, OneSGlobalContext globalContext, IOneSObjectDefiner oneSObjectDefiner)
        {
            return new OneSProxy(
                comObject,
                new OneSProxyWrapperWithGlobalContext(oneSObjectDefiner, globalContext));
        }

        /// <summary>Регистрация объекта 1С связанного с текущим контекстом.</summary>
        internal void RegisterOneSObject(OneSContextBoundObject oneSObject)
        {
            Contract.Requires<ArgumentNullException>(oneSObject != null);

            _disposableTable.RegisterObject(oneSObject);
        }

        // TODO: Нужен Рефакторинг. Оставить после внедрения параметрических запросов. Нужна просто фабрика объектов. Думаю нужен еще один класс использующий этот метод.
        /// <summary>Создание нового объекта.</summary>
        /// <param name="typeName">Имя типа создаваемого объекта.</param>
        public dynamic NewObject(string typeName)
        {
            return DynamicProxy.NewObject(typeName);
        }

        // TODO: Нужен Рефакторинг. Убрать после внедрения параметрических запросов. Нужна просто фабрика объектов. Думаю нужен еще один класс использующий нетипизированный метод.
        /// <summary>Создание объекта.</summary>
        /// <typeparam name="T">Тип интерфейса соответствующего типу объекта 1С.</typeparam>
        public T NewObject<T>() where T : IGlobalContextBound
        {
            return NewObject(
                GetOneSTypeNameForNewObject<T>());
        }

        // TODO: Нужен Рефакторинг. Убрать после внедрения параметрических запросов. Нужна просто фабрика объектов. Думаю нужен еще один класс использующий нетипизированный метод.
        /// <summary>
        /// Получение имени типа в 1С для объекта реализующего 
        /// запрашиваемый тип.
        /// </summary>
        /// <typeparam name="T">Запрашиваемый тип.</typeparam>
        private string GetOneSTypeNameForNewObject<T>()
        {
            var requestedType = typeof(T);
            try
            {
                return _oneSTypeResolver.GetTypeNameFor(requestedType);
            }
            catch (InvalidOperationException e)
            {
                throw new NotSupportedException(string.Format(
                "Тип \"{0}\" для создания через метод NewObject{{T}} не поддерживается.",
                requestedType), e);
            }
        }

        // TODO: Нужен Рефакторинг. Временный метод. Нужно убрать после рефакторинга методов NewObject.
        /// <summary>
        /// Создание объекта 1С типа UUID по строке.
        /// </summary>
        /// <param name="guidString">Строквое представление идентификатора GUID.</param>
        object IGlobalContextService.NewUuid(string guidString)
        {
            return DynamicProxy.NewObject("UUID", guidString);
        }

        /// <summary>
        /// Получение признака - монопольный ли режим.
        /// </summary>
        public bool ExclusiveMode()
        {
            return DynamicProxy.ExclusiveMode();
        }

        /// <summary>
        /// Установка монопольного режима.
        /// </summary>
        /// <param name="value">
        /// Если передать <c>true</c>, то установится монопольный режим в ином случае нет.
        /// </param>
        public void SetExclusiveMode(bool value)
        {
            DynamicProxy.SetExclusiveMode(value);
        }

        /// <summary>
        /// Открывает транзакцию.
        /// Транзакция предназначена для записи в информационную базу согласованных изменений.
        /// Все изменения, внесенные в информационную базу после начала транзакции, будут затем либо целиком записаны, либо целиком отменены.
        /// </summary>
        public void BeginTransaction()
        {
            DynamicProxy.BeginTransaction();
        }

        /// <summary>
        /// Завершает успешную транзакцию.
        /// Все изменения, внесенные в информационную базу в процессе транзакции, будут записаны.
        /// </summary>
        public void CommitTransaction()
        {
            DynamicProxy.CommitTransaction();
        }

        /// <summary>
        /// Отменяет открытую ранее транзакцию.
        /// Все изменения, внесенные в информационную базу в процессе транзакции, будут отменены.
        /// </summary>
        public void RollbackTransaction()
        {
            DynamicProxy.RollbackTransaction();
        }

        /// <summary>Строковое представление объекта.</summary>
        /// <param name="obj">Объект.</param>
        public string String(object obj)
        {
            return DynamicProxy.String(obj);
        }

        /// <summary>Освобождение ресурсов.</summary>
        public override void Dispose()
        {
            base.Dispose();

            _disposableTable.Dispose();
        }
    }
}
