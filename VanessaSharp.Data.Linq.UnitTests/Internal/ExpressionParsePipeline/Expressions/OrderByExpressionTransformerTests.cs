using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="OrderByExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class OrderByExpressionTransformerTests
    {
        private readonly Mock<IOneSMappingProvider> _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
        
        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля записи методом <see cref="OneSDataRecord.GetInt32(string)"/>.
        /// </summary>
        [Test]
        public void TestTransformGetInt32()
        {
            const string FIELD_NAME = "sort_field";

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(FIELD_NAME);

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            Assert.AreEqual(FIELD_NAME, result.FieldName);
        }
    }
}
