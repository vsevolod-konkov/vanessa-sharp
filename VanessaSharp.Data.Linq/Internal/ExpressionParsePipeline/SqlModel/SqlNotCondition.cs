using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Унарная логическая операция отрицания.
    /// </summary>
    internal sealed class SqlNotCondition : SqlCondition
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="condition">
        /// Отрицаемое условие.
        /// </param>
        public SqlNotCondition(SqlCondition condition)
        {
            Contract.Requires<ArgumentNullException>(condition != null);

            _condition = condition;
        }

        /// <summary>Отрицаемое условие.</summary>
        public SqlCondition Condition
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCondition>() != null);

                return _condition;
            }
        }
        private readonly SqlCondition _condition;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("NOT ( ");
            Condition.BuildSql(sqlBuilder);
            sqlBuilder.Append(" )");
        }
    }
}
