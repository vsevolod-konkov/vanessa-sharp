using System.Collections.ObjectModel;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    /// <summary>Выражение в части Select.</summary>
    internal sealed class SelectSqlExpression : SqlExpression
    {
        public SelectSqlExpression(ReadOnlyCollection<SqlExpression> columns)
        {
            _columns = columns;
        }

        /// <summary>Колонки.</summary>
        public ReadOnlyCollection<SqlExpression> Columns
        {
            get { return _columns; }
        }
        private readonly ReadOnlyCollection<SqlExpression> _columns;

        /// <summary>Построение SQL-запроса.</summary>
        /// <param name="sqlBuilder">Построитель SQL-выражений.</param>
        public override void BuildSql(ISqlBuilder sqlBuilder)
        {
            sqlBuilder.WriteBeginSelect();
            foreach (var column in Columns)
            {
                sqlBuilder.WriteBeginColumn();
                column.BuildSql(sqlBuilder);
                sqlBuilder.WriteEndColumn();
            }
            sqlBuilder.WriteEndSelect();
        }
    }
}
