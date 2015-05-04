using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Тестирование <see cref="ConditionTransformer"/>.</summary>
    [TestFixture]
    public sealed class ConditionTransformerTests : ExpressionTransformerTestsBase
    {
        private const int FILTER_VALUE = 24;

        /// <summary>
        /// Запуск тестируемого преобразования.
        /// </summary>
        /// <param name="testedFilter">Тестируемое выражение.</param>
        private SqlCondition Transform<T>(Expression<Func<T, bool>> testedFilter)
        {
            return ConditionTransformer.Transform(MappingProvider, Context, testedFilter);
        }

        /// <summary>
        /// Получение sql-параметров запроса.
        /// </summary>
        private ReadOnlyCollection<SqlParameter> GetSqlParameters()
        {
            return Context.Parameters.GetSqlParameters();
        }

        /// <summary>
        /// Проверка бинарного отношения.
        /// </summary>
        /// <param name="testedCondition">Тестируемое условие.</param>
        /// <param name="expectedRelationType">Тип сравнения.</param>
        private void AssertBinaryRelation(SqlCondition testedCondition, SqlBinaryRelationType expectedRelationType)
        {
            var parameters = GetSqlParameters();
            Assert.AreEqual(0, parameters.Count);
            
            var binaryCondition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(testedCondition);

            Assert.AreEqual(expectedRelationType, binaryCondition.RelationType);

            AssertField(ID_FIELD_NAME, binaryCondition.FirstOperand);
            AssertLiteral(FILTER_VALUE, binaryCondition.SecondOperand);
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
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordNotEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformDataRecordWhenGreater()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordGreaterOrEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordLess()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenDataRecordLessOrEqual()
        {
            TestTransformWhenDataRecordBinaryRelation(r => r.GetInt32(ID_FIELD_NAME) <= FILTER_VALUE, SqlBinaryRelationType.LessOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие равенства.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id == FILTER_VALUE, SqlBinaryRelationType.Equal);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие неравенства.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleNotEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id != FILTER_VALUE, SqlBinaryRelationType.NotEqual);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreater()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id > FILTER_VALUE, SqlBinaryRelationType.Greater);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие больше или равно.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleGreaterOrEqual()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id >= FILTER_VALUE, SqlBinaryRelationType.GreaterOrEqual);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие меньше.
        /// </summary>
        [Test]
        public void TestTransformWhenTypedTupleLess()
        {
            TestTransformWhenTypedRecordBinaryRelation(d => d.Id < FILTER_VALUE, SqlBinaryRelationType.Less);
        }

        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
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
        private void TestTransformWhenBinaryOperation(Expression<Func<SomeData, bool>> testedFilter, SqlBinaryLogicOperationType expectedOperationType)
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
            TestTransformWhenBinaryOperation(testedFilter, SqlBinaryLogicOperationType.And);
        }

        /// <summary>
        /// Тестирование бинарной операции ИЛИ.
        /// </summary>
        [Test]
        public void TestTransformWhenOrOperation()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => (d.Id <= FILTER_VALUE) || (d.Id >= FILTER_VALUE);
            TestTransformWhenBinaryOperation(testedFilter, SqlBinaryLogicOperationType.Or);
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
            AssertField(NAME_FIELD_NAME, isNullCondition.Expression);
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

        /// <summary>
        /// Тестирование бинарной операции разности.
        /// </summary>
        [Test]
        public void TestTransformWhenSubtractOperation()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => (d.Value - d.Quantity) == d.Id;

            // Act
            var result = Transform(testedFilter);

            // Assert
            var binaryRelationCondition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(result);
            Assert.AreEqual(SqlBinaryRelationType.Equal, binaryRelationCondition.RelationType);
            
            var first = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(binaryRelationCondition.FirstOperand);
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Subtract, first.OperationType);

            AssertField(VALUE_FIELD_NAME, first.Left);
            AssertField(QUANTITY_FIELD_NAME, first.Right);
        }

        /// <summary>
        /// Тестирование условия проверки ссылки.
        /// </summary>
        [Test]
        public void TestTransformWhenRefsCondition()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => d.Reference is RefData;

            // Act
            var result = Transform(testedFilter);

            // Assert
            var refsCondition = AssertEx.IsInstanceAndCastOf<SqlRefsCondition>(result);
            
            Assert.AreEqual(REFERENCE_TABLE, refsCondition.DataSourceName);
            AssertField(REF_FIELD_NAME, refsCondition.Operand);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений.
        /// </summary>
        private void TestTransformWhenInCondition(Expression<Func<SomeData, bool>> testedFilter, bool expectedIsHierarchy, IEnumerable expectedValues)
        {
            // Arrange
            testedFilter = PreEvaluator.Evaluate(testedFilter);
            
            // Act
            var result = Transform(testedFilter);

            // Assert
            var inValuesListCondition = AssertEx.IsInstanceAndCastOf<SqlInValuesListCondition>(result);
            Assert.IsTrue(inValuesListCondition.IsIn);
            Assert.AreEqual(expectedIsHierarchy, inValuesListCondition.IsHierarchy);

            var actualValues = inValuesListCondition
                .ValuesList
                .Cast<SqlLiteralExpression>()
                .Select(l => l.Value)
                .ToArray();

            CollectionAssert.AreEqual(expectedValues, actualValues);

            AssertField(ID_FIELD_NAME, inValuesListCondition.Operand);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений.
        /// </summary>
        [Test]
        public void TestTransformWhenContains()
        {
            var values = new[] {1, 2, 3, 4};
            Expression<Func<SomeData, bool>> testedFilter = d => values.Contains(d.Id);

            TestTransformWhenInCondition(testedFilter, false, values);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений с помощью метода <see cref="OneSSqlFunctions.In{T}"/>.
        /// </summary>
        [Test]
        public void TestTransformWhenIn()
        {
            var values = new[] { 1, 2, 3, 4 };
            Expression<Func<SomeData, bool>> testedFilter = d => OneSSqlFunctions.In(d.Id, 1, 2, 3, 4);

            TestTransformWhenInCondition(testedFilter, false, values);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений с помощью метода <see cref="OneSSqlFunctions.InHierarchy{T}"/>.
        /// </summary>
        [Test]
        public void TestTransformWhenInHierarchy()
        {
            var values = new[] { 1, 2, 3, 4 };
            Expression<Func<SomeData, bool>> testedFilter = d => OneSSqlFunctions.InHierarchy(d.Id, 1, 2, 3, 4);

            TestTransformWhenInCondition(testedFilter, true, values);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений с помощью метода <see cref="OneSSqlFunctions.Like"/>.
        /// </summary>
        [Test]
        public void TestTransformWhenLike()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => OneSSqlFunctions.Like(d.Name, "pattern", '_');
            testedFilter = PreEvaluator.Evaluate(testedFilter);

            var result = Transform(testedFilter);

            // Assert
            var likeCondition = AssertEx.IsInstanceAndCastOf<SqlLikeCondition>(result);
            Assert.IsTrue(likeCondition.IsLike);
            Assert.AreEqual("pattern", likeCondition.Pattern);
            Assert.AreEqual('_', likeCondition.EscapeSymbol);

            AssertField(NAME_FIELD_NAME, likeCondition.Operand);
        }

        /// <summary>
        /// Тестирование условия проверки в списке значений с помощью метода <see cref="OneSSqlFunctions.Between{T}"/>.
        /// </summary>
        [Test]
        public void TestTransformWhenBetween()
        {
            var startDate = new DateTime(2011, 01, 01);
            var endDate = new DateTime(2015, 12, 31);
            
            Expression<Func<SomeData, bool>> testedFilter = d => OneSSqlFunctions.Between(d.CreatedDate, startDate, endDate);
            testedFilter = PreEvaluator.Evaluate(testedFilter);

            var result = Transform(testedFilter);

            // Assert
            var betweenCondition = AssertEx.IsInstanceAndCastOf<SqlBetweenCondition>(result);
            Assert.IsTrue(betweenCondition.IsBetween);
            
            AssertField(CREATED_DATE_FIELD_NAME, betweenCondition.Operand);
            AssertLiteral(startDate, betweenCondition.Start);
            AssertLiteral(endDate, betweenCondition.End);
        }

        /// <summary>
        /// Тестирование условия равенства свойства <see cref="DateTime.DayOfWeek"/> значению <see cref="DayOfWeek"/>.
        /// </summary>
        [Test]
        [Ignore("Есть проблема, связанная с компилятором, который вставляет узел преобразования.")]
        public void TestTransformWhenEqualsDayOfWeek()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => d.CreatedDate.DayOfWeek == DayOfWeek.Wednesday;

            // Act
            var result = Transform(testedFilter);

            // Assert
            var equals = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(result);

            Assert.AreEqual(SqlBinaryRelationType.Equal, equals.RelationType);
            var left = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(equals.FirstOperand);

            Assert.AreEqual(SqlEmbeddedFunction.DayWeek, left.Function);
            Assert.AreEqual(1, left.Arguments.Count);

            AssertField(CREATED_DATE_FIELD_NAME, left.Arguments[0]);
            AssertLiteral(3, equals.SecondOperand);
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
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
            var result = Transform(testedFilter);
            var parameters = context.Parameters.GetSqlParameters();

            // Assert
            // ?
        }

        // TODO Надо подумать о желаемом поведении"
        /// <summary>
        /// Тестирование <see cref="ConditionTransformer.Transform{T}"/>
        /// в случае если в обрабатывается условие получения булевого значения поля.
        /// </summary>
        [Ignore("Надо подумать о желаемом поведении")]
        [Test]
        public void TestTransformWhenDataRecordGetBooleanField()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> testedFilter = r => r.GetBoolean(ID_FIELD_NAME);

            // Act
            var result = Transform(testedFilter);
            var parameters = GetSqlParameters();

            // Assert
            // ?
        }
    }
}
