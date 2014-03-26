using System.IO;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    /// <summary>
    /// Базовый класс SQL-выражений.
    /// </summary>
    internal abstract class SqlExpression
    {
        /// <summary>Построение SQL-запроса.</summary>
        /// <param name="sqlBuilder">Построитель SQL-выражений.</param>
        public abstract void BuildSql(ISqlBuilder sqlBuilder);
    }
}
