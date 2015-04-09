using System.Collections.ObjectModel;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Условие совпадения значения со одним из значений списка.
    /// </summary>
    internal sealed class SqlInValuesListCondition : SqlCondition
    {
        /// <summary>Конструктор.</summary>
        /// <param name="operand">Проверяемый операнд.</param>
        /// <param name="valuesList">Список значений.</param>
        /// <param name="isIn">Проверка операнда в списке или вне его.</param>
        /// <param name="isHierarchy">Проверка операнда в иерархии списка значений.</param>
        public SqlInValuesListCondition(
            SqlExpression operand, ReadOnlyCollection<SqlParameterExpression> valuesList, bool isIn, bool isHierarchy)
        {
            _operand = operand;
            _valuesList = valuesList;
            _isIn = isIn;
            _isHierarchy = isHierarchy;
        }

        /// <summary>
        /// Проверяемый операнд.
        /// </summary>
        public SqlExpression Operand
        {
            get { return _operand; }
        }
        private readonly SqlExpression _operand;

        /// <summary>
        /// Список значений.
        /// </summary>
        public ReadOnlyCollection<SqlParameterExpression> ValuesList
        {
            get { return _valuesList; }
        }
        private readonly ReadOnlyCollection<SqlParameterExpression> _valuesList;

        /// <summary>
        /// Проверка операнда в списке или вне его.
        /// </summary>
        public bool IsIn
        {
            get { return _isIn; }
        }
        private readonly bool _isIn;

        /// <summary>
        /// Проверка операнда в иерархии списка значений.
        /// </summary>
        public bool IsHierarchy
        {
            get { return _isHierarchy; }
        }
        private readonly bool _isHierarchy;

        /// <summary>Генерация кода SQL-запроса.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            Operand.BuildSql(sqlBuilder);

            if (!IsIn)
                sqlBuilder.Append(" NOT");

            sqlBuilder.Append(" IN");

            if (IsHierarchy)
                sqlBuilder.Append(" HIERARCHY");

            sqlBuilder.Append(" (");

            var separator = string.Empty;

            foreach (var value in ValuesList)
            {
                sqlBuilder.Append(separator);
                value.BuildSql(sqlBuilder);

                separator = ", ";
            }

            sqlBuilder.Append(")");
        }
    }
}
