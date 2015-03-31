using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Тестирование <see cref="PreEvaluator"/>.
    /// </summary>
    [TestFixture]
    public sealed class PreEvaluatorTests
    {
        /// <summary>
        /// Основной тест
        /// </summary>
        [Test]
        public void Test()
        {
            // Arrange
            var closedVariable = 3456;

            Expression<Func<int, string>> testedNode =
                i => ((124 + new DateTime(2010, 10, 23).AddDays(4.5).Ticks/(double)closedVariable) + (i/2)).ToString();

            // Act
            var result = PreEvaluator.Evaluate(testedNode);

            // Assert
            Assert.IsInstanceOf<Expression<Func<int, string>>>(result);

            var expectedConstant = 124 + new DateTime(2010, 10, 23).AddDays(4.5).Ticks/(double)closedVariable;
            var operationNode = ((MethodCallExpression)((LambdaExpression)result).Body).Object;

            var leftNode = ((BinaryExpression)operationNode).Left;
            Assert.IsInstanceOf<ConstantExpression>(leftNode);
            var actualConstant = ((ConstantExpression)leftNode).Value;

            Assert.AreEqual(expectedConstant, actualConstant);

            var sourceDelegate = testedNode.Compile();
            var resultDelegate = ((Expression<Func<int, string>>)result).Compile();

            for (int i = 100, counter = 0; counter < 10; i += 11, counter++)
                Assert.AreEqual(
                    sourceDelegate(i),
                    resultDelegate(i)
                    );
        }
    }
}
