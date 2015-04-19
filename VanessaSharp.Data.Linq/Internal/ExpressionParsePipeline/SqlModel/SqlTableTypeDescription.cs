using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Описание табличного типа.</summary>
    internal sealed class SqlTableTypeDescription : SqlTypeDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="tableName">Имя таблицы.</param>
        public SqlTableTypeDescription(string tableName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(tableName));
            
            _tableName = tableName;
        }

        /// <summary>Вид SQL-типа.</summary>
        public override SqlTypeKind Kind
        {
            get { return SqlTypeKind.Table; }
        }

        /// <summary>Имя таблицы.</summary>
        public string TableName
        {
            get { return _tableName; }
        }
        private readonly string _tableName;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(TableName);
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public override bool Equals(SqlTypeDescription other)
        {
            var otherTable = other as SqlTableTypeDescription;

            return (otherTable != null)
                   && TableName == otherTable.TableName;
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
            return Kind.GetHashCode()
                   ^ TableName.GetHashCode();
        }
    }
}