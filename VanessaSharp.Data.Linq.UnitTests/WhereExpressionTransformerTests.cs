using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование <see cref="WhereExpressionTransformer"/>.</summary>
    [TestFixture]
    public sealed class WhereExpressionTransformerTests : TestsBase
    {
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenEquals()
        {
            // Arrange
            var context = new QueryParseContext();
            
            const string FILTER_FIELD = "filter_field";
            const string FILTER_VALUE = "filter_value";

            Expression<Func<OneSDataRecord, bool>> testedFilter = r => r.GetString(FILTER_FIELD) == FILTER_VALUE;

            // Act
            var result = WhereExpressionTransformer.Transform(context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            var equalsCondition = AssertAndCast<SqlEqualsCondition>(result);
            
            var fieldExpression = AssertAndCast<SqlFieldExpression>(equalsCondition.FirstOperand);
            Assert.AreEqual(FILTER_FIELD, fieldExpression.FieldName);

            var parameterExpression = AssertAndCast<SqlParameterExpression>(equalsCondition.SecondOperand);
            var parameterName = parameterExpression.ParameterName;

            Assert.AreEqual(1, parameters.Count);
            var parameter = parameters[0];
            Assert.AreEqual(parameterName, parameter.Name);
            Assert.AreEqual(FILTER_VALUE, parameter.Value);
        }
    }
}
