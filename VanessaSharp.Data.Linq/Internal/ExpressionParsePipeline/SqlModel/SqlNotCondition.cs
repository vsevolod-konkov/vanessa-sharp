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

        /// <summary>
        /// Проверка эквивалентности в дочернем классе.
        /// </summary>
        protected override bool OverrideEquals(SqlCondition other)
        {
            var otherNot = other as SqlNotCondition;

            return otherNot != null
                   && Condition.Equals(otherNot.Condition);
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
            return typeof(SqlNotCondition).GetHashCode()
                   ^ Condition.GetHashCode();
        }
    }
}
