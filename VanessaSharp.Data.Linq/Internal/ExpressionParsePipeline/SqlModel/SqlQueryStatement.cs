using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Инструкция SQL-запроса.
    /// </summary>
    internal sealed class SqlQueryStatement
    {
        /// <summary>Конструктор.</summary>
        /// <param name="selectStatement">Инструкция выборки данных.</param>
        /// <param name="fromStatement">Инструкция описания источника данных.</param>
        /// <param name="whereStatement">Инструкция фильтрации данных.</param>
        /// <param name="orderByStatement">Инструкция сортировки данных.</param>
        public SqlQueryStatement(
            SqlSelectStatement selectStatement, SqlFromStatement fromStatement, SqlWhereStatement whereStatement, SqlOrderByStatement orderByStatement)
        {
            Contract.Requires<ArgumentNullException>(selectStatement != null);

            _selectStatement = selectStatement;
            _fromStatement = fromStatement;
            _whereStatement = whereStatement;
            _orderByStatement = orderByStatement;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_selectStatement != null);
        }

        /// <summary>Инструкция выборки данных.</summary>
        public SqlSelectStatement SelectStatement
        {
            get { return _selectStatement; }
        }
        private readonly SqlSelectStatement _selectStatement;

        /// <summary>Инструкция описания источника данных.</summary>
        public SqlFromStatement FromStatement
        {
            get { return _fromStatement; }
        }
        private readonly SqlFromStatement _fromStatement;

        /// <summary>Инструкция фильтрации данных.</summary>
        public SqlWhereStatement WhereStatement
        {
            get { return _whereStatement; }
        }
        private readonly SqlWhereStatement _whereStatement;
        
        /// <summary>Инструкция сортировки данных.</summary>
        public SqlOrderByStatement OrderByStatement
        {
            get { return _orderByStatement; }
        }
        private readonly SqlOrderByStatement _orderByStatement;

        /// <summary>Генерация SQL-запроса.</summary>
        public string BuildSql()
        {
            var sqlBuilder = new StringBuilder();

            SelectStatement.BuildSql(sqlBuilder);

            if (FromStatement != null)
            {
                sqlBuilder.Append(" ");
                FromStatement.BuildSql(sqlBuilder);
            }

            if (WhereStatement != null)
            {
                sqlBuilder.Append(" ");
                WhereStatement.BuildSql(sqlBuilder);
            }

            if (OrderByStatement != null)
            {
                sqlBuilder.Append(" ");
                OrderByStatement.BuildSql(sqlBuilder);
            }

            return sqlBuilder.ToString();
        }
    }
}
