using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобразователь LINQ-выражение метода Where в SQL-условие WHERE.</summary>
    internal sealed class WhereExpressionTransformer : ExpressionTransformerBase
    {
        private static readonly ISet<MethodInfo>
            _getValueMethods = new HashSet<MethodInfo>
            {
                { OneSQueryExpressionHelper.DataRecordGetStringMethod },
                { OneSQueryExpressionHelper.DataRecordGetInt32Method },
                { OneSQueryExpressionHelper.DataRecordGetDoubleMethod },
                { OneSQueryExpressionHelper.DataRecordGetDateTimeMethod },
                { OneSQueryExpressionHelper.DataRecordGetBooleanMethod },
                { OneSQueryExpressionHelper.DataRecordGetCharMethod },
            };
        
        /// <summary>Контекст разбора запроса.</summary>
        private readonly QueryParseContext _context;

        /// <summary>Выражение записи данных.</summary>
        private readonly ParameterExpression _recordExpression;

        // TODO: Сделать более защищенный вариант
        /// <summary>Стек выражений.</summary>
        private readonly Stack<object> _stack = new Stack<object>(); 

        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        private WhereExpressionTransformer(QueryParseContext context, ParameterExpression recordExpression)
        {
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(context != null);

            _context = context;
            _recordExpression = recordExpression;
        }

        /// <summary>Получение результирующего запроса.</summary>
        private SqlCondition GetCondition()
        {
            return (SqlCondition)_stack.Pop();
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        public static SqlCondition Transform(
            QueryParseContext context, 
            Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(filterExpression != null);

            var visitor = new WhereExpressionTransformer(context, filterExpression.Parameters[0]);
            visitor.Visit(filterExpression.Body);

            return visitor.GetCondition();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object == _recordExpression)
            {
                if (_getValueMethods.Contains(node.Method))
                {
                    var fieldName = GetConstant<string>(node.Arguments[0]);

                    _stack.Push(new SqlFieldExpression(fieldName));
                    return node;
                }

                throw CreateExpressionNotSupportedException(node);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var parameterName = _context.Parameters.GetOrAddNewParameterName(node.Value);
            var parameter = new SqlParameterExpression(parameterName);
            _stack.Push(parameter);
            
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var result = DefaultVisitBinary(node);

            var relationType = GetSqlBinaryRelationType(node.NodeType);
            if (relationType.HasValue)
            {
                var operand2 = (SqlExpression)_stack.Pop();
                var operand1 = (SqlExpression)_stack.Pop();

                var condition = new SqlBinaryRelationCondition(relationType.Value, operand1, operand2);
                _stack.Push(condition);

                return result;
            }
            
            throw CreateExpressionNotSupportedException(node);
        }

        /// <summary>Получение типа бинарного отношения в зависимости от типа узла выражения.</summary>
        private static SqlBinaryRelationType? GetSqlBinaryRelationType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return SqlBinaryRelationType.Equal;
                case ExpressionType.NotEqual:
                    return SqlBinaryRelationType.NotEqual;
                case ExpressionType.GreaterThan:
                    return SqlBinaryRelationType.Greater;
                case ExpressionType.GreaterThanOrEqual:
                    return SqlBinaryRelationType.GreaterOrEqual;
                case ExpressionType.LessThan:
                    return SqlBinaryRelationType.Less;
                case ExpressionType.LessThanOrEqual:
                    return SqlBinaryRelationType.LessOrEqual;
                default:
                    return null;
            }
        }
    }
}
