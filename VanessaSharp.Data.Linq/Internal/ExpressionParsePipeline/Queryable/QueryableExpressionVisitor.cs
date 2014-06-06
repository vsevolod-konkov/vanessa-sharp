﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Посетитель для парсинга выражения генерируемого 
    /// <see cref="Queryable"/>.
    /// </summary>
    internal sealed class QueryableExpressionVisitor : ExpressionTransformerBase
    {
        /// <summary>Обработчики вызовов методов.</summary>
        private static readonly ReadOnlyCollection<ICallMethodHandler> _callMethodHandlers
            = new ReadOnlyCollection<ICallMethodHandler>
                (
                    new ICallMethodHandler[]
                        {
                            new CallGettingRecordsHandler(),
                            new CallEnumerableGetEnumeratorHandler(),
                            new CallQueryableSelectHandler(),
                            new CallQueryableWhereHandler(), 
                            new CallQueryableOrderByHandler(),
                            new CallQueryableOrderByDescendingHandler(),
                            new CallQueryableThenByHandler(),
                            new CallQueryableThenByDescendingHandler()
                        }
                );

        /// <summary>Обработчик выражений.</summary>
        private readonly IQueryableExpressionHandler _handler;

        /// <summary>Конструктор.</summary>
        /// <param name="handler">Обработчик выражений.</param>
        public QueryableExpressionVisitor(IQueryableExpressionHandler handler)
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
            foreach (var callMethodHandler in _callMethodHandlers)
            {
                Expression childNode;
                if (callMethodHandler.CheckAndHandle(_handler, node, out childNode))
                {
                    if (childNode != null)
                        Visit(childNode);
                    return node;
                }
            }

            throw CreateExpressionNotSupportedException(node);
        }

        #region Обработчики методов

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

        #endregion

        #region Вспомогательные типы

        /// <summary>Обработчик вызова метода.</summary>
        private interface ICallMethodHandler
        {
            /// <summary>
            /// Проверяет метод, что требуется его специальная обработка.
            /// </summary>
            /// <param name="handler">Обработчик визитора.</param>
            /// <param name="node">Узел вызова метода.</param>
            /// <param name="childNode">Дочерний узел, который надо обработать.
            /// Если метод не соответствует, то возвращается <c>null</c>.</param>
            /// <returns>
            /// Возвращает <c>true</c>, если метод был обработан.
            /// </returns>
            bool CheckAndHandle(IQueryableExpressionHandler handler, MethodCallExpression node, out Expression childNode);
        }

        /// <summary>
        /// Обработчик вызова метода <see cref="OneSQueryMethods.GetRecords"/>.
        /// </summary>
        private sealed class CallGettingRecordsHandler : ICallMethodHandler
        {
            public bool CheckAndHandle(IQueryableExpressionHandler handler, MethodCallExpression node, out Expression childNode)
            {
                childNode = null;

                if (node.Method != OneSQueryExpressionHelper.GetRecordsMethodInfo)
                    return false;

                handler.HandleGettingRecords(GetConstant<string>(node.Arguments[0]));
                return true;
            }
        }

        /// <summary>
        /// Обработчик вызова метода <see cref="IEnumerable{T}.GetEnumerator"/>.
        /// </summary>
        private sealed class CallEnumerableGetEnumeratorHandler : ICallMethodHandler
        {
            public bool CheckAndHandle(IQueryableExpressionHandler handler, MethodCallExpression node, out Expression childNode)
            {
                childNode = null;
                Type itemType;
                if (!OneSQueryExpressionHelper.IsEnumerableGetEnumeratorMethod(node.Method, out itemType))
                    return false;

                handler.HandleGettingEnumerator(itemType);
                childNode = node.Object;
                return true;
            }
        }

        /// <summary>
        /// Базовый класс обработчиков методов <see cref="Queryable"/>.
        /// </summary>
        private abstract class CallQueryableMethodHandler : ICallMethodHandler
        {
            public bool CheckAndHandle(IQueryableExpressionHandler handler, MethodCallExpression node,
                                       out Expression childNode)
            {
                const int QUERYABLE_ARGUMENT_INDEX = 0;
                const int EXPRESSION_ARGUMENT_INDEX = 1;

                childNode = null;
                if (!IsThisMethod(node.Method))
                    return false;

                var quotedExpression = node.Arguments[EXPRESSION_ARGUMENT_INDEX];
                LambdaExpression lambdaExpression;
                try
                {
                    lambdaExpression = UnQuote(quotedExpression);
                }
                catch (ArgumentException)
                {
                    throw CreateExpressionNotSupportedException(quotedExpression);
                }

                if (CheckExpressionAndHandle(handler, lambdaExpression))
                {
                    childNode = node.Arguments[QUERYABLE_ARGUMENT_INDEX];
                    return true;
                }

                throw CreateExpressionNotSupportedException(lambdaExpression);
            }

            /// <summary>Проверка того, что это действительно нужный метод.</summary>
            /// <param name="method">Проверяемый метод.</param>
            protected abstract bool IsThisMethod(MethodInfo method);

            /// <summary>Проверка типа выражения и в случае если тип выражения подходит производиться обработка.</summary>
            /// <param name="handler">Обработчик визитора.</param>
            /// <param name="lambdaExpression">Выражение.</param>
            /// <returns>
            /// Возвращает <c>true</c>, если вызов был обработан.
            /// </returns>
            protected abstract bool CheckExpressionAndHandle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression);
        }

        /// <summary>
        /// Базовый класс обработчиков методов запросов <see cref="Queryable"/>
        /// в которых результатом выражения может быть любой тип.
        /// </summary>
        private abstract class CallQueryableMethodWithAnyResultTypeHandler : CallQueryableMethodHandler
        {
            /// <summary>Обработка вызова метода.</summary>
            /// <param name="handler">Обработчик визитора.</param>
            /// <param name="lambdaExpression">Выражение получения данных.</param>
            protected abstract void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression);

            protected sealed override bool CheckExpressionAndHandle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                const int EXPRESSION_SOURCE_TYPE_INDEX = 0;

                var lambdaType = lambdaExpression.Type;
                if (lambdaType.IsGenericType
                    && lambdaType.GetGenericTypeDefinition() == typeof(Func<,>)
                    && lambdaType.GetGenericArguments()[EXPRESSION_SOURCE_TYPE_INDEX] == typeof(OneSDataRecord))
                {
                    Handle(handler, lambdaExpression);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>.
        /// </summary>
        private sealed class CallQueryableSelectHandler : CallQueryableMethodWithAnyResultTypeHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableSelectMethod(method);
            }

            protected override void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                handler.HandleSelect(lambdaExpression);
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>.
        /// </summary>
        private sealed class CallQueryableWhereHandler : CallQueryableMethodHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableWhereMethod(method);
            }

            protected override bool CheckExpressionAndHandle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                var typedFilterExpression = lambdaExpression as Expression<Func<OneSDataRecord, bool>>;
                if (typedFilterExpression == null)
                    return false;

                handler.HandleFilter(typedFilterExpression);
                return true;
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        private sealed class CallQueryableOrderByHandler : CallQueryableMethodWithAnyResultTypeHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableOrderByMethod(method);
            }

            protected override void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                handler.HandleOrderBy(lambdaExpression);
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        private sealed class CallQueryableOrderByDescendingHandler : CallQueryableMethodWithAnyResultTypeHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableOrderByDescendingMethod(method);
            }

            protected override void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                handler.HandleOrderByDescending(lambdaExpression);
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.ThenBy{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        private sealed class CallQueryableThenByHandler : CallQueryableMethodWithAnyResultTypeHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableThenByMethod(method);
            }

            protected override void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                handler.HandleThenBy(lambdaExpression);
            }
        }

        /// <summary>
        /// Обработчик вызова метода
        /// <see cref="Queryable.ThenByDescending{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        private sealed class CallQueryableThenByDescendingHandler : CallQueryableMethodWithAnyResultTypeHandler
        {
            protected override bool IsThisMethod(MethodInfo method)
            {
                return OneSQueryExpressionHelper.IsQueryableThenByDescendingMethod(method);
            }

            protected override void Handle(IQueryableExpressionHandler handler, LambdaExpression lambdaExpression)
            {
                handler.HandleThenByDescending(lambdaExpression);
            }
        }

        #endregion
    }
}