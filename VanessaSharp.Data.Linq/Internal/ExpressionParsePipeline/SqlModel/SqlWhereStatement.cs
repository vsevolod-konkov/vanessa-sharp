using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Инструкция фильтрации данных SQL-запроса.
    /// </summary>
    internal sealed class SqlWhereStatement
    {
        /// <summary>Конструктор.</summary>
        /// <param name="condition">Предикат для фильтра.</param>
        public SqlWhereStatement(SqlCondition condition)
        {
            Contract.Requires<ArgumentNullException>(condition != null);

            _condition = condition;
        }

        /// <summary>Условие фильтрации.</summary>
        public SqlCondition Condition
        {
            get { return _condition; }
        }
        private readonly SqlCondition _condition;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            sqlBuilder.Append("WHERE ");
            Condition.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);
        }
    }
}