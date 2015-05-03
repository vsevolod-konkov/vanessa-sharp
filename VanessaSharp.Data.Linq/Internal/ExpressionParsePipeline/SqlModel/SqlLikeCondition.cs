using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Условие проверки строки на соответствие.
    /// </summary>
    internal sealed class SqlLikeCondition : SqlCondition
    {
        public SqlLikeCondition(
            SqlExpression testedExpression, bool isLike, string pattern, char? escapeSymbol)
        {
            Contract.Requires<ArgumentNullException>(testedExpression != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(pattern));

            _testedExpression = testedExpression;
            _isLike = isLike;
            _pattern = SqlLiteralExpression.Create(pattern);
            _escapeSymbol = escapeSymbol.HasValue
                ? SqlLiteralExpression.Create(escapeSymbol.ToString())
                : null;
        }

        /// <summary>
        /// Тестируемое строковое выражение.
        /// </summary>
        public SqlExpression TestedExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _testedExpression;
            }
        }
        private readonly SqlExpression _testedExpression;

        /// <summary>
        /// Тестирование на соответствие.
        /// </summary>
        public bool IsLike
        {
            get { return _isLike; }
        }
        private readonly bool _isLike;

        /// <summary>
        /// Шаблон строки для тестирование на соответствие.
        /// </summary>
        public string Pattern
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return (string)_pattern.Value;
            }
        }
        private readonly SqlLiteralExpression _pattern;

        /// <summary>
        /// Нестандартный эскейп-символ в шаблоне.
        /// </summary>
        public char? EscapeSymbol
        {
            get
            {
                return _escapeSymbol == null
                    ? (char?)null
                    : ((string)_escapeSymbol.Value)[0];
            }
        }
        private readonly SqlLiteralExpression _escapeSymbol;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            _testedExpression.BuildSql(sqlBuilder);

            if (!IsLike)
            {
                sqlBuilder.Append(" ");
                sqlBuilder.Append(SqlKeywords.NOT);
            }

            sqlBuilder.Append(" LIKE ");
            _pattern.BuildSql(sqlBuilder);

            if (_escapeSymbol != null)
            {
                sqlBuilder.Append(" ESCAPE ");
                _escapeSymbol.BuildSql(sqlBuilder);
            }
        }
    }
}
