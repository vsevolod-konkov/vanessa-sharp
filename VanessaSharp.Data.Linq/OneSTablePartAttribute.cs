using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Атрибут маркировки публичных полей и свойств для 
    /// указания соответсвия табличной части в источнике данных 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OneSTablePartAttribute : Attribute
    {
        /// <summary>Конструктор.</summary>
        /// <param name="tablePartName">Имя табличной части в источнике данных 1С.</param>
        public OneSTablePartAttribute(string tablePartName)
        {
            _tablePartName = tablePartName;
        }
        
        /// <summary>
        /// Имя табличной части в источнике данных 1С.
        /// </summary>
        public string TablePartName
        {
            get { return _tablePartName; }
        }
        private readonly string _tablePartName;
    }
}
