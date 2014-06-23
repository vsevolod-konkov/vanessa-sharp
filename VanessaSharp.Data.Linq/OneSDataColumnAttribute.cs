using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Атрибут маркировки публичных полей и свойств для 
    /// указания соответсвия полю в источнике данных 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OneSDataColumnAttribute : Attribute
    {
        /// <summary>Конструктор принимающий имя колонки источника данных 1С.</summary>
        /// <param name="columnName">Имя колонки источника данных 1С.</param>
        public OneSDataColumnAttribute(string columnName)
        {
            _columnName = columnName;
        }

        /// <summary>Имя колонки источника данных 1С.</summary>
        public string ColumnName
        {
            get { return _columnName; }
        }
        private readonly string _columnName;
    }
}
