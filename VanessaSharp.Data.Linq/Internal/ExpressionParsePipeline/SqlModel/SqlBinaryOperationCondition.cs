using System;
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
            SqlBinaryLogicOperationType operationType, SqlCondition firstOperand, SqlCondition secondOperand)
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
        public SqlBinaryLogicOperationType OperationType
        {
            get { return _operationType; }
        }
        private readonly SqlBinaryLogicOperationType _operationType;

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
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            FirstOperand.AppendSqlTo(sqlBuilder);

            sqlBuilder.Append(" ");
            sqlBuilder.Append(GetSqlSymbolForOperation(OperationType));
            sqlBuilder.Append(" ");

            SecondOperand.AppendSqlTo(sqlBuilder);
        }

        /// <summary>
        /// Проверка эквивалентности в дочернем классе.
        /// </summary>
        protected override bool OverrideEquals(SqlCondition other)
        {
            var otherBinary = other as SqlBinaryOperationCondition;

            return otherBinary != null
                   && OperationType == otherBinary.OperationType
                   && FirstOperand.Equals(otherBinary.FirstOperand)
                   && SecondOperand.Equals(otherBinary.SecondOperand);
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
            return typeof(SqlBinaryOperationCondition).GetHashCode()
                   ^ OperationType.GetHashCode()
                   ^ FirstOperand.GetHashCode()
                   ^ SecondOperand.GetHashCode();
        }

        /// <summary>Получение SQL-символа для бинарной операции.</summary>
        /// <param name="operationType">Тип бинарной операции.</param>
        private static string GetSqlSymbolForOperation(SqlBinaryLogicOperationType operationType)
        {
            switch (operationType)
            {
                case SqlBinaryLogicOperationType.And:
                    return SqlKeywords.AND;
                case SqlBinaryLogicOperationType.Or:
                    return "OR";
                default:
                    throw new ArgumentOutOfRangeException("operationType");
            }
        }
    }
}
