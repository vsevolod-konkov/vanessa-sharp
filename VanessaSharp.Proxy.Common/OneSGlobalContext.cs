namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Прокси к глобальному контексту 1С.
    /// </summary>
    public sealed class OneSGlobalContext : OneSObject, IGlobalContext
    {
        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        public OneSGlobalContext(object comObject)
            : base(comObject, new OneSProxyWrapper())
        {}

        /// <summary>Получение реального обертывателя.</summary>
        /// <param name="originWrapper">Исходный обертыватель.</param>
        internal override IOneSProxyWrapper GetOneSProxyWrapper(IOneSProxyWrapper originWrapper)
        {
            return new OneSProxyWrapperWithGlobalContext(this);
        }

        /// <summary>Создание нового объекта.</summary>
        /// <param name="typeName">Имя типа создаваемого объекта.</param>
        public dynamic NewObject(string typeName)
        {
            return DynamicProxy.NewObject(typeName);
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
    }
}
