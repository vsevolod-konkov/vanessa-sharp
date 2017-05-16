using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Атрибут маркировки типов, соответствующих табличным частям,
    /// для указания типа-владельца табличной части. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class OneSTablePartOwnerAttribute : Attribute
    {
        /// <summary>Конструктор.</summary>
        /// <param name="ownerType">Тип владельца табличной части.</param>
        public OneSTablePartOwnerAttribute(Type ownerType)
        {
            _ownerType = ownerType;
        }

        /// <summary>Тип владельца табличной части.</summary>
        public Type OwnerType
        {
            get { return _ownerType; }
        }
        private readonly Type _ownerType;
    }
}
