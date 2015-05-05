using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Выражение вызова агрегируемой функции.</summary>
    internal sealed class SqlAggregateFunctionExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="function">Вызываемая агрегируемая функция.</param>
        /// <param name="argument">Выражение аргумента функции.</param>
        public SqlAggregateFunctionExpression(
            SqlAggregateFunction function, SqlExpression argument)
        {
            Contract.Requires<ArgumentNullException>(argument != null);
            
            _function = function;
            _argument = argument;
        }

        /// <summary>
        /// Вызываемая агрегируемая функция.
        /// </summary>
        public SqlAggregateFunction Function
        {
            get { return _function; }
        }
        private readonly SqlAggregateFunction _function;

        /// <summary>
        /// Выражение аргумента функции.
        /// </summary>
        public SqlExpression Argument
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _argument;
            }
        }
        private readonly SqlExpression _argument;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherFunction = other as SqlAggregateFunctionExpression;

            return (otherFunction != null)
                   && (Function == otherFunction.Function)
                   && Argument.Equals(otherFunction.Argument);
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
            return typeof (SqlAggregateFunctionExpression).GetHashCode()
                   ^ Function.GetHashCode()
                   ^ Argument.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(GetAggregateFunctionSql(Function));
            sqlBuilder.Append("(");

            Argument.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);

            sqlBuilder.Append(")");
        }

        private static string GetAggregateFunctionSql(SqlAggregateFunction function)
        {
            return Enum.GetName(typeof(SqlAggregateFunction), function).ToUpperInvariant();
        }
    }
}
