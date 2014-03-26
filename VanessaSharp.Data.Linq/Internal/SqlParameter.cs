using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Параметр для SQL-запроса.</summary>
    /// <remarks>Немутабельная структура.</remarks>
    internal sealed class SqlParameter
    {
        /// <summary>Конструктор.</summary>
        /// <param name="name">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        public SqlParameter(string name, object value)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            _name = name;
            _value = value;
        }

        /// <summary>Имя параметра.</summary>
        public string Name
        {
            get { return _name; }
        }
        private readonly string _name;

        /// <summary>Значение параметра.</summary>
        public object Value
        {
            get { return _value; }
        }
        private readonly object _value;
    }
}
