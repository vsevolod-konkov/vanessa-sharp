using System;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>SQL-выражение значения.</summary>
    internal abstract class SqlExpression : IEquatable<SqlExpression>
    {
        public bool Equals(SqlExpression other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            return OverrideEquals(other);
        }

        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (ReferenceEquals(obj, null))
                return false;
            
            return OverrideEquals(obj as SqlExpression);
        }

        protected abstract bool OverrideEquals(SqlExpression other);

        public abstract override int GetHashCode();

        /// <summary>Генерация SQL-кода.</summary>
        public abstract void BuildSql(StringBuilder sqlBuilder);
    }
}
