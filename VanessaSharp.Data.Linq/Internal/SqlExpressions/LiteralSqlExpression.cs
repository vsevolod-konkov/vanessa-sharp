using System;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    /// <summary>Литерал в SQL-выражении.</summary>
    internal sealed class LiteralSqlExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="literal">Литерал.</param>
        public LiteralSqlExpression(string literal)
        {
            _literal = literal;
        }

        /// <summary>Литерал.</summary>
        public string Literal
        {
            get { return _literal; }
        }
        private readonly string _literal;

        /// <summary>Построение SQL-запроса.</summary>
        /// <param name="sqlBuilder">Построитель SQL-выражений.</param>
        public override void BuildSql(ISqlBuilder sqlBuilder)
        {
            sqlBuilder.WriteLiteral(Literal);
        }
    }
}
