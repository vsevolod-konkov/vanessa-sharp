using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// SQL-Выражение указывающая на поле таблицы.
    /// </summary>
    internal sealed class SqlFieldExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="fieldName">Имя поля.</param>
        public SqlFieldExpression(string fieldName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

            _fieldName = fieldName;
        }

        /// <summary>Имя поля таблицы.</summary>
        public string FieldName
        {
            get { return _fieldName; }
        }
        private readonly string _fieldName;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherField = other as SqlFieldExpression;

            return !ReferenceEquals(otherField, null)
                   && string.Equals(FieldName, otherField.FieldName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return typeof (SqlFieldExpression).GetHashCode()
                   ^ _fieldName.ToUpperInvariant().GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            sqlBuilder.Append(FieldName);
        }
    }
}
