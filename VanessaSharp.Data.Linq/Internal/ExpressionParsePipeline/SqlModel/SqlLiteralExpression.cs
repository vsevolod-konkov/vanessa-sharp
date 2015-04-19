using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение литерала.
    /// </summary>
    internal sealed class SqlLiteralExpression : SqlExpression
    {
        public static SqlLiteralExpression Create(bool value)
        {
            return new SqlLiteralExpression(value, value ? "TRUE" : "FALSE");
        }

        public static SqlLiteralExpression Create(ulong value)
        {
            return new SqlLiteralExpression(value, value.ToString(CultureInfo.InvariantCulture));
        }

        public static SqlLiteralExpression Create(long value)
        {
            return new SqlLiteralExpression(value, value.ToString(CultureInfo.InvariantCulture));
        }

        public static SqlLiteralExpression Create(double value)
        {
            return new SqlLiteralExpression(value, value.ToString(CultureInfo.InvariantCulture));
        }

        public static SqlLiteralExpression Create(decimal value)
        {
            return new SqlLiteralExpression(value, value.ToString(CultureInfo.InvariantCulture));
        }

        public static SqlLiteralExpression Create(string value)
        {
            return new SqlLiteralExpression(value, "\"" + value + "\"");
        }

        public static SqlLiteralExpression Create(DateTime value)
        {
            return new SqlLiteralExpression(value,
                string.Format(CultureInfo.InvariantCulture, 
                "DATETIME({0}, {1}, {2}, {3}, {4}, {5})",
                          value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second));
        }

        /// <summary>
        /// SQL-выражение литерала.
        /// </summary>
        private readonly string _literal;

        /// <summary>
        /// Значение литерала.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }
        private readonly object _value;

        private SqlLiteralExpression(object value, string literal)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(literal));
            
            _value = value;
            _literal = literal;
        }

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherLiteral = other as SqlLiteralExpression;

            return (otherLiteral != null)
                   && (Equals(Value, otherLiteral.Value));
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return typeof (SqlLiteralExpression).GetHashCode()
                   ^ (Value == null ? 0 : Value.GetHashCode());
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(_literal);
        }
    }
}
