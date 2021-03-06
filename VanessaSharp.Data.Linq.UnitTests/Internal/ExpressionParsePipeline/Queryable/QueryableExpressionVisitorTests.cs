﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>Тестирование <see cref="QueryableExpressionVisitor"/>.</summary>
    [TestFixture]
    public sealed class QueryableExpressionVisitorTests
    {
        private Mock<IQueryableExpressionHandler> _handlerMock; 
        private QueryableExpressionVisitor _testedInstance;
        private List<MemberInfo> _methodCallsLog; 

        private static readonly MethodInfo _handleGettingEnumeratorMethod =
            GetMethod(h => h.HandleGettingEnumerator(It.IsAny<Type>()));

        private static readonly MethodInfo _handleGettingRecordsMethod =
            GetMethod(h => h.HandleGettingRecords(It.IsAny<string>()));

        private static readonly MethodInfo _handleDistinct =
            GetMethod(h => h.HandleDistinct());

        private static readonly MethodInfo _handleSelectMethod =
            GetMethod(h => h.HandleSelect(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleFilterMethod =
            GetMethod(h => h.HandleFilter(It.IsAny<Expression<Func<OneSDataRecord, bool>>>()));

        private static readonly MethodInfo _handleOrderByMethod =
            GetMethod(h => h.HandleOrderBy(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleOrderByDescendingMethod =
            GetMethod(h => h.HandleOrderByDescending(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleThenByMethod =
            GetMethod(h => h.HandleThenBy(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleThenByDescendingMethod =
            GetMethod(h => h.HandleThenByDescending(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleAggregateMethod =
            GetMethod(h => h.HandleAggregate(It.IsAny<Type>(), It.IsAny<AggregateFunction>(), It.IsAny<Type>()));

        private static readonly MethodInfo _handleTakeMethod =
            GetMethod(h => h.HandleTake(It.IsAny<int>()));

        private static MethodInfo GetMethod(Expression<Action<IQueryableExpressionHandler>> expression)
        {
            return OneSQueryExpressionHelper.ExtractMethodInfo(expression);
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _handlerMock = new Mock<IQueryableExpressionHandler>(MockBehavior.Strict);
            _testedInstance = new QueryableExpressionVisitor(_handlerMock.Object);
            
            _methodCallsLog = new List<MemberInfo>();
        }

        private void HandlerSetupHandleGettingEnumerator(Type itemType)
        {
            _handlerMock.Setup(h => h.HandleGettingEnumerator(itemType))
                .Callback(() => _methodCallsLog.Add(_handleGettingEnumeratorMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleDistinct()
        {
            _handlerMock
                .Setup(h => h.HandleDistinct())
                .Callback(() => _methodCallsLog.Add(_handleDistinct))
                .Verifiable();
        }

        private void HandlerSetupHandleSelect(LambdaExpression selectExpression)
        {
            _handlerMock
                .Setup(h => h.HandleSelect(selectExpression))
                .Callback(() => _methodCallsLog.Add(_handleSelectMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleOrderBy(LambdaExpression sortKeyExpression)
        {
            _handlerMock
                .Setup(h => h.HandleOrderBy(sortKeyExpression))
                .Callback(() => _methodCallsLog.Add(_handleOrderByMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            _handlerMock
                .Setup(h => h.HandleOrderByDescending(sortKeyExpression))
                .Callback(() => _methodCallsLog.Add(_handleOrderByDescendingMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleThenBy(LambdaExpression sortKeyExpression)
        {
            _handlerMock
                .Setup(h => h.HandleThenBy(sortKeyExpression))
                .Callback(() => _methodCallsLog.Add(_handleThenByMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            _handlerMock
                .Setup(h => h.HandleThenByDescending(sortKeyExpression))
                .Callback(() => _methodCallsLog.Add(_handleThenByDescendingMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            _handlerMock
                .Setup(h => h.HandleFilter(filterExpression))
                .Callback(() => _methodCallsLog.Add(_handleFilterMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleGettingRecords(string sourceName)
        {
            _handlerMock
                .Setup(h => h.HandleGettingRecords(sourceName))
                .Callback(() => _methodCallsLog.Add(_handleGettingRecordsMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
        {
            _handlerMock
                .Setup(h => h.HandleAggregate(outputItemType, function, scalarType))
                .Callback(() => _methodCallsLog.Add(_handleAggregateMethod))
                .Verifiable();
        }

        private void HandlerSetupHandleTake(int count)
        {
            _handlerMock
                .Setup(h => h.HandleTake(count))
                .Callback(() => _methodCallsLog.Add(_handleTakeMethod))
                .Verifiable();
        }

        private void AssertMethodCalls(params MethodInfo[] expectedMethods)
        {
            CollectionAssert.AreEqual(expectedMethods, _methodCallsLog);
        }

        /// <summary>
        /// Тестирование посещения выражения выборки экземпляров анонимного типа из полученных записей.
        /// </summary>
        [Test]
        public void TestVisits()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const int COUNT = 5;
            var selectExpression = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new { Name = r.GetString("Name"), Value = r.GetInt32("Value") });
            var itemType = selectExpression.GetTraitOfOutputType().Type;

            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("filter_value") == "filter_value";
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("sort_field_2");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3Expression = r => r.GetDateTime("sort_field_3");

            HandlerSetupHandleGettingEnumerator(itemType);
            HandlerSetupHandleTake(COUNT);
            HandlerSetupHandleDistinct();
            HandlerSetupHandleSelect(selectExpression);
            HandlerSetupHandleOrderBy(sortKey1Expression);
            HandlerSetupHandleThenBy(sortKey2Expression);
            HandlerSetupHandleThenByDescending(sortKey3Expression);
            HandlerSetupHandleFilter(filterExpression);
            HandlerSetupHandleGettingRecords(SOURCE_NAME);

            var expression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
                    .Where(filterExpression)
                    .OrderBy(sortKey1Expression)
                    .ThenBy(sortKey2Expression)
                    .ThenByDescending(sortKey3Expression)
                    .Select(selectExpression)
                    .Distinct()
                    .Take(COUNT));
            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(itemType), Times.Once());
            _handlerMock.Verify(h => h.HandleTake(COUNT), Times.Once());
            _handlerMock.Verify(h => h.HandleDistinct(), Times.Once());
            _handlerMock.Verify(h => h.HandleSelect(selectExpression), Times.Once());
            _handlerMock.Verify(h => h.HandleOrderBy(sortKey1Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleThenBy(sortKey2Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleThenByDescending(sortKey3Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleFilter(filterExpression), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            AssertMethodCalls(
                        _handleGettingEnumeratorMethod,
                        _handleTakeMethod,
                        _handleDistinct,
                        _handleSelectMethod,
                        _handleThenByDescendingMethod,
                        _handleThenByMethod,
                        _handleOrderByMethod,
                        _handleFilterMethod,
                        _handleGettingRecordsMethod);
        }

        /// <summary>
        /// Тестирование посещения выражения с вызовом метода
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestVisitOrderByDescending()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("sort_field_2");

            HandlerSetupHandleGettingEnumerator(typeof(OneSDataRecord));
            HandlerSetupHandleOrderByDescending(sortKey1Expression);
            HandlerSetupHandleThenBy(sortKey2Expression);
            HandlerSetupHandleGettingRecords(SOURCE_NAME);

            var expression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
                    .OrderByDescending(sortKey1Expression)
                    .ThenBy(sortKey2Expression));
            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(typeof(OneSDataRecord)), Times.Once());
            _handlerMock.Verify(h => h.HandleOrderByDescending(sortKey1Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleThenBy(sortKey2Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            AssertMethodCalls(
                        _handleGettingEnumeratorMethod,
                        _handleThenByMethod,
                        _handleOrderByDescendingMethod,
                        _handleGettingRecordsMethod);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// с двойным вызовом метода <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestVisitDoubleOrderBy()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("sort_field_2");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3Expression = r => r.GetDateTime("sort_field_3");

            HandlerSetupHandleGettingEnumerator(typeof(OneSDataRecord));
            HandlerSetupHandleOrderBy(sortKey1Expression);
            HandlerSetupHandleOrderBy(sortKey2Expression);
            HandlerSetupHandleThenBy(sortKey3Expression);
            HandlerSetupHandleGettingRecords(SOURCE_NAME);

            var expression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
                    .OrderBy(sortKey1Expression)
                    .OrderBy(sortKey2Expression)
                    .ThenBy(sortKey3Expression));

            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(typeof(OneSDataRecord)), Times.Once());
            _handlerMock.Verify(h => h.HandleOrderBy(sortKey1Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleOrderBy(sortKey2Expression), Times.Once()); 
            _handlerMock.Verify(h => h.HandleThenBy(sortKey3Expression), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            AssertMethodCalls(
                        _handleGettingEnumeratorMethod,
                        _handleThenByMethod,
                        _handleOrderByMethod,
                        _handleOrderByMethod,
                        _handleGettingRecordsMethod);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения агрегатного значения.
        /// </summary>
        private void TestVisitAggregate<T>(Expression<Func<IQueryable<int>, T>> scalarGetter, AggregateFunction expectedFunction)
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, int>> selectExpression = r => r.GetInt32("field_1");

            HandlerSetupHandleAggregate(typeof(int), expectedFunction, typeof(T));
            HandlerSetupHandleSelect(selectExpression);
            HandlerSetupHandleGettingRecords(SOURCE_NAME);

            var expression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .ScalarQuery(q => q.Select(selectExpression), scalarGetter);

            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleAggregate(typeof(int), expectedFunction, typeof(T)), Times.Once());
            _handlerMock.Verify(h => h.HandleSelect(selectExpression), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            AssertMethodCalls(
                        _handleAggregateMethod,
                        _handleSelectMethod,
                        _handleGettingRecordsMethod);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения суммы.
        /// </summary>
        [Test]
        public void TestVisitSum()
        {
            TestVisitAggregate(q => q.Sum(), AggregateFunction.Summa);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения среднего.
        /// </summary>
        [Test]
        public void TestVisitAverage()
        {
            TestVisitAggregate(q => q.Average(), AggregateFunction.Average);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения максимума.
        /// </summary>
        [Test]
        public void TestVisitMax()
        {
            TestVisitAggregate(q => q.Max(), AggregateFunction.Maximum);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения минимума.
        /// </summary>
        [Test]
        public void TestVisitMin()
        {
            TestVisitAggregate(q => q.Min(), AggregateFunction.Minimum);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения количества.
        /// </summary>
        [Test]
        public void TestVisitCount()
        {
            TestVisitAggregate(q => q.Count(), AggregateFunction.Count);
        }

        /// <summary>
        /// Тестирование посещения выражения 
        /// c функцией получения количества.
        /// </summary>
        [Test]
        public void TestVisitLongCount()
        {
            TestVisitAggregate(q => q.LongCount(), AggregateFunction.Count);
        }
    }
}
