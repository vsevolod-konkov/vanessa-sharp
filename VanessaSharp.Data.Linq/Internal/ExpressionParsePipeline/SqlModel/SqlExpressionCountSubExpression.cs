using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Подвыражение выбирающее все колонки.
    /// </summary>
    internal sealed class SqlExpressionCountSubExpression : SqlCountSubExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="expression">Выражение колонки.</param>
        /// <param name="isDistinct">Считать только различные.</param>
        public SqlExpressionCountSubExpression(SqlExpression expression, bool isDistinct)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            _expression = expression;
            _isDistinct = isDistinct;
        }

        /// <summary>Выражение колонки.</summary>
        public SqlExpression Expression
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);
                
                return _expression;
            }
        }
        private readonly SqlExpression _expression;

        /// <summary>Считать только различные.</summary>
        public bool IsDistinct
        {
            get { return _isDistinct; }
        }
        private readonly bool _isDistinct;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            if (IsDistinct)
            {
                sqlBuilder.Append(SqlKeywords.DISTINCT);
                sqlBuilder.Append(" ");
            }

            Expression.BuildSql(sqlBuilder);
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public override bool Equals(SqlCountSubExpression other)
        {
            var otherExpression = other as SqlExpressionCountSubExpression;
            return otherExpression != null
                   && Expression.Equals(otherExpression.Expression)
                   && IsDistinct == otherExpression.IsDistinct;
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
            return typeof(SqlExpressionCountSubExpression).GetHashCode()
                   ^ Expression.GetHashCode()
                   ^ IsDistinct.GetHashCode();
        }
    }
}