using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Описание SQL-типа.</summary>
    internal abstract class SqlTypeDescription : IEquatable<SqlTypeDescription>
    {
        /// <summary>Булев тип.</summary>
        public static SqlTypeDescription Boolean
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

                return _boolean;
            }
        }
        private static readonly SqlTypeDescription _boolean = new SqlSimpleTypeDescription(SqlTypeKind.Boolean, "BOOLEAN");

        /// <summary>Фабричный метод описания числового типа.</summary>
        /// <param name="length">Длина.</param>
        /// <param name="precision">Точность.</param>
        public static SqlTypeDescription Number(int? length = null, int? precision = null)
        {
            Contract.Requires<ArgumentException>(
                (precision.HasValue && length.HasValue) || !precision.HasValue);
            Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

            return new SqlNumberTypeDescription(length, precision);
        }

        /// <summary>Фабричный метод описания строкового типа.</summary>
        /// <param name="length">Длина.</param>
        public static SqlTypeDescription String(int? length = null)
        {
            Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

            return new SqlStringTypeDescription(length);
        }

        /// <summary>Тип даты.</summary>
        public static SqlTypeDescription Date
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

                return _date;
            }
        }
        private static readonly SqlTypeDescription _date = new SqlSimpleTypeDescription(SqlTypeKind.Date, "DATE");

        /// <summary>Фабричный метод описания табличного типа.</summary>
        /// <param name="tableName">Имя таблицы.</param>
        public static SqlTypeDescription Table(string tableName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(tableName));
            Contract.Ensures(Contract.Result<SqlTypeDescription>() != null);

            return new SqlTableTypeDescription(tableName);
        }

        /// <summary>Вид SQL-типа.</summary>
        public abstract SqlTypeKind Kind { get; }

        /// <summary>Генерация SQL-кода.</summary>
        public abstract void BuildSql(StringBuilder sqlBuilder);

        /// <summary>
        /// Определяет, равен ли заданный объект <see cref="T:System.Object"/> текущему объекту <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Значение true, если заданный объект <see cref="T:System.Object"/> равен текущему объекту <see cref="T:System.Object"/>; в противном случае — значение false.
        /// </returns>
        /// <param name="obj">Элемент <see cref="T:System.Object"/>, который требуется сравнить с текущим элементом <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as SqlTypeDescription);
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public abstract bool Equals(SqlTypeDescription other);

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract override int GetHashCode();

        #region Вспомогательный тип

        /// <summary>Реализация для непараметрических типов.</summary>
        private sealed class SqlSimpleTypeDescription : SqlTypeDescription
        {
            /// <summary>Имя типа в SQL.</summary>
            private readonly string _sqlTypeName;

            /// <summary>Конструктор.</summary>
            /// <param name="kind">Вид типа.</param>
            /// <param name="sqlTypeName">Имя типа в SQL.</param>
            public SqlSimpleTypeDescription(SqlTypeKind kind, string sqlTypeName)
            {
                _kind = kind;
                _sqlTypeName = sqlTypeName;
            }

            public override SqlTypeKind Kind
            {
                get { return _kind; }
            }
            private readonly SqlTypeKind _kind;
            
            public override void BuildSql(StringBuilder sqlBuilder)
            {
                sqlBuilder.Append(_sqlTypeName);
            }

            public override bool Equals(SqlTypeDescription other)
            {
                return (other != null)
                       && Kind == other.Kind;
            }

            public override int GetHashCode()
            {
                return Kind.GetHashCode();
            }
        }

        #endregion
    }
}
