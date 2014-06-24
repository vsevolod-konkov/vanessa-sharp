using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
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

        private Mock<IOneSMappingProvider> _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
        }

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenBinaryRelation<T>(Expression<Func<T, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            // Arrange
            var context = new QueryParseContext();

            // Act
            var result = WhereExpressionTransformer.Transform(_mappingProviderMock.Object, context, testedFilter);
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

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenDataRecordBinaryRelation(Expression<Func<OneSDataRecord, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            TestTransformWhenBinaryRelation(testedFilter, expectedRelationType);
        }

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenTypedTupleBinaryRelation(Expression<Func<AnyData, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            var typeMapping = new OneSTypeMapping(
                "?",
                new ReadOnlyCollection<OneSFieldMapping>(
                    new[] {CreateFieldMapping(d => d.Id, FILTER_FIELD)}));

            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof (AnyData)))
                .Returns(typeMapping);
            
            TestTransformWhenBinaryRelation(testedFilter, expectedRelationType);
        }

        // TODO Копипаста QueryTransformerComponentTests
        private static OneSFieldMapping CreateFieldMapping<T>(Expression<Func<AnyData, T>> accessor, string fieldName)
        {
            var memberInfo = ((MemberExpression)accessor.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }

        public sealed class AnyData
        {
            public int Id;
        }
        
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordNotEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformDataRecordWhenGreater()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordGreaterOrEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordLess()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordLessOrEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(FILTER_FIELD) <= FILTER_VALUE, SqlBinaryRelationType.LessOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleEqual()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleNotEqual()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreater()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreaterOrEqual()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleLess()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleLessOrEqual()
        {
            TestTransformWhenTypedTupleBinaryRelation(d => d.Id <= FILTER_VALUE, SqlBinaryRelationType.LessOrEqual);
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие всегда истина.
        /// </summary>
        [Ignore("Надо подумать о желаемом поведении")]
        [Test]
        public void TestTransformWhenDataRecordAlwaysTrue()
        {
            // Arrange
            var context = new QueryParseContext();

            Expression<Func<OneSDataRecord, bool>> testedFilter = r => true;

            // Act
            var result = WhereExpressionTransformer.Transform(_mappingProviderMock.Object, context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            // ?
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие получения булевого значения поля.
        /// </summary>
        [Ignore("Надо подумать о желаемом поведении")]
        [Test]
        public void TestTransformWhenDataRecordGetBooleanField()
        {
            // Arrange
            var context = new QueryParseContext();

            Expression<Func<OneSDataRecord, bool>> testedFilter = r => r.GetBoolean(FILTER_FIELD);

            // Act
            var result = WhereExpressionTransformer.Transform(_mappingProviderMock.Object, context, testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            // ?
        }
    }
}
