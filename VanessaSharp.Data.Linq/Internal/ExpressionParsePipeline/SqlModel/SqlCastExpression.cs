using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение приведения операнда к нужному типу.
    /// </summary>
    internal sealed class SqlCastExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="operand">Операнд.</param>
        /// <param name="sqlType">Описание типа, к которому производится приведение.</param>
        public SqlCastExpression(SqlExpression operand, SqlTypeDescription sqlType)
        {
            Contract.Requires<ArgumentNullException>(operand != null);
            Contract.Requires<ArgumentNullException>(sqlType != null);
            
            _operand = operand;
            _sqlType = sqlType;
        }

        /// <summary>Операнд.</summary>
        public SqlExpression Operand
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _operand;
            }
        }
        private readonly SqlExpression _operand;

        /// <summary>
        /// Описание типа, к которому производится приведение.
        /// </summary>
        public SqlTypeDescription SqlType
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

                return _sqlType;
            }
        }
        private readonly SqlTypeDescription _sqlType;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherCast = other as SqlCastExpression;

            return (otherCast != null)
                   && Operand.Equals(otherCast.Operand)
                   && SqlType.Equals(otherCast.SqlType);
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
            return typeof (SqlCastExpression).GetHashCode()
                   ^ Operand.GetHashCode()
                   ^ SqlType.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("CAST( ");
            
            Operand.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);

            sqlBuilder.Append(" AS ");

            SqlType.BuildSql(sqlBuilder);

            sqlBuilder.Append(" )");
        }
    }
}
