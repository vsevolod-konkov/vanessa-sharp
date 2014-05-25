using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>SQL-предикат.</summary>
    internal abstract class SqlCondition
    {
        /// <summary>Генерация кода SQL-запроса.</summary>
        public abstract void BuildSql(StringBuilder sqlBuilder);
    }
}
