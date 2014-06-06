using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Тестирование <see cref="WhereExpressionTransformer"/>.</summary>
    [TestFixture]
    public sealed class WhereExpressionTransformerTests : TestsBase
    {
        private const string FILTER_FIELD = "filter_field";
        private const int FILTER_VALUE = 24;

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private static void TestTransformWhenBinaryRelation(Expression<Func<OneSDataRecord, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            // Arrange
            var context = new QueryParseContext();

            // Act
            var result = WhereExpressionTransformer.Transform(context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            var binaryCondition = AssertAndCast<SqlBinaryRelationCondition>(result);

            Assert.AreEqual(expectedRelationType, binaryCondition.RelationType);

            var fieldExpression = AssertAndCast<SqlFieldExpression>(binaryCondition.FirstOperand);
            Assert.AreEqual(FILTER_FIELD, fieldExpression.FieldName);

            var parameterExpression = AssertAndCast<SqlParameterExpression>(binaryCondition.SecondOperand);
            var parameterName = parameterExpression.ParameterName;

            Assert.AreEqual(1, parameters.Count);
            var parameter = parameters[0];
            Assert.AreEqual(parameterName, parameter.Name);
            Assert.AreEqual(FILTER_VALUE, parameter.Value);
        }
        
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenEqual()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenNotEqual()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformWhenGreater()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenGreaterOrEqual()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenLess()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие меньше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenLessOrEqual()
        {
            TestTransformWhenBinaryRelation(r => r.GetInt32(FILTER_FIELD) <= FILTER_VALUE, SqlBinaryRelationType.LessOrEqual);
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие всегда истина.
        /// </summary>
        [Ignore("Надо подумать о желаемом поведении")]
        [Test]
        public void TestTransformWhenAlwaysTrue()
        {
            // Arrange
            var context = new QueryParseContext();

            Expression<Func<OneSDataRecord, bool>> testedFilter = r => true;

            // Act
            var result = WhereExpressionTransformer.Transform(context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            // ?
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform"/>
        /// в случае если в обрабатывается условие получения булевого значения поля.
        /// </summary>
        [Ignore("Надо подумать о желаемом поведении")]
        [Test]
        public void TestTransformWhenGetBooleanField()
        {
            // Arrange
            var context = new QueryParseContext();

            Expression<Func<OneSDataRecord, bool>> testedFilter = r => r.GetBoolean(FILTER_FIELD);

            // Act
            var result = WhereExpressionTransformer.Transform(context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            // ?
        }
    }
}
