using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение вызова агрегируемой функции COUNT.
    /// </summary>
    internal sealed class SqlCountExpression : SqlExpression
    {
        /// <summary>
        /// Выражение вызова агрегируемой функции COUNT для подсчета все строк запроса.
        /// </summary>
        public static SqlCountExpression All
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCountExpression>() != null);

                return _all;
            }
        }
        private static readonly SqlCountExpression _all
            = new SqlCountExpression(SqlAllCountSubExpression.Instance);

        /// <summary>
        /// Создание выражения вызова агрегируемой функции COUNT для подсчета ненулевых значений заданного выражения.
        /// </summary>
        /// <param name="expression">Выражение для которого подсчитывается количество.</param>
        /// <param name="isDistinct">Подсчитывать количество только различных значений.</param>
        public static SqlCountExpression CreateForExpression(SqlExpression expression, bool isDistinct)
        {
            return new SqlCountExpression(
                new SqlExpressionCountSubExpression(expression, isDistinct)
                );
        }
        
        /// <summary>Конструктор.</summary>
        /// <param name="subExpression">Подвыражение функции.</param>
        private SqlCountExpression(SqlCountSubExpression subExpression)
        {
            Contract.Requires<ArgumentNullException>(subExpression != null);
            
            _subExpression = subExpression;
        }

        /// <summary>Подвыражение функции.</summary>
        public SqlCountSubExpression SubExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlCountSubExpression>() != null);
                
                return _subExpression;
            }
        }
        private readonly SqlCountSubExpression _subExpression;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherCount = other as SqlCountExpression;

            return (otherCount != null)
                   && SubExpression.Equals(otherCount.SubExpression);
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
            return typeof (SqlCountExpression).GetHashCode()
                   ^ SubExpression.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("COUNT(");
            SubExpression.BuildSql(sqlBuilder);
            sqlBuilder.Append(")");
        }
    }
}
