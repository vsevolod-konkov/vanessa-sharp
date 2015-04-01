using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Тестирование <see cref="WhereExpressionTransformer"/>.</summary>
    [TestFixture]
    public sealed class WhereExpressionTransformerTests
    {
        private const string FILTER_FIELD = "filter_field";
        private const string NULLABLE_FIELD = "nullable_field";
        private const int FILTER_VALUE = 24;

        private Mock<IOneSMappingProvider> _mappingProviderMock;
        private QueryParseContext _context;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("?")
                    .FieldMap(d => d.Id, FILTER_FIELD)
                    .FieldMap(d => d.Name, NULLABLE_FIELD)
                .End();

            _context = new QueryParseContext();
        }

        /// <summary>
        /// Запуск тестируемого преобразования.
        /// </summary>
        /// <param name="testedFilter">Тестируемое выражение.</param>
        private SqlCondition Transform<T>(Expression<Func<T, bool>> testedFilter)
        {
            return WhereExpressionTransformer.Transform(_mappingProviderMock.Object, _context, testedFilter);
        }

        /// <summary>
        /// Получение sql-параметров запроса.
        /// </summary>
        private ReadOnlyCollection<SqlParameter> GetSqlParameters()
        {
            return _context.Parameters.GetSqlParameters();
        }

        /// <summary>
        /// Проверка бинарного отношения.
        /// </summary>
        /// <param name="testedCondition">Тестируемое условие.</param>
        /// <param name="expectedRelationType">Тип сравнения.</param>
        private void AssertBinaryRelation(SqlCondition testedCondition, SqlBinaryRelationType expectedRelationType)
        {
            var binaryCondition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(testedCondition);

            Assert.AreEqual(expectedRelationType, binaryCondition.RelationType);

            var fieldExpression = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(binaryCondition.FirstOperand);
            Assert.AreEqual(FILTER_FIELD, fieldExpression.FieldName);

            var parameterExpression = AssertEx.IsInstanceAndCastOf<SqlParameterExpression>(binaryCondition.SecondOperand);
            var parameterName = parameterExpression.ParameterName;

            var parameters = GetSqlParameters();
            Assert.AreEqual(1, parameters.Count);
            var parameter = parameters[0];
            Assert.AreEqual(parameterName, parameter.Name);
            Assert.AreEqual(FILTER_VALUE, parameter.Value);
        }

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenBinaryRelation<T>(Expression<Func<T, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            // Act
            var result = Transform(testedFilter);

            // Assert
            AssertBinaryRelation(result, expectedRelationType);
        }

        /// <summary>Тестирование преобразования бинарного отношения записей <see cref="OneSDataRecord"/>.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenDataRecordBinaryRelation(Expression<Func<OneSDataRecord, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            TestTransformWhenBinaryRelation(testedFilter, expectedRelationType);
        }

        /// <summary>Тестирование преобразования бинарного отношения.</summary>
        /// <param name="testedFilter">Тестируемое выражение фильтрации.</param>
        /// <param name="expectedRelationType">Ожидаемый тип бинарного отношения в результирующем SQL-условии.</param>
        private void TestTransformWhenTypedRecordBinaryRelation(Expression<Func<SomeData, bool>> testedFilter, SqlBinaryRelationType expectedRelationType)
        {
            TestTransformWhenBinaryRelation(testedFilter, expectedRelationType);
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
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleNotEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreater()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreaterOrEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleLess()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="WhereExpressionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleLessOrEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id <= FILTER_VALUE, SqlBinaryRelationType.LessOrEqual);
        }

        /// <summary>
        /// Тестирование унарной операции.
        /// </summary>
        [Test]
        public void TestTransformWhenNotOperation()
        {
            // Act
            Expression<Func<SomeData, bool>> testedFilter = d => !(d.Id < FILTER_VALUE);
            var result = Transform(testedFilter);

            // Assert
            var notCondition = AssertEx.IsInstanceAndCastOf<SqlNotCondition>(result);
            AssertBinaryRelation(notCondition.Condition, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование бинарной операции.
        /// </summary>
        private void TestTransformWhenBinaryOperation(Expression<Func<SomeData, bool>> testedFilter, SqlBinaryOperationType expectedOperationType)
        {
            // Act
            var result = Transform(testedFilter);

            // Assert
            var binaryOperationCondition = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationCondition>(result);
            Assert.AreEqual(expectedOperationType, binaryOperationCondition.OperationType);

            AssertBinaryRelation(binaryOperationCondition.FirstOperand, SqlBinaryRelationType.LessOrEqual);
            AssertBinaryRelation(binaryOperationCondition.SecondOperand, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование бинарной операции И.
        /// </summary>
        [Test]
        public void TestTransformWhenAndOperation()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => (d.Id <= FILTER_VALUE) && (d.Id >= FILTER_VALUE);
            TestTransformWhenBinaryOperation(testedFilter, SqlBinaryOperationType.And);
        }

        /// <summary>
        /// Тестирование бинарной операции ИЛИ.
        /// </summary>
        [Test]
        public void TestTransformWhenOrOperation()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => (d.Id <= FILTER_VALUE) || (d.Id >= FILTER_VALUE);
            TestTransformWhenBinaryOperation(testedFilter, SqlBinaryOperationType.Or);
        }

        /// <summary>
        /// Тестирование преобразования проверки на <c>null</c>.
        /// </summary>
        private void TestTransformWhenIsNull(Expression<Func<SomeData, bool>> testedFilter, bool expectedTestIsNull)
        {
            // Act
            var result = Transform(testedFilter);

            // Assert
            var isNullCondition = AssertEx.IsInstanceAndCastOf<SqlIsNullCondition>(result);
            Assert.AreEqual(expectedTestIsNull, isNullCondition.IsNull);

            var fieldExpression = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(isNullCondition.Expression);
            Assert.AreEqual(NULLABLE_FIELD, fieldExpression.FieldName);
        }

        /// <summary>
        /// Тестирование преобразования проверки на <c>null</c>.
        /// </summary>
        [Test]
        public void TestTransformWhenIsNull()
        {
            TestTransformWhenIsNull(d => d.Name == null, true);
        }

        /// <summary>
        /// Тестирование преобразования проверки на не <c>null</c>.
        /// </summary>
        [Test]
        public void TestTransformWhenIsNotNull()
        {
            TestTransformWhenIsNull(d => d.Name != null, false);
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

        /// <summary>
        /// Тестовый тип записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;
        }
    }
}
