using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Описание поля табличных данных.</summary>
    public abstract class FieldDescription
    {
        /// <summary>Конструктор для скалярного поля.</summary>
        /// <param name="name">Имя поля.</param>
        protected FieldDescription(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));

            _name = name;
        }

        /// <summary>Вид поля.</summary>
        public abstract FieldKind Kind { get; }

        /// <summary>Имя поля.</summary>
        public string Name
        {
            get { return _name; }
        }
        private readonly string _name;

        /// <summary>Тип поля.</summary>
        public abstract Type Type { get; }

        /// <summary>Имя типа данных.</summary>
        public abstract string DataTypeName { get; }
    }
}