using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Параметр для SQL-запроса.</summary>
    /// <remarks>Немутабельная структура.</remarks>
    internal sealed class SqlParameter : IEquatable<SqlParameter>
    {
        /// <summary>Пустая коллекция параметров.</summary>
        public static ReadOnlyCollection<SqlParameter> EmptyCollection
        {
            get { return _emptyCollection; }
        }
        private static readonly ReadOnlyCollection<SqlParameter> _emptyCollection =
            new ReadOnlyCollection<SqlParameter>(new SqlParameter[0]);

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
        
        public bool Equals(SqlParameter other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            return (Name == other.Name && Equals(Value, other.Value));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlParameter);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) 
                    ^ (_value != null ? _value.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "SQL Parameter Name:{0}; Value:{1}", Name, Value);
        }
    }
}
