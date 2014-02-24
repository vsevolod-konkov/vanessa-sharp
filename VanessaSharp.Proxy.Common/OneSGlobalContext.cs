using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Прокси к глобальному контексту 1С.
    /// </summary>
    public sealed class OneSGlobalContext : OneSObject, IGlobalContext
    {
        // TODO: Нужен Рефакторинг. Убрать после внедрения параметрических запросов. Нужна просто фабрика объектов. Думаю нужен еще один класс использующий нетипизированный метод.
        /// <summary>Определитель типов 1С.</summary>
        private readonly IOneSTypeResolver _oneSTypeResolver;

        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        public OneSGlobalContext(object comObject)
            : this(comObject, OneSWrapFactory.Default)
        {}

        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="oneSTypeResolver">Определитель типов 1С.</param>
        public OneSGlobalContext(object comObject, IOneSTypeResolver oneSTypeResolver)
            : base(o => CreateOneSProxy(comObject, (OneSGlobalContext) o))
        {
            Contract.Requires<ArgumentNullException>(oneSTypeResolver != null);

            _oneSTypeResolver = oneSTypeResolver;
        }

        /// <summary>Создание прокси для глобального контекста.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="globalContext">Ссылка на глобальный контекст.</param>
        private static OneSProxy CreateOneSProxy(object comObject, OneSGlobalContext globalContext)
        {
            return new OneSProxy(
                comObject,
                new OneSProxyWrapperWithGlobalContext(globalContext));
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
    }
}
