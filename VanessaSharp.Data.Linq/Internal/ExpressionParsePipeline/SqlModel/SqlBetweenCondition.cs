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
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Operand.BuildSql(sqlBuilder);
            
            if (!IsBetween)
            {
                sqlBuilder.Append(" ");
                sqlBuilder.Append(SqlKeywords.NOT);
            }

            sqlBuilder.Append(" BETWEEN ");

            Start.BuildSql(sqlBuilder);

            sqlBuilder.Append(" ");
            sqlBuilder.Append(SqlKeywords.AND);
            sqlBuilder.Append(" ");

            End.BuildSql(sqlBuilder);
        }
    }
}
