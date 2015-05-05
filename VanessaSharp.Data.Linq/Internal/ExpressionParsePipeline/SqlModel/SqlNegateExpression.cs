using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение арифметического отрицания.
    /// </summary>
    internal sealed class SqlNegateExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="operand">Отрицаемый операнд.</param>
        public SqlNegateExpression(SqlExpression operand)
        {
            Contract.Requires<ArgumentNullException>(operand != null);

            _operand = operand;
        }

        /// <summary>Отрицаемый операнд.</summary>
        public SqlExpression Operand
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _operand;
            }
        }
        private readonly SqlExpression _operand;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherNegative = other as SqlNegateExpression;

            return (otherNegative != null)
                   && Operand.Equals(otherNegative.Operand);
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
            return GetType().GetHashCode()
                   ^ Operand.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("-");
            Operand.AppendSqlTo(sqlBuilder);
        }

        /// <summary>Имеются ли пробелы в SQL.</summary>
        protected override bool HasSpaces
        {
            get { return true; }
        }
    }
}
