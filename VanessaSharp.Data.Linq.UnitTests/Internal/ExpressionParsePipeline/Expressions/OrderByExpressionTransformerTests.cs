using System;
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
            Assert.IsInstanceOf<SqlDefaultTableExpression>(field.Table);
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
            Assert.IsInstanceOf<SqlDefaultTableExpression>(field.Table);
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
            Assert.IsInstanceOf<SqlDefaultTableExpression>(leftOperand.Table);
            Assert.AreEqual(PRICE_FIELD_NAME, leftOperand.FieldName);

            var rightOperand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Right);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(rightOperand.Table);
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
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(VALUE_FIELD_NAME, operand.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к заданному типу типизированного кортежа.
        /// </summary>
        private void TestTransfromCast<T>(SqlTypeKind expectedSqlType, Action<SqlTypeDescription> assertType = null)
        {
            // Arrange
            const string INFO_FIELD_NAME = "value";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.AddInfo, INFO_FIELD_NAME)
                .End();

            Expression<Func<SomeData, T>> sortKeyExpression = d => (T)d.AddInfo;

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(result);

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(castOperation.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(INFO_FIELD_NAME, operand.FieldName);

            Assert.AreEqual(expectedSqlType, castOperation.SqlType.Kind);

            if (assertType != null)
                assertType(castOperation.SqlType);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к булевому типу типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastBoolean()
        {
            TestTransfromCast<bool>(SqlTypeKind.Boolean);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к типу даты типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastDate()
        {
            TestTransfromCast<DateTime>(SqlTypeKind.Date);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к строковому типу типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastString()
        {
            TestTransfromCast<string>(
                SqlTypeKind.String,
                actualType =>
                    {
                        var stringType = AssertEx.IsInstanceAndCastOf<SqlStringTypeDescription>(actualType);
                        Assert.IsNull(stringType.Length);
                    });
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к числовому типу типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastInt()
        {
            TestTransfromCast<int>(
                SqlTypeKind.Number,
                actualType =>
                {
                    var numberType = AssertEx.IsInstanceAndCastOf<SqlNumberTypeDescription>(actualType);
                    Assert.IsNull(numberType.Length);
                    Assert.IsNull(numberType.Precision);
                });
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к числовому типу типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastOtherTypedRecord()
        {
            // Arrange
            const string ADD_INFO_TABLE_NAME = "add_info";
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<AdditionalInfo>(ADD_INFO_TABLE_NAME)
                .End();

            // Act & Assert
            TestTransfromCast<AdditionalInfo>(
                SqlTypeKind.Table,
                actualType =>
                {
                    var tableType = AssertEx.IsInstanceAndCastOf<SqlTableTypeDescription>(actualType);
                    
                    Assert.AreEqual(ADD_INFO_TABLE_NAME, tableType.TableName);
                });
        }

        private SqlExpression ArrangeAndActForTestTransfromSqlFunctionsTo<T>(
            string infoFieldName,
            Expression<Func<SomeData, T>> sortKeyExpression)
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.AddInfo, infoFieldName)
                .End();

            sortKeyExpression = PreEvaluator.Evaluate(sortKeyExpression);

            // Act
            return OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);
        }

        private SqlTypeDescription TestTransfromSqlFunctionsTo<T>(
            Expression<Func<SomeData, T>> sortKeyExpression,
            SqlTypeKind expectedTypeKind)
        {
            const string INFO_FIELD_NAME = "value";

            // Arrange & Act
            var result = ArrangeAndActForTestTransfromSqlFunctionsTo(
                INFO_FIELD_NAME, sortKeyExpression);

            // Assert
            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(result);

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(castOperation.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(INFO_FIELD_NAME, operand.FieldName);

            Assert.AreEqual(expectedTypeKind, castOperation.SqlType.Kind);

            return castOperation.SqlType;
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передан вызов метода <see cref="OneSSqlFunctions.ToInt64{T}"/>.
        /// </summary>
        [Test]
        public void TestTransfromSqlFunctionsToInt64()
        {
            const int NUMBER_LENGHT = 20;

            // Arrange & Act
            var result = TestTransfromSqlFunctionsTo(
                d => OneSSqlFunctions.ToInt64(d.AddInfo, NUMBER_LENGHT),
                SqlTypeKind.Number);

            // Assert
            var numberType = AssertEx.IsInstanceAndCastOf<SqlNumberTypeDescription>(result);
            Assert.AreEqual(NUMBER_LENGHT, numberType.Length);
            Assert.IsNull(numberType.Precision);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передан вызов метода <see cref="OneSSqlFunctions.ToDecimal{T}"/>.
        /// </summary>
        [Test]
        public void TestTransfromSqlFunctionsToDecimal()
        {
            const int NUMBER_LENGHT = 10;
            const int NUMBER_PRECISION = 2;

            // Arrange & Act
            var result = TestTransfromSqlFunctionsTo(
                d => OneSSqlFunctions.ToDecimal(d.AddInfo, NUMBER_LENGHT, NUMBER_PRECISION),
                SqlTypeKind.Number);

            // Assert
            var numberType = AssertEx.IsInstanceAndCastOf<SqlNumberTypeDescription>(result);
            Assert.AreEqual(NUMBER_LENGHT, numberType.Length);
            Assert.AreEqual(NUMBER_PRECISION, numberType.Precision);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передан вызов метода <see cref="OneSSqlFunctions.ToString{T}"/>.
        /// </summary>
        [Test]
        public void TestTransfromSqlFunctionsToString()
        {
            // Arrange
            const int STRING_LENGTH = 100;

            // Arrange & Act
            var result = TestTransfromSqlFunctionsTo(
                d => OneSSqlFunctions.ToString(d.AddInfo, STRING_LENGTH),
                SqlTypeKind.String);

            // Assert
            var stringType = AssertEx.IsInstanceAndCastOf<SqlStringTypeDescription>(result);
            Assert.AreEqual(STRING_LENGTH, stringType.Length);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передан вызов метода <see cref="OneSSqlFunctions.ToDataRecord{T}"/>.
        /// </summary>
        [Test]
        public void TestTransfromSqlFunctionsToDataRecord()
        {
            // Arrange
            const string INFO_FIELD_NAME = "value";
            const string TABLE_NAME = "ref_data";
            const string NAME_FIELD_NAME = "name";

            // Arrange & Act
            var result = ArrangeAndActForTestTransfromSqlFunctionsTo(
                INFO_FIELD_NAME,
                d => OneSSqlFunctions.ToDataRecord(d.AddInfo, TABLE_NAME).GetString(NAME_FIELD_NAME));

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.AreEqual(NAME_FIELD_NAME, field.FieldName);

            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(field.Table);

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(castOperation.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(INFO_FIELD_NAME, operand.FieldName);

            Assert.AreEqual(SqlTypeKind.Table, castOperation.SqlType.Kind);

            var tableType = AssertEx.IsInstanceAndCastOf<SqlTableTypeDescription>(castOperation.SqlType);
            Assert.AreEqual(TABLE_NAME, tableType.TableName);
        }

        /// <summary>
        /// Тип тестовой типизированной записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public double Price;

            public double Value;

            public object AddInfo;
        }

        public sealed class AdditionalInfo
        {}
    }
}
