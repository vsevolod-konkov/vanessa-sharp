﻿using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Логическая бинарная операция.
    /// </summary>
    internal sealed class SqlBinaryOperationCondition : SqlCondition
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="operationType">Тип бинарной логической операции.</param>
        /// <param name="firstOperand">Первый операнд.</param>
        /// <param name="secondOperand">Второй операнд.</param>
        public SqlBinaryOperationCondition(
            SqlBinaryOperationType operationType, SqlCondition firstOperand, SqlCondition secondOperand)
        {
            Contract.Requires<ArgumentNullException>(firstOperand != null);
            Contract.Requires<ArgumentNullException>(secondOperand != null);
            
            _operationType = operationType;
            _firstOperand = firstOperand;
            _secondOperand = secondOperand;
        }

        /// <summary>
        /// Тип бинарной логической операции.
        /// </summary>
        public SqlBinaryOperationType OperationType
        {
            get { return _operationType; }
        }
        private readonly SqlBinaryOperationType _operationType;

        /// <summary>
        /// Первый операнд.
        /// </summary>
        public SqlCondition FirstOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCondition>() != null);

                return _firstOperand;
            }
        }
        private readonly SqlCondition _firstOperand;

        /// <summary>
        /// Второй операнд.
        /// </summary>
        public SqlCondition SecondOperand
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCondition>() != null);

                return _secondOperand;
            }
        }
        private readonly SqlCondition _secondOperand;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            BuildOperandSql(FirstOperand, sqlBuilder);
            sqlBuilder.Append(GetSqlSymbolForOperation(OperationType));
            BuildOperandSql(SecondOperand, sqlBuilder);
        }

        /// <summary>
        /// Генерация SQL-кода для операнда операции.
        /// </summary>
        private static void BuildOperandSql(SqlCondition operand, StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(" ( ");
            operand.BuildSql(sqlBuilder);
            sqlBuilder.Append(" ) ");
        }

        /// <summary>Получение SQL-символа для бинарной операции.</summary>
        /// <param name="operationType">Тип бинарной операции.</param>
        private static string GetSqlSymbolForOperation(SqlBinaryOperationType operationType)
        {
            switch (operationType)
            {
                case SqlBinaryOperationType.And:
                    return "AND";
                case SqlBinaryOperationType.Or:
                    return "OR";
                default:
                    throw new ArgumentOutOfRangeException("operationType");
            }
        }
    }
}