using System;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Описание строкового типа в SQL.</summary>
    internal sealed class SqlStringTypeDescription : SqlTypeDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="length">Длина строки.</param>
        public SqlStringTypeDescription(int? length)
        {
            _length = length;
        }

        /// <summary>Вид SQL-типа.</summary>
        public override SqlTypeKind Kind
        {
            get { return SqlTypeKind.String; }
        }

        /// <summary>Длина строки.</summary>
        public int? Length
        {
            get { return _length; }
        }
        private readonly int? _length;

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("STRING");
            if (Length.HasValue)
            {
                sqlBuilder.Append("(");
                sqlBuilder.Append(Length.Value);
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
            var otherString = other as SqlStringTypeDescription;

            return (otherString != null)
                   && Nullable.Equals(Length, otherString.Length);
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
                   ^ Length.GetHashCode();
        }
    }
}