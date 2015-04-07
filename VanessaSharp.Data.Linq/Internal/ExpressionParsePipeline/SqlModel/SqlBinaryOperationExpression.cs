using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Бинарная арифметическая операция.
    /// </summary>
    internal sealed class SqlBinaryOperationExpression : SqlExpression
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="operationType">Тип арифметической операции.</param>
        /// <param name="left">Левый операнд.</param>
        /// <param name="right">Правый операнд.</param>
        public SqlBinaryOperationExpression(
            SqlBinaryArithmeticOperationType operationType, SqlExpression left, SqlExpression right)
        {
            Contract.Requires<ArgumentNullException>(left != null);
            Contract.Requires<ArgumentNullException>(right != null);
            
            _operationType = operationType;
            _left = left;
            _right = right;
        }

        /// <summary>
        /// Тип арифметической операции.
        /// </summary>
        public SqlBinaryArithmeticOperationType OperationType
        {
            get { return _operationType; }
        }
        private readonly SqlBinaryArithmeticOperationType _operationType;

        /// <summary>
        /// Левый операнд.
        /// </summary>
        public SqlExpression Left
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _left;
            }
        }
        private readonly SqlExpression _left;

        /// <summary>
        /// Правый операнд.
        /// </summary>
        public SqlExpression Right
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _right;
            }
        }
        private readonly SqlExpression _right;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherOperation = other as SqlBinaryOperationExpression;

            return !ReferenceEquals(otherOperation, null)
                   && OperationType == otherOperation.OperationType
                   && Left.Equals(otherOperation.Left)
                   && Right.Equals(otherOperation.Right);
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
            return typeof (SqlBinaryOperationExpression).GetHashCode()
                   ^ OperationType.GetHashCode()
                   ^ Left.GetHashCode()
                   ^ Right.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            BuildSqlOperand(Left, sqlBuilder);
            
            sqlBuilder.Append(" ");
            sqlBuilder.Append(GetSqlArithmeticOperatorSymbol(OperationType));
            sqlBuilder.Append(" ");

            BuildSqlOperand(Right, sqlBuilder);
        }

        /// <summary>
        /// Построение SQL для операнда.
        /// </summary>
        private static void BuildSqlOperand(SqlExpression operand, StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("( ");
            operand.BuildSql(sqlBuilder);
            sqlBuilder.Append(" )");
        }

        /// <summary>
        /// Получение SQL-символа арифметического оператора.
        /// </summary>
        private static string GetSqlArithmeticOperatorSymbol(SqlBinaryArithmeticOperationType operationType)
        {
            switch (operationType)
            {
                case SqlBinaryArithmeticOperationType.Add:
                    return "+";
                case SqlBinaryArithmeticOperationType.Subtract:
                    return "-";
                case SqlBinaryArithmeticOperationType.Multiply:
                    return "*";
                case SqlBinaryArithmeticOperationType.Divide:
                    return "/";
                default:
                    throw new ArgumentOutOfRangeException("operationType");
            }
        }
    }
}
