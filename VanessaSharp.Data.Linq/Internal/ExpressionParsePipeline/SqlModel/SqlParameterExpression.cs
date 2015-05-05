using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>SQL-выражение параметра запроса.</summary>
    internal sealed class SqlParameterExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="parameterName">Имя параметра.</param>
        public SqlParameterExpression(string parameterName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(parameterName));
            
            _parameterName = parameterName;
        }

        /// <summary>Имя параметра.</summary>
        public string ParameterName
        {
            get { return _parameterName; }
        }
        private readonly string _parameterName;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherParameter = other as SqlParameterExpression;

            return !ReferenceEquals(otherParameter, null)
                   && string.Equals(ParameterName, otherParameter.ParameterName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return typeof(SqlParameterExpression).GetHashCode()
                   ^ ParameterName.ToUpperInvariant().GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            sqlBuilder.Append('&');
            sqlBuilder.Append(ParameterName);
        }
    }
}
