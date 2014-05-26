using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Бинарный предикат SQL.</summary>
    internal sealed class SqlBinaryRelationCondition : SqlCondition
    {
        /// <summary>Конструктор.</summary>
        /// <param name="relationType">Тип бинарного отношения.</param>
        /// <param name="firstOperand">Первый операнд.</param>
        /// <param name="secondOperand">Второй операнд.</param>
        public SqlBinaryRelationCondition(SqlBinaryRelationType relationType, SqlExpression firstOperand, SqlExpression secondOperand)
        {
            Contract.Requires<ArgumentOutOfRangeException>(Array.BinarySearch(Enum.GetValues(typeof(SqlBinaryRelationType)), relationType) >= 0);
            Contract.Requires<ArgumentNullException>(firstOperand != null);
            Contract.Requires<ArgumentNullException>(secondOperand != null);

            _relationType = relationType;
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
        
        /// <summary>Тип бинарного отношения.</summary>
        public SqlBinaryRelationType RelationType
        {
            get { return _relationType; }
        }
        private readonly SqlBinaryRelationType _relationType;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            FirstOperand.BuildSql(sqlBuilder);
            sqlBuilder.Append(" ");
            sqlBuilder.Append(GetSqlSymbolForRelation(RelationType));
            sqlBuilder.Append(" ");
            SecondOperand.BuildSql(sqlBuilder);
        }

        /// <summary>Получение SQL-символа для бинарного отношения.</summary>
        /// <param name="relationType">Тип бинарного отношения.</param>
        private static string GetSqlSymbolForRelation(SqlBinaryRelationType relationType)
        {
            switch (relationType)
            {
                case SqlBinaryRelationType.Equal:
                    return "=";
                case SqlBinaryRelationType.NotEqual:
                    return "<>";
                case SqlBinaryRelationType.Greater:
                    return ">";
                case SqlBinaryRelationType.GreaterOrEqual:
                    return ">=";
                case SqlBinaryRelationType.Less:
                    return "<";
                case SqlBinaryRelationType.LessOrEqual:
                    return "<=";
                default:
                    throw new ArgumentOutOfRangeException("relationType");
            }
        }
    }
}
