using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>SQL-выражение списка колонок.</summary>
    internal sealed class SqlColumnListExpression : SqlColumnSetExpression
    {
        public SqlColumnListExpression(IList<SqlExpression> columns)
        {
            Contract.Requires<ArgumentNullException>(columns != null);
            Contract.Requires<ArgumentException>(columns.Count > 0);

            _columns = new ReadOnlyCollection<SqlExpression>(columns);
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_columns != null && _columns.Count > 0);
        }
        
        /// <summary>Коллекция колонок.</summary>
        public ReadOnlyCollection<SqlExpression> Columns
        {
            get { return _columns; }
        }
        private readonly ReadOnlyCollection<SqlExpression> _columns;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            const string COLUMN_SEPARATOR = ", ";

            var separator = string.Empty;
            foreach (var column in Columns)
            {
                sqlBuilder.Append(separator);
                column.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);

                separator = COLUMN_SEPARATOR;
            }
        }
    }
}
