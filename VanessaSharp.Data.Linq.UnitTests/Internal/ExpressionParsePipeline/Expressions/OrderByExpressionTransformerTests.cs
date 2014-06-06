using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="OrderByExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class OrderByExpressionTransformerTests
    {
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
            var result = OrderByExpressionTransformer.Transform(new QueryParseContext(), sortKeyExpression);

            // Assert
            Assert.AreEqual(FIELD_NAME, result.FieldName);
        }
    }
}
