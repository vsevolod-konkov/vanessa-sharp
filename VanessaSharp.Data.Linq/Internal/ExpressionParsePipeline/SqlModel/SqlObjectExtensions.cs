using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Расширения для <see cref="ISqlObject"/>.</summary>
    internal static class SqlObjectExtensions
    {
        /// <summary>Добавление SQL-объекта в построитель строки SQL.</summary>
        /// <param name="sqlObject">Добавляемый SQL-объект.</param>
        /// <param name="sqlBuilder">Построитель SQL-строки запроса.</param>
        /// <param name="options">Опции построения SQL.</param>
        public static void AppendSqlTo(this ISqlObject sqlObject, StringBuilder sqlBuilder, SqlBuildOptions options = SqlBuildOptions.Default)
        {
            Contract.Requires<ArgumentNullException>(sqlObject != null);
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            var hasParenthesis = sqlObject.HasSpaces && (options != SqlBuildOptions.IgnoreSpaces);

            if (hasParenthesis)
                sqlBuilder.Append("( ");

            sqlObject.BuildSql(sqlBuilder);

            if (hasParenthesis)
                sqlBuilder.Append(" )");
        }
    }
}
