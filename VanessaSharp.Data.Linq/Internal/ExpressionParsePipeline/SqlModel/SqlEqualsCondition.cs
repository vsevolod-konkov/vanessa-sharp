using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Предикат равенства в SQL.</summary>
    internal sealed class SqlEqualsCondition : SqlCondition
    {
        /// <summary>Конструктор.</summary>
        /// <param name="firstOperand">Первый операнд.</param>
        /// <param name="secondOperand">Второй операнд.</param>
        public SqlEqualsCondition(SqlExpression firstOperand, SqlExpression secondOperand)
        {
            Contract.Requires<ArgumentNullException>(firstOperand != null);
            Contract.Requires<ArgumentNullException>(secondOperand != null);
            
            _firstOperand = firstOperand;
            _secondOperand = secondOperand;
        }

        /// <summary>Первый операнд.</summary>
        public SqlExpression FirstOperand
        {
            get { return _firstOperand; }
        }
        private readonly SqlExpression _firstOperand;

        /// <summary>Второй операнд.</summary>
        public SqlExpression SecondOperand
        {
            get { return _secondOperand; }
        }
        private readonly SqlExpression _secondOperand;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            FirstOperand.BuildSql(sqlBuilder);
            sqlBuilder.Append(" = ");
            SecondOperand.BuildSql(sqlBuilder);
        }
    }
}
