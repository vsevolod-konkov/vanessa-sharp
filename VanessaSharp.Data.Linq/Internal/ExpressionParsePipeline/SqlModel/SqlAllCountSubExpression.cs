using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Подвыражение выбирающее все колонки.
    /// </summary>
    internal sealed class SqlAllCountSubExpression : SqlCountSubExpression
    {
        private static readonly SqlAllCountSubExpression _instance
            = new SqlAllCountSubExpression(SqlAllColumnsExpression.Instance);
        
        /// <summary>Единственный экземпляр.</summary>
        public static SqlAllCountSubExpression Instance
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlAllCountSubExpression>() != null);

                return _instance;
            }
        }

        private SqlAllCountSubExpression(SqlAllColumnsExpression allColumns)
        {
            Contract.Requires<ArgumentNullException>(allColumns != null);
            
            _allColumns = allColumns;
        }

        public SqlAllColumnsExpression AllColumns
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlAllColumnsExpression>() != null);
                
                return _allColumns;
            }
        }
        private readonly SqlAllColumnsExpression _allColumns;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            AllColumns.BuildSql(sqlBuilder);
        }

        public override bool Equals(SqlCountSubExpression other)
        {
            return other is SqlAllCountSubExpression;
        }

        public override int GetHashCode()
        {
            return typeof(SqlAllCountSubExpression).GetHashCode();
        }
    }
}