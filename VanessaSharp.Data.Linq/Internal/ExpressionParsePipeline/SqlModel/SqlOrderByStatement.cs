using System.Collections.ObjectModel;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Инструкция сортировки данных.</summary>
    internal sealed class SqlOrderByStatement
    {
        /// <summary>Конструктор.</summary>
        /// <param name="sortFields">Список полей для сортировки.</param>
        public SqlOrderByStatement(ReadOnlyCollection<SqlSortFieldExpression> sortFields)
        {
            _sortFields = sortFields;
        }

        /// <summary>Список полей для сортировки.</summary>
        public ReadOnlyCollection<SqlSortFieldExpression> SortFields
        {
            get { return _sortFields; }
        }
        private readonly ReadOnlyCollection<SqlSortFieldExpression> _sortFields;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("ORDER BY ");

            var separator = string.Empty;
            foreach (var sortField in SortFields)
            {
                sqlBuilder.Append(separator);
                sortField.BuildSql(sqlBuilder);

                separator = ", ";
            }
        }
    }
}
