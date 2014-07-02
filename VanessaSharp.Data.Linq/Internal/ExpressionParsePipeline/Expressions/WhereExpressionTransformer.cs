using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь LINQ-выражение метода Where в SQL-условие WHERE.</summary>
    internal sealed class WhereExpressionTransformer : FieldAccessExpressionTransformerBase<SqlCondition>
    {
        /// <summary>Стековая машина для генерации условия.</summary>
        private readonly StackEngine _stackEngine;

        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="fieldAccessVisitorStrategy">Стратегия посещения доступа к полю.</param>
        /// <param name="stackEngine">Стековая машина для генерации условия.</param>
        private WhereExpressionTransformer(QueryParseContext context,
                                           FieldAccessVisitorStrategy fieldAccessVisitorStrategy,
                                           StackEngine stackEngine)
            : base(context, fieldAccessVisitorStrategy)
        {
            _stackEngine = stackEngine;
        }

        /// <summary>Получение результата трансформации.</summary>
        protected override SqlCondition GetTransformResult()
        {
            return _stackEngine.GetCondition();
        }

        /// <summary>Фабрика преобразователя.</summary>
        private sealed class Factory : TransformerFactoryBase
        {
            /// <summary>Создание преобразователя выражения.</summary>
            /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
            /// <param name="context">Контекст разбора запроса.</param>
            /// <param name="recordExpression">Выражение записи данных</param>
            public override FieldAccessExpressionTransformerBase<SqlCondition> Create(
                IOneSMappingProvider mappingProvider, QueryParseContext context, ParameterExpression recordExpression)
            {
                var stackEngine = new StackEngine();
                var fieldAccessVisitorStrategy = CreateFieldAccessVisitorStrategy(stackEngine, recordExpression, mappingProvider);
                
                return new WhereExpressionTransformer(context, fieldAccessVisitorStrategy, stackEngine);
            }
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип фильтруемых элементов.</typeparam>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        public static SqlCondition Transform<T>(
            IOneSMappingProvider mappingProvider,
            QueryParseContext context, 
            Expression<Func<T, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(filterExpression != null);
            Contract.Ensures(Contract.Result<SqlCondition>() != null);

            return Transform<Factory>(mappingProvider, context, filterExpression);
        }

        /// <summary>
        /// Просматривает выражение <see cref="T:System.Linq.Expressions.ConstantExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var parameterName = Context.Parameters.GetOrAddNewParameterName(node.Value);
            var parameter = new SqlParameterExpression(parameterName);
            _stackEngine.Push(parameter);
            
            return node;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var result = DefaultVisitBinary(node);

            var relationType = GetSqlBinaryRelationType(node.NodeType);
            if (relationType.HasValue)
            {
                _stackEngine.BinaryRelation(relationType.Value);
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

        #region Вспомогательные типы

        /// <summary>
        /// Стековая машина для разбора выражений.
        /// </summary>
        private sealed class StackEngine : IFieldAccessVisitorForOnlySql
        {
            /// <summary>Стек выражений.</summary>
            private readonly Stack<object> _stack = new Stack<object>(); 

            /// <summary>Вставка выражения в стек.</summary>
            public void Push(SqlExpression expression)
            {
                Contract.Requires<ArgumentNullException>(expression != null);

                _stack.Push(expression);
            }

            /// <summary>Вытягивание типизированного объекта из стека.</summary>
            /// <typeparam name="T">Тип объекта.</typeparam>
            private T Pop<T>()
                where T : class
            {
                if (_stack.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но стек оказался пуст.", typeof(T)));
                }

                var obj = _stack.Pop();
                var result = obj as T;

                if (result == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof(T), obj));
                }

                return result;
            }

            /// <summary>Вытягивание двух выражений из стека и положение в стек созданного условия бинарного отношения.</summary>
            /// <param name="relationType">Тип бинарного отношения.</param>
            public void BinaryRelation(SqlBinaryRelationType relationType)
            {
                var secondOperand = Pop<SqlExpression>();
                var firstOperand = Pop<SqlExpression>();

                var condition = new SqlBinaryRelationCondition(relationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            /// <summary>Получение условия.</summary>
            public SqlCondition GetCondition()
            {
                return Pop<SqlCondition>();
            }

            /// <summary>Посещение узла доступа к полю.</summary>
            /// <param name="fieldExpression">SQL-Выражение поля.</param>
            void IFieldAccessVisitorForOnlySql.VisitFieldAccess(SqlFieldExpression fieldExpression)
            {
                Push(fieldExpression);
            }
        }

        #endregion
    }
}
