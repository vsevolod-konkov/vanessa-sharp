using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// SQL-выражение набора колонок запроса.
    /// </summary>
    internal abstract class SqlColumnSetExpression
    {
        /// <summary>Генерация SQL-кода.</summary>
        public abstract void BuildSql(StringBuilder sqlBuilder);
    }
}
