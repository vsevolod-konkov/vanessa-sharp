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
        private readonly StackEngine _stackEngine = new StackEngine(); 

        /// <summary>Конструктор.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        private WhereExpressionTransformer(IOneSMappingProvider mappingProvider, QueryParseContext context, ParameterExpression recordExpression)
            : base(mappingProvider, context, recordExpression)
        {}

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
                return new WhereExpressionTransformer(mappingProvider, context, recordExpression);
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

        /// <summary>Посещение доступа к полю.</summary>
        /// <param name="fieldExpression">Выражение доступа к полю источника.</param>
        protected override void VisitFieldAccess(SqlFieldExpression fieldExpression)
        {
            _stackEngine.Push(fieldExpression);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var parameterName = Context.Parameters.GetOrAddNewParameterName(node.Value);
            var parameter = new SqlParameterExpression(parameterName);
            _stackEngine.Push(parameter);
            
            return node;
        }

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

        private sealed class StackEngine
        {
            private readonly Stack<object> _stack = new Stack<object>(); 

            public void Push(SqlExpression expression)
            {
                Contract.Requires<ArgumentNullException>(expression != null);

                _stack.Push(expression);
            }

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

            public void BinaryRelation(SqlBinaryRelationType relationType)
            {
                var secondOperand = Pop<SqlExpression>();
                var firstOperand = Pop<SqlExpression>();

                var condition = new SqlBinaryRelationCondition(relationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            public SqlCondition GetCondition()
            {
                return Pop<SqlCondition>();
            }
        }

        #endregion
    }
}
