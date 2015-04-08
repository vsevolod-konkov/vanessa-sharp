using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Условие проверки ссылки.
    /// </summary>
    internal sealed class SqlRefsCondition : SqlCondition
    {
        /// <summary>Конструктор.</summary>
        /// <param name="operand">Операнд.</param>
        /// <param name="dataSourceName">Имя источника данных.</param>
        public SqlRefsCondition(SqlExpression operand, string dataSourceName)
        {
            Contract.Requires<ArgumentNullException>(operand != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(dataSourceName));
            
            _operand = operand;
            _dataSourceName = dataSourceName;
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

        /// <summary>Имя источника данных.</summary>
        public string DataSourceName
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                
                return _dataSourceName;
            }
        }
        private readonly string _dataSourceName;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Operand.BuildSql(sqlBuilder);
            sqlBuilder.Append(" REFS ");
            sqlBuilder.Append(DataSourceName);
        }
    }
}
