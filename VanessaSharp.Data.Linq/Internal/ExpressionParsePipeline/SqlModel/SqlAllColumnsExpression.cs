using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// SQL-выражение множества колонок обозначающее все колонки источника.
    /// </summary>
    internal sealed class SqlAllColumnsExpression : SqlColumnSetExpression
    {
        private SqlAllColumnsExpression() {}

        /// <summary>Единственный экземпляр.</summary>
        public static SqlAllColumnsExpression Instance
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlAllColumnsExpression>() != null);
                
                return _instance;
            }
        }
        private readonly static SqlAllColumnsExpression _instance = new SqlAllColumnsExpression();

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            sqlBuilder.Append("*");
        }
    }
}
