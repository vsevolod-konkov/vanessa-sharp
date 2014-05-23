using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование <see cref="QueryableExpressionVisitor"/>.</summary>
    [TestFixture]
    public sealed class QueryableExpressionVisitorTests
    {
        private Mock<IQueryableExpressionHandler> _handlerMock; 

        private QueryableExpressionVisitor _testedInstance;

        private static readonly MethodInfo _handleGettingEnumeratorMethod =
            GetMethod(h => h.HandleGettingEnumerator(It.IsAny<Type>()));

        private static readonly MethodInfo _handleGettingRecordsMethod =
            GetMethod(h => h.HandleGettingRecords(It.IsAny<string>()));

        private static readonly MethodInfo _handleSelectMethod =
            GetMethod(h => h.HandleSelect(It.IsAny<LambdaExpression>()));

        private static readonly MethodInfo _handleFilterMethod =
            GetMethod(h => h.HandleFilter(It.IsAny<Expression<Func<OneSDataRecord, bool>>>()));

        private static MethodInfo GetMethod(Expression<Action<IQueryableExpressionHandler>> expression)
        {
            return OneSQueryExpressionHelper.ExtractMethodInfo(expression);
        }

        /// <summary>
        /// Получение выражения получения записей из источника данных.
        /// </summary>
        /// <param name="sourceName">Источник.</param>
        private static Expression GetGetRecordsExpression(string sourceName)
        {
            return Expression.Call(
                OneSQueryExpressionHelper.GetRecordsExpression(sourceName),
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _handlerMock = new Mock<IQueryableExpressionHandler>(MockBehavior.Strict);

            _testedInstance = new QueryableExpressionVisitor(_handlerMock.Object);
        }

        /// <summary>
        /// Тестирование посещения выражения получения записей.
        /// </summary>
        [Test]
        public void TestVisitGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expectedType = typeof(OneSDataRecord);

            var calls = new List<MethodInfo>();

            _handlerMock
                .Setup(h => h.HandleGettingEnumerator(expectedType))
                .Callback(() => calls.Add(_handleGettingEnumeratorMethod))
                .Verifiable();

            _handlerMock
                .Setup(h => h.HandleGettingRecords(SOURCE_NAME))
                .Callback(() => calls.Add(_handleGettingRecordsMethod))
                .Verifiable();

            var expression = GetGetRecordsExpression(SOURCE_NAME);
            
            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(expectedType), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            Assert.AreEqual(2, calls.Count);
            Assert.AreEqual(_handleGettingEnumeratorMethod, calls[0]);
            Assert.AreEqual(_handleGettingRecordsMethod, calls[1]);
        }

        /// <summary>
        /// Тестирование посещения выражения выборки экземпляров анонимного типа из полученных записей.
        /// </summary>
        [Test]
        public void TestVisitSelectExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var selectExpression = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new {Name = r.GetString("Name"), Value = r.GetInt32("Value")});
            
            var calls = new List<MethodInfo>();

            var traitOfOutputType = selectExpression.GetTraitOfOutputType();
            var itemType = traitOfOutputType.Type;

            _handlerMock.Setup(h => h.HandleGettingEnumerator(itemType))
                .Callback(() => calls.Add(_handleGettingEnumeratorMethod))
                .Verifiable();

            _handlerMock
                .Setup(h => h.HandleSelect(selectExpression))
                .Callback(() => calls.Add(_handleSelectMethod))
                .Verifiable();

            _handlerMock
                .Setup(h => h.HandleGettingRecords(SOURCE_NAME))
                .Callback(() => calls.Add(_handleGettingRecordsMethod))
                .Verifiable();

            var expression = TestHelperQueryProvider
                                .BuildTestQueryExpression(traitOfOutputType, SOURCE_NAME, q => q.Select(selectExpression));

            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(itemType), Times.Once());
            _handlerMock.Verify(h => h.HandleSelect(selectExpression), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());

            Assert.AreEqual(3, calls.Count);
            Assert.AreEqual(_handleGettingEnumeratorMethod, calls[0]);
            Assert.AreEqual(_handleSelectMethod, calls[1]);
            Assert.AreEqual(_handleGettingRecordsMethod, calls[2]);
        }

        /// <summary>
        /// Тестирование посещения выражения фильтрации получения записей.
        /// </summary>
        [Test]
        public void TestVisitWhereRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expectedType = typeof(OneSDataRecord);

            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filterField]") == "filterValue";

            var calls = new List<MethodInfo>();

            _handlerMock
                .Setup(h => h.HandleGettingEnumerator(expectedType))
                .Callback(() => calls.Add(_handleGettingEnumeratorMethod))
                .Verifiable();

            _handlerMock
                .Setup(h => h.HandleGettingRecords(SOURCE_NAME))
                .Callback(() => calls.Add(_handleGettingRecordsMethod))
                .Verifiable();

            _handlerMock
                .Setup(h => h.HandleFilter(filterExpression))
                .Callback(() => calls.Add(_handleFilterMethod))
                .Verifiable();

            var expression = TestHelperQueryProvider
                                .BuildTestQueryExpression(SOURCE_NAME, q => q.Where(filterExpression));

            // Act
            _testedInstance.Visit(expression);

            // Assert
            _handlerMock.Verify(h => h.HandleGettingEnumerator(expectedType), Times.Once());
            _handlerMock.Verify(h => h.HandleGettingRecords(SOURCE_NAME), Times.Once());
            _handlerMock.Verify(h => h.HandleFilter(filterExpression), Times.Once());

            Assert.AreEqual(3, calls.Count);
            Assert.AreEqual(_handleGettingEnumeratorMethod, calls[0]);
            Assert.AreEqual(_handleFilterMethod, calls[1]);
            Assert.AreEqual(_handleGettingRecordsMethod, calls[2]);
        }
    }
}
