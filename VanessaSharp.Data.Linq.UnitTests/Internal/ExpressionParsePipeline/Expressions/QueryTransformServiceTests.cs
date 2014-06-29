using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="QueryTransformService"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryTransformServiceTests : TestsBase
    {
        /// <summary>
        /// Тестирование <see cref="QueryTransformService.CreateTransformer"/>
        /// в случае когда сервис создан через <see cref="QueryTransformService(IExpressionTransformMethods)"/>.
        /// </summary>
        [Test]
        public void TestCreateTransformerWhenCreateServiceThroughIExpressionTransformMethods()
        {
            // Arrange
            var expressionTransformMethods = new Mock<IExpressionTransformMethods>(MockBehavior.Strict).Object;

            // Act
            var testedService = new QueryTransformService(expressionTransformMethods);
            var result = testedService.CreateTransformer();

            // Assert
            var typedTransformer = AssertAndCast<QueryTransformer>(result);
            Assert.AreSame(expressionTransformMethods, typedTransformer.ExpressionTransformMethods);
        }

        /// <summary>
        /// Тестирование <see cref="QueryTransformService.CreateTransformer"/>
        /// в случае когда сервис создан через <see cref="QueryTransformService(IOneSMappingProvider)"/>.
        /// </summary>
        [Test]
        public void TestCreateTransformerWhenCreateServiceThroughIOneSMappingProvider()
        {
            // Arrange
            var mappingProvider = new Mock<IOneSMappingProvider>(MockBehavior.Strict).Object;

            // Act
            var testedService = new QueryTransformService(mappingProvider);
            var result = testedService.CreateTransformer();

            // Assert
            var typedTransformer = AssertAndCast<QueryTransformer>(result);
            var typedTransformMethods =
                AssertAndCast<ExpressionTransformMethods>(typedTransformer.ExpressionTransformMethods);

            Assert.AreSame(mappingProvider, typedTransformMethods.MappingProvider);
        }

        /// <summary>
        /// Тестирование <see cref="QueryTransformService.CreateTransformer"/>
        /// проверяющее, что каждый раз создается новый экзземпляр.
        /// </summary>
        [Test]
        public void TestCreateTransformThatNewInstance()
        {
            // Arrange
            var testedService = new QueryTransformService(new Mock<IExpressionTransformMethods>(MockBehavior.Strict).Object);

            // Act
            var result1 = testedService.CreateTransformer();
            var result2 = testedService.CreateTransformer();

            // Assert
            Assert.AreNotSame(result1, result2);
        }
    }
}
