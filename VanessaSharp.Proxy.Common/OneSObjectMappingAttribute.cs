using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Атрибут описывающий соответствие между
    /// интерфейсом и объектом 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class OneSObjectMappingAttribute : Attribute
    {
        /// <summary>Тип обертки реализующей интерфейс.</summary>
        public Type WrapType { get; set; }

        /// <summary>Имя типа в 1С.</summary>
        public string OneSTypeName { get; set; }
    }
}
