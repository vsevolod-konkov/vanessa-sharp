using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Посетитель для парсинга выражения генерируемого 
    /// <see cref="Queryable"/>.
    /// </summary>
    internal sealed class QueryableExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>Делегат обработки вызова методов.</summary>
        /// <param name="node">Узел вызова метода.</param>
        /// <param name="childNode">Возвращаемый дочерний узел, который необходимо дальше разобрать.</param>
        /// <returns>Возвращает <c>true</c> если узел обработан.</returns>
        private delegate bool CheckAndHandlerMethod(MethodCallExpression node, out Expression childNode);

        /// <summary>Коллекция обработчиков узла метода.</summary>
        private readonly ReadOnlyCollection<CheckAndHandlerMethod> _checkAndHandlersOfMethod;
        
        /// <summary>Обработчик выражений.</summary>
        private readonly IQueryableExpressionHandler _handler;

        /// <summary>Конструктор для инициализации списка обработчиков.</summary>
        private QueryableExpressionVisitor()
        {
            _checkAndHandlersOfMethod = new ReadOnlyCollection<CheckAndHandlerMethod>(
                new CheckAndHandlerMethod[]
                    {
                        CheckAndHandleGettingRecordsMethod,
                        CheckAndHandleEnumerableGetEnumeratorMethod,
                        CheckAndHandleQueryableSelectMethod,
                        CheckAndHandleQueryableWhereMethod
                    });
        }

        /// <summary>Конструктор.</summary>
        /// <param name="handler">Обработчик выражений.</param>
        public QueryableExpressionVisitor(IQueryableExpressionHandler handler) : this()
        {
            Contract.Requires<ArgumentNullException>(handler != null);

            _handler = handler;
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
            foreach (var checkAndHandler in _checkAndHandlersOfMethod)
            {
                Expression childNode;
                if (checkAndHandler(node, out childNode))
                {
                    if (childNode != null)
                        Visit(childNode);
                    return node;
                }
            }

            throw CreateExpressionNotSupportedException(node);
        }

        #region Обработчики методов

        /// <summary>
        /// Проверка, того что узел соответствует вызову метода <see cref="IEnumerable{T}.GetEnumerator"/>
        /// и обработка его в случае если это так.
        /// </summary>
        /// <param name="node">Проверяемый узел.</param>
        /// <param name="childNode">
        /// Дочерний узел, который надо обработать.
        /// Если метод не соответствует, то возвращается <c>null</c>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если узел соответствует вызову методу <see cref="IEnumerable{T}.GetEnumerator"/>,
        /// в ином случае возвращается <c>false</c>.
        /// </returns>
        private bool CheckAndHandleEnumerableGetEnumeratorMethod(MethodCallExpression node, out Expression childNode)
        {
            childNode = null;
            Type itemType;
            if (!OneSQueryExpressionHelper.IsEnumerableGetEnumeratorMethod(node.Method, out itemType))
                return false;

            _handler.HandleGettingEnumerator(itemType);
            childNode = node.Object;
            return true;
        }

        /// <summary>
        /// Проверка, того что узел соответствует вызову метода 
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>
        /// и обработка его в случае если это так.
        /// </summary>
        /// <param name="node">Проверяемый узел.</param>
        /// <param name="childNode">
        /// Дочерний узел, который надо обработать.
        /// Если метод не соответствует, то возвращается <c>null</c>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если узел соответствует вызову методу
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>,
        /// в ином случае возвращается <c>false</c>.
        /// </returns>  
        private bool CheckAndHandleQueryableSelectMethod(MethodCallExpression node, out Expression childNode)
        {
            const int QUERYABLE_ARGUMENT_INDEX = 0;
            const int SELECT_EXPRESSION_ARGUMENT_INDEX = 1;
            const int SELECTOR_SOURCE_TYPE_INDEX = 0;

            childNode = null;
            Type itemType;
            if (!OneSQueryExpressionHelper.IsQueryableSelectMethod(node.Method, out itemType))
                return false;
            
            var quotedSelectExpression = node.Arguments[SELECT_EXPRESSION_ARGUMENT_INDEX];
            LambdaExpression selectExpression;
            try
            {
                selectExpression = UnQuote(quotedSelectExpression);
            }
            catch (ArgumentException)
            {
                throw CreateExpressionNotSupportedException(quotedSelectExpression);
            }

            var lambdaType = selectExpression.Type;
            if (lambdaType.IsGenericType
                && lambdaType.GetGenericTypeDefinition() == typeof(Func<,>)
                && lambdaType.GetGenericArguments()[SELECTOR_SOURCE_TYPE_INDEX] == typeof(OneSDataRecord))
            {
                _handler.HandleSelect(selectExpression);
                childNode = node.Arguments[QUERYABLE_ARGUMENT_INDEX];

                return true;
            }
                
            throw CreateExpressionNotSupportedException(selectExpression);
        }

        /// <summary>
        /// Проверка, того что узел соответствует вызову метода 
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>
        /// и обработка его в случае если это так.
        /// </summary>
        /// <param name="node">Проверяемый узел.</param>
        /// <param name="childNode">
        /// Дочерний узел, который надо обработать.
        /// Если метод не соответствует, то возвращается <c>null</c>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если узел соответствует вызову методу
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>,
        /// в ином случае возвращается <c>false</c>.
        /// </returns>  
        private bool CheckAndHandleQueryableWhereMethod(MethodCallExpression node, out Expression childNode)
        {
            const int QUERYABLE_ARGUMENT_INDEX = 0;
            const int WHERE_EXPRESSION_ARGUMENT_INDEX = 1;

            childNode = null;
            if (!OneSQueryExpressionHelper.IsQueryableWhereMethod(node.Method))
                return false;

            var quotedFilterExpression = node.Arguments[WHERE_EXPRESSION_ARGUMENT_INDEX];
            LambdaExpression filterExpression;
            try
            {
                filterExpression = UnQuote(quotedFilterExpression);
            }
            catch (ArgumentException)
            {
                throw CreateExpressionNotSupportedException(quotedFilterExpression);
            }

            var typedFilterExpression = filterExpression as Expression<Func<OneSDataRecord, bool>>;
            if (typedFilterExpression != null)
            {
                _handler.HandleFilter(typedFilterExpression);
                childNode = node.Arguments[QUERYABLE_ARGUMENT_INDEX];

                return true;
            }

            throw CreateExpressionNotSupportedException(filterExpression);
        }

        /// <summary>Разкавычивание лямбда-выражения.</summary>
        /// <param name="expression">Закавыченное лямбда-выражение.</param>
        private static LambdaExpression UnQuote(Expression expression)
        {
            CheckNodeType(expression, ExpressionType.Quote);
            var unquotedExpression = ((UnaryExpression)expression).Operand;

            CheckNodeType(unquotedExpression, ExpressionType.Lambda);
            return (LambdaExpression)unquotedExpression;
        }

        /// <summary>
        /// Проверка типа узла.
        /// </summary>
        /// <param name="expression">Проверяемый узел.</param>
        /// <param name="expressionType">Тип узла.</param>
        private static void CheckNodeType(Expression expression, ExpressionType expressionType)
        {
            if (expression.NodeType != expressionType)
            {
                throw new ArgumentException(string.Format(
                    "Ожидалось выражение типа \"{0}\". Переданное выражение \"{1}\".",
                    expressionType, expression));
            }
        }

        /// <summary>
        /// Проверка, того что узел соответствует вызову метода 
        /// <see cref="OneSQueryMethods.GetRecords"/>
        /// и обработка его в случае если это так.
        /// </summary>
        /// <param name="node">Проверяемый узел.</param>
        /// <param name="childNode">
        /// Всегда возвращается <c>null</c>.
        /// Данный параметер нужен только для того чтобы сигнатура метода соответствовала <see cref="CheckAndHandlerMethod"/>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если узел соответствует вызову методу
        /// <see cref="OneSQueryMethods.GetRecords"/>,
        /// в ином случае возвращается <c>false</c>.
        /// </returns>
        private bool CheckAndHandleGettingRecordsMethod(MethodCallExpression node, out Expression childNode)
        {
            childNode = null;

            if (node.Method != OneSQueryExpressionHelper.GetRecordsMethodInfo)
                return false;

            _handler.HandleGettingRecords(GetConstant<string>(node.Arguments[0]));
            return true;
        }

        #endregion

        protected override Expression VisitBinary(BinaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }
    }
}
