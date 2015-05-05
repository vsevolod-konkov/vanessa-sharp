using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Условие определения выражения как NULL или нет.
    /// </summary>
    internal sealed class SqlIsNullCondition : SqlCondition
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="expression">Выражение, чье значение проверяется на NULL.</param>
        /// <param name="isNull">Проверка на NULL. Подробнее см. <see cref="IsNull"/>.</param>
        public SqlIsNullCondition(SqlExpression expression, bool isNull)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            
            _expression = expression;
            _isNull = isNull;
        }

        /// <summary>
        /// Выражение, чье значение проверяется на NULL.
        /// </summary>
        public SqlExpression Expression
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);
                
                return _expression;
            }
        }
        private readonly SqlExpression _expression;

        /// <summary>
        /// Проверка на NULL.
        /// </summary>
        /// <remarks>
        /// Если <c>true</c>, то проверяется на NULL.
        /// Если <c>false</c>, то проверяется, что не NULL.
        /// </remarks>
        public bool IsNull
        {
            get { return _isNull; }
        }
        private readonly bool _isNull;

        /// <summary>Генерация кода SQL-запроса.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            Expression.AppendSqlTo(sqlBuilder);

            if (!IsNull)
            {
                sqlBuilder.Append(" ");
                sqlBuilder.Append(SqlKeywords.NOT);
            }

            sqlBuilder.Append(" IS NULL");
        }

        /// <summary>
        /// Проверка эквивалентности в дочернем классе.
        /// </summary>
        protected override bool OverrideEquals(SqlCondition other)
        {
            var otherIsNull = other as SqlIsNullCondition;

            return otherIsNull != null
                   && Expression.Equals(otherIsNull.Expression)
                   && IsNull == otherIsNull.IsNull;
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
            return typeof (SqlIsNullCondition).GetHashCode()
                   ^ Expression.GetHashCode()
                   ^ IsNull.GetHashCode();
        }
    }
}
