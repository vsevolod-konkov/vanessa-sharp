using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Выражение поля по которому производится сортировка.</summary>
    internal sealed class SqlSortFieldExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="field">Поле по которому производится сортировка.</param>
        /// <param name="sortKind">Направление сортировки.</param>
        public SqlSortFieldExpression(SqlFieldExpression field, SortKind sortKind)
        {
            Contract.Requires<ArgumentNullException>(field != null);
            
            _field = field;
            _sortKind = sortKind;
        }

        /// <summary>
        /// Поле по которому производится сортировка.
        /// </summary>
        public SqlFieldExpression Field
        {
            get { return _field; }
        }
        private readonly SqlFieldExpression _field;

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
            Field.BuildSql(sqlBuilder);
            if (SortKind == SortKind.Descending)
                sqlBuilder.Append(" DESC");
        }
    }
}