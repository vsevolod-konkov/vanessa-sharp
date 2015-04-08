using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

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
        public void TestTransformDataRecordGetInt32()
        {
            const string FIELD_NAME = "sort_field";

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(FIELD_NAME);

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.AreEqual(FIELD_NAME, field.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordField()
        {
            // Arrange
            const string FIELD_NAME = "id_field";
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.Id, FIELD_NAME)
                .End();

            Expression<Func<SomeData, int>> sortKeyExpression = d => d.Id;

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.AreEqual(FIELD_NAME, field.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения суммы полей типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordSum()
        {
            // Arrange
            const string PRICE_FIELD_NAME = "price";
            const string VALUE_FIELD_NAME = "value";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.Price, PRICE_FIELD_NAME)
                    .FieldMap(d => d.Value, VALUE_FIELD_NAME)
                .End();

            Expression<Func<SomeData, double>> sortKeyExpression = d => d.Price + d.Value;

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var operation = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(result);
            
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Add, operation.OperationType);

            var leftOperand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Left);
            Assert.AreEqual(PRICE_FIELD_NAME, leftOperand.FieldName);

            var rightOperand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Right);
            Assert.AreEqual(VALUE_FIELD_NAME, rightOperand.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения отрицания поля типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordNegate()
        {
            // Arrange
            const string VALUE_FIELD_NAME = "value";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.Value, VALUE_FIELD_NAME)
                .End();

            Expression<Func<SomeData, double>> sortKeyExpression = d => -d.Value;

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var operation = AssertEx.IsInstanceAndCastOf<SqlNegateExpression>(result);
            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Operand);
            Assert.AreEqual(VALUE_FIELD_NAME, operand.FieldName);
        }

        /// <summary>
        /// Тип тестовой типизированной записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public double Price;

            public double Value;
        }
    }
}
