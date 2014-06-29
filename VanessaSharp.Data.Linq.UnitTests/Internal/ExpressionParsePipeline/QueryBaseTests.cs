using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>Тесты на базовый класс <see cref="QueryBase{TInput,TOutput}"/>.</summary>
    [TestFixture]
    public sealed class QueryBaseTests
    {
        /// <summary>Тестирование <see cref="QueryBase{TInput,TOutput}.Transform"/>.</summary>
        [Test]
        public void TestTransform()
        {
            // Arrange
            var query = new Mock<QueryBase<InputData, OutputData>>(MockBehavior.Strict).Object;
            var expressionParseProduct = new CollectionReadExpressionParseProduct<OutputData>(
                new SqlCommand("SQL", new ReadOnlyCollection<SqlParameter>(new SqlParameter[0])), 
                new Mock<IItemReaderFactory<OutputData>>(MockBehavior.Strict).Object);

            var queryTransformerMock = new Mock<IQueryTransformer>(MockBehavior.Strict);
            queryTransformerMock
                .Setup(t => t.Transform(query))
                .Returns(expressionParseProduct);

            var queryTransformServiceMock = new Mock<IQueryTransformService>(MockBehavior.Strict);
            queryTransformServiceMock
                .Setup(s => s.CreateTransformer())
                .Returns(queryTransformerMock.Object);

            // Act
            var result = query.Transform(queryTransformServiceMock.Object);

            // Assert
            Assert.AreSame(expressionParseProduct, result);

            queryTransformerMock.Verify(t => t.Transform(query), Times.Once());
            queryTransformServiceMock.Verify(s => s.CreateTransformer(), Times.Once());
        }

        public sealed class InputData
        {}

        public sealed class OutputData
        {}
    }
}
