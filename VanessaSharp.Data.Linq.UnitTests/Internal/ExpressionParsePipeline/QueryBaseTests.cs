using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.UnitTests.Utility;

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
            var sourceDescription = SourceDescriptionByType<InputData>.Instance;
            Expression<Func<InputData, OutputData>> selector = i => new OutputData();

            var expressionParseProduct = new Mock<ExpressionParseProduct>(
                MockBehavior.Strict,
                new SqlCommand("SQL", Empty.ReadOnly<SqlParameter>())).Object;

            var testedQuery = new TestedQuery<InputData, OutputData>(sourceDescription, selector);
            testedQuery.ExpectedExpressionParseProduct = expressionParseProduct;

            var queryTransformer = new Mock<IQueryTransformer>(MockBehavior.Strict).Object;

            var queryTransformServiceMock = new Mock<IQueryTransformService>(MockBehavior.Strict);
            queryTransformServiceMock
                .Setup(s => s.CreateTransformer())
                .Returns(queryTransformer);

            // Act
            var result = testedQuery.Transform(queryTransformServiceMock.Object);

            // Assert
            Assert.AreSame(expressionParseProduct, result);
            Assert.AreSame(queryTransformer, testedQuery.ActualTransformer);

            queryTransformServiceMock.Verify(s => s.CreateTransformer(), Times.Once());
        }

        public sealed class InputData
        {}

        public sealed class OutputData
        {}

        private sealed class TestedQuery<TInput, TOutput> : QueryBase<TInput, TOutput>
        {
            public TestedQuery(ISourceDescription source, Expression<Func<TInput, TOutput>> selector)
                : base(source, selector, null, Empty.ReadOnly<SortExpression>(), false)
            {}

            public ExpressionParseProduct ExpectedExpressionParseProduct 
            { 
                private get;
                set;
            }

            public IQueryTransformer ActualTransformer { get; private set; }

            protected override ExpressionParseProduct Transform(IQueryTransformer transformer)
            {
                ActualTransformer = transformer;

                return ExpectedExpressionParseProduct;
            }
        }
    }
}
