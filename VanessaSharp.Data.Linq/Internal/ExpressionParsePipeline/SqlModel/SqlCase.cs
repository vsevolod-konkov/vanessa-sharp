using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Подвыражение выбора значения в выражении операции выбора.
    /// </summary>
    internal sealed class SqlCase : IEquatable<SqlCase>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="condition">Условие при котором выбирается значение.</param>
        /// <param name="value">Выбираемое значение.</param>
        public SqlCase(SqlCondition condition, SqlExpression value)
        {
            Contract.Requires<ArgumentNullException>(condition != null);
            Contract.Requires<ArgumentNullException>(value != null);

            _condition = condition;
            _value = value;
        }

        /// <summary>
        /// Условие при котором выбирается значение.
        /// </summary>
        public SqlCondition Condition
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCondition>() != null);
                return _condition;
            }
        }
        private readonly SqlCondition _condition;

        /// <summary>Выбираемое значение.</summary>
        public SqlExpression Value
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);
                return _value;
            }
        }
        private readonly SqlExpression _value;

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SqlCase other)
        {
            return other != null
                   && Condition.Equals(other.Condition)
                   && Value.Equals(other.Value);
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
            return Equals(obj as SqlCase);
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
            return Condition.GetHashCode()
                ^ Value.GetHashCode();
        }
    }
}