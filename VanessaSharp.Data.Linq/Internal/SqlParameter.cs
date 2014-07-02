using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Параметр для SQL-запроса.</summary>
    /// <remarks>Немутабельная структура.</remarks>
    internal sealed class SqlParameter : IEquatable<SqlParameter>
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

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SqlParameter other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            return (Name == other.Name && Equals(Value, other.Value));
        }

        /// <summary>
        /// Определяет, равен ли заданный объект <see cref="T:System.Object"/> текущему объекту <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Значение true, если заданный объект <see cref="T:System.Object"/> равен текущему объекту <see cref="T:System.Object"/>; в противном случае — значение false.
        /// </returns>
        /// <param name="obj">Элемент <see cref="T:System.Object"/>, который требуется сравнить с текущим элементом <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as SqlParameter);
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) 
                    ^ (_value != null ? _value.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format(
                "SQL Parameter Name:{0}; Value:{1}", Name, Value);
        }
    }
}
