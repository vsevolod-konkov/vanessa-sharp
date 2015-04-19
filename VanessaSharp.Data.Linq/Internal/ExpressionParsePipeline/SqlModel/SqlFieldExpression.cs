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
        /// <param name="table">Выражение таблицы.</param>
        /// <param name="fieldName">Имя поля.</param>
        public SqlFieldExpression(SqlExpression table, string fieldName)
        {
            Contract.Requires<ArgumentNullException>(table != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

            _table = table;
            _fieldName = fieldName;
        }

        /// <summary>Выражение таблицы.</summary>
        public SqlExpression Table
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);
                
                return _table;
            }    
        }
        private readonly SqlExpression _table;

        /// <summary>Имя поля таблицы.</summary>
        public string FieldName
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return _fieldName;
            }
        }
        private readonly string _fieldName;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherField = other as SqlFieldExpression;

            return !ReferenceEquals(otherField, null)
                   && Table.Equals(otherField.Table)
                   && string.Equals(FieldName, otherField.FieldName, StringComparison.InvariantCultureIgnoreCase);
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
            return typeof(SqlFieldExpression).GetHashCode()
                   ^ Table.GetHashCode()
                   ^ FieldName.ToUpperInvariant().GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            var subSqlBuilder = new StringBuilder();
            Table.BuildSql(subSqlBuilder);
            var tableSql = subSqlBuilder.ToString();
            
            if (!string.IsNullOrWhiteSpace(tableSql))
            {
                sqlBuilder.Append(tableSql);
                sqlBuilder.Append(".");
            }

            sqlBuilder.Append(FieldName);
        }
    }
}
