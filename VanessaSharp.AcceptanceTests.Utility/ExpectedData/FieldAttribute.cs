using System;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Имя поля в источнике данных 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }

        /// <summary>Имя поля.</summary>
        public string FieldName
        {
            get { return _fieldName; }
        }
        private readonly string _fieldName;

        /// <summary>Тип поля.</summary>
        public Type FieldType { get; set; }
    }
}