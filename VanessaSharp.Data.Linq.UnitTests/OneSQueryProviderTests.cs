﻿using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSQueryProvider"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSQueryProviderTests : TestsBase
    {
        /// <summary>Соединение.</summary>
        private OneSConnection _connection;

        /// <summary>Мок парсера выражений.</summary>
        private Mock<IExpressionParser> _expressionParserMock;

        /// <summary>Мок выполнения sql-запросов.</summary>
        private Mock<ISqlCommandExecuter> _sqlCommandExecuterMock; 

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSQueryProvider _testedInstance;

        /// <summary>Инициализация тестов.</summary>
        [SetUp]
        public void SetUp()
        {
            _connection = new OneSConnection();
            _expressionParserMock = new Mock<IExpressionParser>(MockBehavior.Strict);
            _sqlCommandExecuterMock = new Mock<ISqlCommandExecuter>(MockBehavior.Strict);
            _testedInstance = new OneSQueryProvider(_connection, _expressionParserMock.Object, _sqlCommandExecuterMock.Object);
        }

        /// <summary>
        /// Тестирование <see cref="OneSQueryProvider.Connection"/>
        /// </summary>
        [Test]
        public void TestConnection()
        {
            Assert.AreSame(_connection, _testedInstance.Connection);
        }

        /// <summary>
        /// Тестирование <see cref="OneSQueryProvider.CreateGetRecordsQuery"/>.
        /// </summary>
        [Test]
        public void TestCreateGetRecordsQuery()
        {
            const string SOURCE_NAME = "AnySource";

            // Act
            var result = _testedInstance.CreateGetRecordsQuery(SOURCE_NAME);

            // Assert
            var query = AssertAndCast<OneSQueryable<OneSDataRecord>>(result);

            Assert.AreSame(_testedInstance, query.Provider);

            var methodCallExpression = AssertAndCast<MethodCallExpression>(query.Expression);
            Assert.AreEqual(OneSQueryExpressionHelper.GetRecordsMethodInfo, methodCallExpression.Method);

            Assert.AreEqual(1, methodCallExpression.Arguments.Count);
            var argument = AssertAndCast<ConstantExpression>(methodCallExpression.Arguments[0]);

            Assert.AreEqual(SOURCE_NAME, argument.Value);
        }

        /// <summary>Вспомогательный тип для тестов.</summary>
        public struct AnyData
        {}

        /// <summary>
        /// Тестирование <see cref="OneSQueryProvider.CreateQueryOf{T}"/>.
        /// </summary>
        [Test]
        public void TestCreateCreateQueryOfAnyData()
        {
            // Arrange
            _expressionParserMock
                .Setup(p => p.CheckDataType(typeof(AnyData)))
                .Verifiable();

            // Act
            var result = _testedInstance.CreateQueryOf<AnyData>();

            // Assert
            var query = AssertAndCast<OneSQueryable<AnyData>>(result);

            Assert.AreSame(_testedInstance, query.Provider);

            var methodCallExpression = AssertAndCast<MethodCallExpression>(query.Expression);
            Assert.AreEqual(OneSQueryExpressionHelper.GetGetTypedRecordsMethodInfo<AnyData>(), methodCallExpression.Method);

            _expressionParserMock
                .Verify(p => p.CheckDataType(typeof(AnyData)), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="OneSQueryProvider.Execute{TResult}"/>
        /// в случае если выражение имеет тип несовместимый с типом результата.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTypedExecuteWhenIncompatibleTypeExpression()
        {
            Expression<Func<int>> expr = () => 5;

            var result = _testedInstance.Execute<string>(expr);
        }

        /// <summary>
        /// Тестирование выполнения запросов.
        /// </summary>
        [Test]
        public void TestExecute()
        {
            // Arrange
            const string EXPECTED_RESULT = "[Result]";

            var expectedCommand = new SqlCommand("[SQL]", new ReadOnlyCollection<SqlParameter>(new SqlParameter[0]));

            var productMock = new Mock<ExpressionParseProduct>(MockBehavior.Strict, expectedCommand);
            productMock
                .Setup(p => p.Execute(It.IsAny<ISqlCommandExecuter>()))
                .Returns(EXPECTED_RESULT)
                .Verifiable();

            Expression<Func<string>> expression = () => "Some string";

            _expressionParserMock
                .Setup(parser => parser.Parse(expression))
                .Returns(productMock.Object)
                .Verifiable();

            // Act
            var result = _testedInstance.Execute<string>(expression);

            // Assert
            Assert.AreEqual(EXPECTED_RESULT, result);

            _expressionParserMock.Verify(parser => parser.Parse(expression), Times.Once());
            productMock.Verify(p => p.Execute(_sqlCommandExecuterMock.Object), Times.Once());
        }
    }
}
