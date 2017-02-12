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
        /// <param name="kind">Вид колонки источника данных 1С.</param>
        public OneSDataColumnAttribute(string columnName, OneSDataColumnKind kind = OneSDataColumnKind.Default)
        {
            _columnName = columnName;
            _kind = kind;
        }

        /// <summary>Имя колонки источника данных 1С.</summary>
        public string ColumnName
        {
            get { return _columnName; }
        }
        private readonly string _columnName;

        /// <summary>Вид колонки источника данных 1С.</summary>
        public OneSDataColumnKind Kind
        {
            get { return _kind; }
        }
        private readonly OneSDataColumnKind _kind;
    }
}
