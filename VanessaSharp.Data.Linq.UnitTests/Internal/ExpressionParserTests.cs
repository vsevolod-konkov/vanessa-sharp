using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>Модульные тесты на <see cref="ExpressionParser"/>.</summary>
    [TestFixture]
    public sealed class ExpressionParserTests
    {
        /// <summary>Тестирование <see cref="ExpressionParser.Parse"/>.</summary>
        [Test]
        public void TestParse()
        {
            // Arrange
            var expressionMock = new Mock<Expression>(MockBehavior.Strict);
            expressionMock
                .Setup(e => e.ToString())
                .Returns("test");
            var expression = expressionMock.Object;

            var sqlCommand = new SqlCommand("SQL", SqlParameter.EmptyCollection);
            var expressionParseProduct = new Mock<ExpressionParseProduct>(MockBehavior.Strict, sqlCommand).Object;
            
            var simpleQueryMock = new Mock<SimpleQuery>(MockBehavior.Strict, "source", null, new ReadOnlyCollection<SortExpression>(new SortExpression[0]));
            simpleQueryMock
                .Setup(q => q.Transform())
                .Returns(expressionParseProduct)
                .Verifiable();

            var queryableExpressionTransformerMock = new Mock<IQueryableExpressionTransformer>(MockBehavior.Strict);
            queryableExpressionTransformerMock
                .Setup(t => t.Transform(expression))
                .Returns(simpleQueryMock.Object)
                .Verifiable();

            var testedInstance = new ExpressionParser(queryableExpressionTransformerMock.Object);

            // Act
            var result = testedInstance.Parse(expression);

            // Assert
            Assert.AreSame(expressionParseProduct, result);

            queryableExpressionTransformerMock
                .Verify(t => t.Transform(expression), Times.Once());
            simpleQueryMock
                .Verify(q => q.Transform(), Times.Once());
        }
    }
}
