using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
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

        private const string PRICE_FIELD = "price";
        private const string QUANTITY_FIELD = "quantity";
        private const string REFERENCE_FIELD = "object";

        private const string REFERENCE_TABLE = "some_table";

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
                    .FieldMap(d => d.Price, PRICE_FIELD)
                    .FieldMap(d => d.Quantity, QUANTITY_FIELD)
                    .FieldMap(d => d.Reference, REFERENCE_FIELD)
                .End();

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<RefData>(REFERENCE_TABLE)
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
            Assert.IsInstanceOf<SqlDefaultTableExpression>(fieldExpression.Table);
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

            var fieldExpression = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(isNullCondition.Expression);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(fieldExpression.Table);
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

        /// <summary>
        /// Тестирование бинарной операции разности.
        /// </summary>
        [Test]
        public void TestTransformWhenSubtractOperation()
        {
            Expression<Func<SomeData, bool>> testedFilter = d => (d.Price - d.Quantity) == d.Id;

            // Act
            var result = Transform(testedFilter);

            // Assert
            var binaryRelationCondition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(result);
            Assert.AreEqual(SqlBinaryRelationType.Equal, binaryRelationCondition.RelationType);
            
            var first = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(binaryRelationCondition.FirstOperand);
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Subtract, first.OperationType);

            var left = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(first.Left);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(left.Table);
            Assert.AreEqual(PRICE_FIELD, left.FieldName);

            var right = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(first.Right);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(right.Table);
            Assert.AreEqual(QUANTITY_FIELD, right.FieldName);
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

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(refsCondition.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(REFERENCE_FIELD, operand.FieldName);
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

            var parametersMap = _context.Parameters
                                        .GetSqlParameters()
                                        .ToDictionary(p => p.Name, p => p.Value);

            var actualValues = inValuesListCondition
                .ValuesList
                .Select(p => parametersMap[p.ParameterName])
                .ToArray();

            CollectionAssert.AreEqual(expectedValues, actualValues);

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(inValuesListCondition.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(FILTER_FIELD, operand.FieldName);
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

            public int Price;

            public int Quantity;

            public object Reference;
        }

        /// <summary>
        /// Тестовый тип записи для тестирования ссылки на нее.
        /// </summary>
        public sealed class RefData
        {
             
        }
    }
}
