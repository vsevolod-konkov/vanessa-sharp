using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Выражение поля по которому производится сортировка.</summary>
    internal sealed class SqlSortFieldExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="expression">Выражение по которому производится сортировка.</param>
        /// <param name="sortKind">Направление сортировки.</param>
        public SqlSortFieldExpression(SqlExpression expression, SortKind sortKind)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            
            _expression = expression;
            _sortKind = sortKind;
        }

        /// <summary>
        /// Выражение по которому производится сортировка.
        /// </summary>
        public SqlExpression Expression
        {
            get { return _expression; }
        }
        private readonly SqlExpression _expression;

        /// <summary>
        /// Направление сортировки.
        /// </summary>
        public SortKind SortKind
        {
            get { return _sortKind; }
        }
        private readonly SortKind _sortKind;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            Expression.BuildSql(sqlBuilder);
            if (SortKind == SortKind.Descending)
                sqlBuilder.Append(" DESC");
        }
    }
}