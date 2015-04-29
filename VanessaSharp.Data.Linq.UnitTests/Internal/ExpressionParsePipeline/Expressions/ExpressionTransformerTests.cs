using System;
using System.Collections.ObjectModel;
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
    /// Тестирование <see cref="ExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionTransformerTests
    {
        private readonly Mock<IOneSMappingProvider> _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
        
        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля записи методом <see cref="OneSDataRecord.GetInt32(string)"/>.
        /// </summary>
        [Test]
        public void TestTransformDataRecordGetInt32()
        {
            const string FIELD_NAME = "sort_field";

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(FIELD_NAME);

            // Act
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(field.Table);
            Assert.AreEqual(FIELD_NAME, field.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(field.Table);
            Assert.AreEqual(FIELD_NAME, field.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var operation = AssertEx.IsInstanceAndCastOf<SqlNegateExpression>(result);
            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Operand);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(operand.Table);
            Assert.AreEqual(VALUE_FIELD_NAME, operand.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к булевому типу типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastBoolean()
        {
            TestTransfromCast<bool>(SqlTypeKind.Boolean);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к типу даты типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromCastDate()
        {
            TestTransfromCast<DateTime>(SqlTypeKind.Date);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
            return ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
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
        /// Тестирование преобразования вызова метода <see cref="string.Substring(int, int)"/>.
        /// </summary>
        [Test]
        public void TestTransformSubstring()
        {
            const string NAME_FIELD_NAME = "name";
            const int LENGTH = 2;
            const int POSITION = 5;

            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                .End();

            Expression<Func<SomeData, string>> sortKeyExpression = d => d.Name.Substring(LENGTH, POSITION);
            sortKeyExpression = PreEvaluator.Evaluate(sortKeyExpression);

            // Act
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var function = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(result);

            Assert.AreEqual(SqlEmbeddedFunction.Substring, function.Function);
            Assert.AreEqual(3, function.Arguments.Count);

            var str = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(function.Arguments[0]);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(str.Table);
            Assert.AreEqual(NAME_FIELD_NAME, str.FieldName);

            var lengthLiteral = AssertEx.IsInstanceAndCastOf<SqlLiteralExpression>(function.Arguments[1]);
            Assert.AreEqual(LENGTH, lengthLiteral.Value);

            var positionLiteral = AssertEx.IsInstanceAndCastOf<SqlLiteralExpression>(function.Arguments[2]);
            Assert.AreEqual(POSITION, positionLiteral.Value);
        }

        private ReadOnlyCollection<SqlExpression> BeginTestTransformCallDateTimeFunction<T>(
            Expression<Func<SomeData, T>> sortKeyExpression,
            SqlEmbeddedFunction expectedFunction)
        {
            const string DATE_FIELD_NAME = "birthdate";

            // Arrange
            _mappingProviderMock
                .Setup(p => p.IsDataType(It.IsAny<Type>()))
                .Returns(false);

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("X")
                    .FieldMap(d => d.BirthDate, DATE_FIELD_NAME)
                .End();

            sortKeyExpression = PreEvaluator.Evaluate(sortKeyExpression);

            // Act
            var result = ExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            var function = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(result);

            Assert.AreEqual(expectedFunction, function.Function);
            Assert.Greater(function.Arguments.Count, 0);

            var date = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(function.Arguments[0]);
            Assert.IsInstanceOf<SqlDefaultTableExpression>(date.Table);
            Assert.AreEqual(DATE_FIELD_NAME, date.FieldName);

            return function.Arguments;
        }

        private void TestTransformCallDateTimeFunction(
            Expression<Func<SomeData, int>> sortKeyExpression,
            SqlEmbeddedFunction expectedFunction)
        {

            var args = BeginTestTransformCallDateTimeFunction(sortKeyExpression, expectedFunction);
            Assert.AreEqual(1, args.Count);
        }
        
        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Year"/>.
        /// </summary>
        [Test]
        public void TestTransformYear()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Year, SqlEmbeddedFunction.Year);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetQuarter"/>.
        /// </summary>
        [Test]
        public void TestTransformGetQuarter()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetQuarter(d.BirthDate), SqlEmbeddedFunction.Quarter);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Month"/>.
        /// </summary>
        [Test]
        public void TestTransformMonth()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Month, SqlEmbeddedFunction.Month);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.DayOfYear"/>.
        /// </summary>
        [Test]
        public void TestTransformDayOfYear()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.DayOfYear, SqlEmbeddedFunction.DayOfYear);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Day"/>.
        /// </summary>
        [Test]
        public void TestTransformDay()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Day, SqlEmbeddedFunction.Day);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetWeek"/>.
        /// </summary>
        [Test]
        public void TestTransformGetWeek()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetWeek(d.BirthDate), SqlEmbeddedFunction.Week);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetDayWeek"/>.
        /// </summary>
        [Test]
        public void TestTransformGetDayWeek()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetDayWeek(d.BirthDate), SqlEmbeddedFunction.DayWeek);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Hour"/>.
        /// </summary>
        [Test]
        public void TestTransformHour()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Hour, SqlEmbeddedFunction.Hour);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Minute"/>.
        /// </summary>
        [Test]
        public void TestTransformMinute()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Minute, SqlEmbeddedFunction.Minute);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Second"/>.
        /// </summary>
        [Test]
        public void TestTransformSecond()
        {
            TestTransformCallDateTimeFunction(d => d.BirthDate.Second, SqlEmbeddedFunction.Second);
        }

        private void TestTransformCallPeriodFunction(Expression<Func<SomeData, DateTime>> sortKeyExpression,
                                                     SqlEmbeddedFunction expectedFunction,
                                                     OneSTimePeriodKind expectedKind)
        {
            var args = BeginTestTransformCallDateTimeFunction(sortKeyExpression, expectedFunction);

            Assert.AreEqual(2, args.Count);
            var periodKind = AssertEx.IsInstanceAndCastOf<SqlLiteralExpression>(args[1]);
            Assert.AreEqual(expectedKind, periodKind.Value);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.BeginOfPeriod"/>.
        /// </summary>
        [Test]
        public void TestTransformBeginOfPeriod()
        {
            TestTransformCallPeriodFunction(
                d => OneSSqlFunctions.BeginOfPeriod(d.BirthDate, OneSTimePeriodKind.Quarter),
                SqlEmbeddedFunction.BeginOfPeriod,
                OneSTimePeriodKind.Quarter
                );
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.EndOfPeriod"/>.
        /// </summary>
        [Test]
        public void TestTransformEndOfPeriod()
        {
            TestTransformCallPeriodFunction(
                d => OneSSqlFunctions.EndOfPeriod(d.BirthDate, OneSTimePeriodKind.Month),
                SqlEmbeddedFunction.EndOfPeriod,
                OneSTimePeriodKind.Month
                );
        }

        /// <summary>
        /// Тип тестовой типизированной записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public double Price;

            public double Value;

            public string Name;

            public DateTime BirthDate;

            public object AddInfo;
        }

        public sealed class AdditionalInfo
        {}
    }
}
