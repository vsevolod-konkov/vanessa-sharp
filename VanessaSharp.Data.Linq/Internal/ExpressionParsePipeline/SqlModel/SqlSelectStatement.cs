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
        /// <param name="isDistinct">Выборка различных записей.</param>
        public SqlSelectStatement(SqlColumnSetExpression columns, bool isDistinct)
        {
            Contract.Requires<ArgumentNullException>(columns != null);
            
            _columns = columns;
            _isDistinct = isDistinct;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_columns != null);
        }

        /// <summary>Выборка различных записей.</summary>
        public bool IsDistinct
        {
            get { return _isDistinct; }
        }
        private readonly bool _isDistinct;

        /// <summary>Коллекция колонок.</summary>
        public SqlColumnSetExpression Columns
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlColumnSetExpression>() != null);
                
                return _columns;
            }
        }
        private readonly SqlColumnSetExpression _columns;

        /// <summary>Генерация SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentException>(sqlBuilder != null);

            sqlBuilder.Append("SELECT ");

            if (IsDistinct)
                sqlBuilder.Append("DISTINCT ");

            Columns.BuildSql(sqlBuilder);
        }
    }
}
