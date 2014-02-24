using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.AcceptanceTests.Mocks
{
    /// <summary>Описание поля табличных данных.</summary>
    internal sealed class FieldDescription
    {
        public FieldDescription(string name, Type type)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentNullException>(type != null);

            _name = name;
            _type = type;
        }

        /// <summary>Имя поля.</summary>
        public string Name
        {
            get { return _name; }
        }
        private readonly string _name;

        /// <summary>Тип поля.</summary>
        public Type Type
        {
            get { return _type; }
        }
        private readonly Type _type;
    }
}