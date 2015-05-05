using System;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>SQL-предикат.</summary>
    internal abstract class SqlCondition : IEquatable<SqlCondition>, ISqlObject
    {
        /// <summary>Генерация кода SQL-запроса.</summary>
        protected abstract void BuildSql(StringBuilder sqlBuilder);

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SqlCondition other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            return OverrideEquals(other);
        }

        /// <summary>
        /// Определяет, равен ли заданный объект <see cref="T:System.Object"/> текущему объекту <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Значение true, если заданный объект <see cref="T:System.Object"/> равен текущему объекту <see cref="T:System.Object"/>; в противном случае — значение false.
        /// </returns>
        /// <param name="obj">Элемент <see cref="T:System.Object"/>, который требуется сравнить с текущим элементом <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (ReferenceEquals(obj, null))
                return false;
            
            return OverrideEquals(obj as SqlCondition);
        }

        /// <summary>
        /// Проверка эквивалентности в дочернем классе.
        /// </summary>
        protected abstract bool OverrideEquals(SqlCondition other);

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract override int GetHashCode();

        /// <summary>Построение SQL-строки.</summary>
        void ISqlObject.BuildSql(StringBuilder sqlBuilder)
        {
            BuildSql(sqlBuilder);
        }

        /// <summary>Имеются ли пробелы в SQL.</summary>
        bool ISqlObject.HasSpaces
        {
            get { return true; }
        }
    }
}
