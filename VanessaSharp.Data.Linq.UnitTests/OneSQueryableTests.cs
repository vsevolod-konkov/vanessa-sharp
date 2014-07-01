using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тесты на <see cref="OneSQueryable{T}"/>.</summary>
    [TestFixture]
    public sealed class OneSQueryableTests
    {
        /// <summary>
        /// Мок <see cref="IOneSQueryProvider"/>.
        /// </summary>
        private Mock<IOneSQueryProvider> _queryProviderMock;

        /// <summary>
        /// Выражение в тестируемом экземпляре.
        /// </summary>
        private Expression _expression;

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSQueryable<string> _testedInstance;

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            var strings = new[] {"Vova", "Petya", "Vasya"}.AsQueryable();

            Expression<Func<IQueryable<string>>> expression = () => strings;

            _expression = expression.Body;

            _queryProviderMock = new Mock<IOneSQueryProvider>(MockBehavior.Strict);

            _testedInstance = new OneSQueryable<string>(_queryProviderMock.Object, _expression);
        }

        /// <summary>Тестирование свойств после инициализации.</summary>
        [Test]
        public void TestInit()
        {
            Assert.AreSame(_expression, _testedInstance.Expression);
            Assert.AreSame(_queryProviderMock.Object, _testedInstance.Provider);
            Assert.AreEqual(typeof(string), _testedInstance.ElementType);
        }

        /// <summary>
        /// Тестирование <see cref="OneSQueryable{T}.GetEnumerator"/>.
        /// </summary>
        [Test]
        public void TestGetEnumerator()
        {
            // Arrange
            Expression actualExpression = null;

            var expectedEnumerator = new Mock<IEnumerator<string>>(MockBehavior.Strict).Object;

            _queryProviderMock
                .Setup(provider => provider.Execute<IEnumerator<string>>(It.IsAny<Expression>()))
                .Callback<Expression>(e => { actualExpression = e; })
                .Returns(expectedEnumerator);

            // Act
            var result = _testedInstance.GetEnumerator();

            // Assert
            Assert.AreSame(expectedEnumerator, result);
            _queryProviderMock.Verify(provider => provider.Execute<IEnumerator<string>>(It.IsAny<Expression>()), Times.Once());

            var methodCallExpression = AssertEx.IsInstanceAndCastOf<MethodCallExpression>(actualExpression);
            Assert.AreSame(_expression, methodCallExpression.Object);
            Assert.AreEqual(OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<string>(), methodCallExpression.Method);
        }
    }
}
