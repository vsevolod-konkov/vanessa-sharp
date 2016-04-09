using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение группы полей.
    /// </summary>
    internal sealed class SqlFieldsGroupExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="table">Выражение таблицы.</param>
        /// <param name="fields">Список имен полей в группе.</param>
        public SqlFieldsGroupExpression(SqlExpression table, ICollection<string> fields)
        {
            Contract.Requires<ArgumentNullException>(table != null);
            Contract.Requires<ArgumentNullException>(fields != null && fields.Count > 0);
            
            _table = table;
            _fields = new ReadOnlyCollection<string>(fields.ToArray());
        }

        /// <summary>Выражение таблицы.</summary>
        public SqlExpression Table
        {
            get { return _table; }
        }
        private readonly SqlExpression _table;

        /// <summary>Список имен полей в группе.</summary>
        public ReadOnlyCollection<string> Fields
        {
            get { return _fields; }
        }
        private readonly ReadOnlyCollection<string> _fields;

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherGroup = other as SqlFieldsGroupExpression;

            return !ReferenceEquals(otherGroup, null)
                   && Table.Equals(otherGroup.Table)
                   && Fields.Count == otherGroup.Fields.Count
                   && Enumerable.Range(0, Fields.Count).All(i => Fields[i] == otherGroup.Fields[i]);
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
            return typeof (SqlFieldsGroupExpression).GetHashCode()
                   ^ Table.GetHashCode()
                   ^ Fields.Aggregate(0, (c, f) => c ^ f.GetHashCode());
        }

        /// <summary>Построение SQL-строки.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            var subSqlBuilder = new StringBuilder();
            Table.AppendSqlTo(subSqlBuilder);
            var tableSql = subSqlBuilder.ToString();

            if (!string.IsNullOrWhiteSpace(tableSql))
            {
                sqlBuilder.Append(tableSql);
                sqlBuilder.Append(".");
            }

            if (Fields.Count == 1)
            {
                sqlBuilder.Append(Fields.Single());    
            }
            else
            {
                sqlBuilder.Append("(");

                var separator = string.Empty;
                foreach (var field in Fields)
                {
                    sqlBuilder.Append(separator);
                    sqlBuilder.Append(field);

                    separator = ", ";
                }

                sqlBuilder.Append(")");
            }
        }
    }
}
