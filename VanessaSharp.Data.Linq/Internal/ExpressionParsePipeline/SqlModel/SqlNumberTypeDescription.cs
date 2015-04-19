using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Описание числового типа в SQL.</summary>
    internal sealed class SqlNumberTypeDescription : SqlTypeDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="length">Длина числа.</param>
        /// <param name="precision">Точность числа.</param>
        public SqlNumberTypeDescription(int? length, int? precision)
        {
            Contract.Requires<ArgumentException>(
                (precision.HasValue && length.HasValue) || !precision.HasValue);

            _length = length;
            _precision = precision;
        }

        /// <summary>Вид SQL-типа.</summary>
        public override SqlTypeKind Kind
        {
            get { return SqlTypeKind.Number; }
        }

        /// <summary>Длина числа.</summary>
        public int? Length
        {
            get { return _length; }
        }
        private readonly int? _length;

        /// <summary>Точность числа.</summary>
        public int? Precision
        {
            get { return _precision; }
        }
        private readonly int? _precision;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("NUMBER");
            if (Length.HasValue)
            {
                sqlBuilder.Append("(");
                sqlBuilder.Append(Length.Value);

                if (Precision.HasValue)
                {
                    sqlBuilder.Append(", ");
                    sqlBuilder.Append(Precision.Value);
                }

                sqlBuilder.Append(")");
            }
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
            var otherNumber = other as SqlNumberTypeDescription;

            return otherNumber != null
                   && Nullable.Equals(Length, otherNumber.Length)
                   && Nullable.Equals(Precision, otherNumber.Precision);
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
                   ^ Length.GetHashCode()
                   ^ Precision.GetHashCode();
        }
    }
}