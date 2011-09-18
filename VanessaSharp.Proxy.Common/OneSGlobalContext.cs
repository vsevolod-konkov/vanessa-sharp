using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
