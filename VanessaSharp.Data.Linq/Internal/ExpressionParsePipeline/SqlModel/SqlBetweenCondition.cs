using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Условие вхождения значения в диапазон.
    /// </summary>
    internal sealed class SqlBetweenCondition : SqlCondition
    {
        /// <summary>Конструктор.</summary>
        /// <param name="operand">Тестируемый операнд.</param>
        /// <param name="isBetween">Тестирование на соответствие диапазону или нет.</param>
        /// <param name="start">Начало диапазона.</param>
        /// <param name="end">Конец диапазона.</param>
        public SqlBetweenCondition(SqlExpression operand, bool isBetween, SqlExpression start, SqlExpression end)
        {
            Contract.Requires<ArgumentNullException>(operand != null);
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);

            _operand = operand;
            _isBetween = isBetween;
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Тестируемый операнд.
        /// </summary>
        public SqlExpression Operand
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);
                
                return _operand;
            }
        }
        private readonly SqlExpression _operand;

        /// <summary>
        /// Тестирование на соответствие диапазону или нет.
        /// </summary>
        public bool IsBetween
        {
            get { return _isBetween; }
        }
        private readonly bool _isBetween;

        /// <summary>
        /// Начало диапазона.
        /// </summary>
        public SqlExpression Start
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _start;
            }
        }
        private readonly SqlExpression _start;

        /// <summary>
        /// Конец диапазона.
        /// </summary>
        public SqlExpression End
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _end;
            }
        }
        private readonly SqlExpression _end;

        /// <summary>Генерация кода SQL-запроса.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            Operand.AppendSqlTo(sqlBuilder);
            
            if (!IsBetween)
            {
                sqlBuilder.Append(" ");
                sqlBuilder.Append(SqlKeywords.NOT);
            }

            sqlBuilder.Append(" BETWEEN ");

            Start.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);

            sqlBuilder.Append(" ");
            sqlBuilder.Append(SqlKeywords.AND);
            sqlBuilder.Append(" ");

            End.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);
        }

        /// <summary>
        /// Проверка эквивалентности в дочернем классе.
        /// </summary>
        protected override bool OverrideEquals(SqlCondition other)
        {
            var otherBetween = other as SqlBetweenCondition;

            return otherBetween != null
                   && Operand.Equals(otherBetween.Operand)
                   && Start.Equals(otherBetween.Start)
                   && End.Equals(otherBetween.End);
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
            return typeof(SqlBetweenCondition).GetHashCode()
                   ^ Operand.GetHashCode()
                   ^ Start.GetHashCode()
                   ^ End.GetHashCode();
        }
    }
}
