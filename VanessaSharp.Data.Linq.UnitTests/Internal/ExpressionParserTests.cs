using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>Модульные тесты на <see cref="ExpressionParser"/>.</summary>
    [TestFixture]
    public sealed class ExpressionParserTests
    {
        /// <summary>
        /// Мок <see cref="IQueryableExpressionTransformer"/>.
        /// </summary>
        private Mock<IQueryableExpressionTransformer> _queryableExpressionTransformerMock;

        /// <summary>
        /// Мок <see cref="IOneSMappingProvider"/>.
        /// </summary>
        private Mock<IOneSMappingProvider> _mappingProviderMock;

        /// <summary>
        /// Тестируемый экземпляр <see cref="ExpressionParser"/>.
        /// </summary>
        private ExpressionParser _testedInstance;

        /// <summary>
        /// Инициализация тестируемого экземпляра.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _queryableExpressionTransformerMock = new Mock<IQueryableExpressionTransformer>(MockBehavior.Strict);
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);

            _testedInstance = new ExpressionParser(
                _queryableExpressionTransformerMock.Object, _mappingProviderMock.Object);
        }
        
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
            
            var simpleQueryMock = new Mock<IQuery>(MockBehavior.Strict);
            simpleQueryMock
                .Setup(q => q.Transform(It.IsAny<IQueryTransformService>()))
                .Returns(expressionParseProduct)
                .Verifiable();

            _queryableExpressionTransformerMock
                .Setup(t => t.Transform(expression))
                .Returns(simpleQueryMock.Object)
                .Verifiable();

            // Act
            var result = _testedInstance.Parse(expression);

            // Assert
            Assert.AreSame(expressionParseProduct, result);

            _queryableExpressionTransformerMock
                .Verify(t => t.Transform(expression), Times.Once());
            simpleQueryMock
                .Verify(q => q.Transform(It.IsAny<IQueryTransformService>()), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="ExpressionParser.CheckDataType"/>.
        /// </summary>
        [Test]
        public void TestCheckDataType()
        {
            // Arrange
            _mappingProviderMock
                .Setup(p => p.CheckDataType(typeof(AnyData)))
                .Verifiable();

            // Act
            _testedInstance.CheckDataType(typeof(AnyData));

            // Assert
            _mappingProviderMock
                .Verify(p => p.CheckDataType(typeof(AnyData)), Times.Once());
        }

        /// <summary>Вспомогательный тип данных для тестирования.</summary>
        public struct AnyData
        {}
    }
}
