using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Инструкция Выборки данных из источнка в SQL-запросе.</summary>
    internal sealed class SqlSelectStatement
    {
        /// <summary>Конструктор.</summary>
        /// <param name="columns">Список выбыираемых колонок.</param>
        public SqlSelectStatement(SqlColumnSetExpression columns)
        {
            Contract.Requires<ArgumentNullException>(columns != null);
            
            _columns = columns;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_columns != null);
        }

        /// <summary>Коллекция колонок.</summary>
        public SqlColumnSetExpression Columns
        {
            get { return _columns; }
        }
        private readonly SqlColumnSetExpression _columns;

        /// <summary>Генерация SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentException>(sqlBuilder != null);

            sqlBuilder.Append("SELECT ");
            Columns.BuildSql(sqlBuilder);
        }
    }
}
