using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
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
    public sealed class ExpressionTransformerTests : ExpressionTransformerTestsBase
    {
        private SqlExpression Transform<T, TResult>(Expression<Func<T, TResult>> testedExpression)
        {
            testedExpression = PreEvaluator.Evaluate(testedExpression);
            return ExpressionTransformer.Transform(MappingProvider, Context, testedExpression);
        }
        
        private SqlExpression Transform<TResult>(Expression<Func<OneSDataRecord, TResult>> testedExpression)
        {
            return Transform<OneSDataRecord, TResult>(testedExpression);
        }
        
        private SqlExpression Transform<TResult>(Expression<Func<SomeData, TResult>> testedExpression)
        {
            return Transform<SomeData, TResult>(testedExpression);
        }
        
        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля записи методом <see cref="OneSDataRecord.GetInt32(string)"/>.
        /// </summary>
        [Test]
        public void TestTransformDataRecordGetInt32()
        {
            // Arrange
            const string FIELD_NAME = "some_field";

            // Act
            var result = Transform(r => r.GetInt32(FIELD_NAME));

            // Assert
            AssertField(FIELD_NAME, result);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordField()
        {
            // Act
            var result = Transform(d => d.Id);

            // Assert
            AssertField(ID_FIELD_NAME, result);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения суммы полей типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordSum()
        {
            // Act
            var result = Transform(d => d.Quantity + d.Value);

            // Assert
            var operation = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(result);
            
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Add, operation.OperationType);
            AssertField(QUANTITY_FIELD_NAME, operation.Left);
            AssertField(VALUE_FIELD_NAME, operation.Right);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения отрицания поля типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedRecordNegate()
        {
            // Act
            var result = Transform(d => -d.Value);

            // Assert
            var operation = AssertEx.IsInstanceAndCastOf<SqlNegateExpression>(result);
            AssertField(VALUE_FIELD_NAME, operation.Operand);
        }

        /// <summary>
        /// Тестирование <see cref="ExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения приведения поля к заданному типу типизированного кортежа.
        /// </summary>
        private void TestTransfromCast<T>(SqlTypeKind expectedSqlType, Action<SqlTypeDescription> assertType = null)
        {
            // Act
            var result = Transform(d => (T)d.AddInfo);

            // Assert
            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(result);
            AssertField(ADD_INFO_FIELD_NAME, castOperation.Operand);
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
            // Act & Assert
            TestTransfromCast<AdditionalInfo>(
                SqlTypeKind.Table,
                actualType =>
                {
                    var tableType = AssertEx.IsInstanceAndCastOf<SqlTableTypeDescription>(actualType);
                    
                    Assert.AreEqual(ADD_INFO_TABLE_NAME, tableType.TableName);
                });
        }

        private SqlTypeDescription TestTransfromSqlFunctionsTo<T>(
            Expression<Func<SomeData, T>> testedExpression,
            SqlTypeKind expectedTypeKind)
        {
            // Act
            var result = Transform(testedExpression);

            // Assert
            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(result);

            AssertField(ADD_INFO_FIELD_NAME, castOperation.Operand);
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
            const string TABLE_NAME = "ref_data";
            const string FIELD_NAME = "some_field";

            // Act
            var result = Transform(d => OneSSqlFunctions.ToDataRecord(d.AddInfo, TABLE_NAME).GetString(FIELD_NAME));

            // Assert
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result);
            Assert.AreEqual(FIELD_NAME, field.FieldName);

            var castOperation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(field.Table);
            AssertField(ADD_INFO_FIELD_NAME, castOperation.Operand);
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
            const int LENGTH = 2;
            const int POSITION = 5;

            // Act
            var result = Transform(d => d.Name.Substring(LENGTH, POSITION));

            // Assert
            var function = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(result);

            Assert.AreEqual(SqlEmbeddedFunction.Substring, function.Function);
            Assert.AreEqual(3, function.Arguments.Count);

            AssertField(NAME_FIELD_NAME, function.Arguments[0]);
            AssertLiteral(LENGTH, function.Arguments[1]);
            AssertLiteral(POSITION, function.Arguments[2]);
        }

        private ReadOnlyCollection<SqlExpression> BeginTestTransformCallDateTimeFunction<T>(
            Expression<Func<SomeData, T>> testedExpression,
            SqlEmbeddedFunction expectedFunction)
        {
            // Act
            var result = Transform(testedExpression);

            // Assert
            var function = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(result);

            Assert.AreEqual(expectedFunction, function.Function);
            Assert.Greater(function.Arguments.Count, 0);
            AssertField(CREATED_DATE_FIELD_NAME, function.Arguments[0]);

            return function.Arguments;
        }

        private void TestTransformCallDateTimeFunction(
            Expression<Func<SomeData, int>> testedExpression,
            SqlEmbeddedFunction expectedFunction)
        {

            var args = BeginTestTransformCallDateTimeFunction(testedExpression, expectedFunction);
            Assert.AreEqual(1, args.Count);
        }
        
        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Year"/>.
        /// </summary>
        [Test]
        public void TestTransformYear()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Year, SqlEmbeddedFunction.Year);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetQuarter"/>.
        /// </summary>
        [Test]
        public void TestTransformGetQuarter()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetQuarter(d.CreatedDate), SqlEmbeddedFunction.Quarter);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Month"/>.
        /// </summary>
        [Test]
        public void TestTransformMonth()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Month, SqlEmbeddedFunction.Month);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.DayOfYear"/>.
        /// </summary>
        [Test]
        public void TestTransformDayOfYear()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.DayOfYear, SqlEmbeddedFunction.DayOfYear);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Day"/>.
        /// </summary>
        [Test]
        public void TestTransformDay()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Day, SqlEmbeddedFunction.Day);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetWeek"/>.
        /// </summary>
        [Test]
        public void TestTransformGetWeek()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetWeek(d.CreatedDate), SqlEmbeddedFunction.Week);
        }

        /// <summary>
        /// Тестирование преобразования вызова метода <see cref="OneSSqlFunctions.GetDayWeek"/>.
        /// </summary>
        [Test]
        public void TestTransformGetDayWeek()
        {
            TestTransformCallDateTimeFunction(d => OneSSqlFunctions.GetDayWeek(d.CreatedDate), SqlEmbeddedFunction.DayWeek);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Hour"/>.
        /// </summary>
        [Test]
        public void TestTransformHour()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Hour, SqlEmbeddedFunction.Hour);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Minute"/>.
        /// </summary>
        [Test]
        public void TestTransformMinute()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Minute, SqlEmbeddedFunction.Minute);
        }

        /// <summary>
        /// Тестирование преобразования вызова свойства <see cref="DateTime.Second"/>.
        /// </summary>
        [Test]
        public void TestTransformSecond()
        {
            TestTransformCallDateTimeFunction(d => d.CreatedDate.Second, SqlEmbeddedFunction.Second);
        }

        private void TestTransformCallPeriodFunction(Expression<Func<SomeData, DateTime>> testedExpression,
                                                     SqlEmbeddedFunction expectedFunction,
                                                     OneSTimePeriodKind expectedKind)
        {
            var args = BeginTestTransformCallDateTimeFunction(testedExpression, expectedFunction);

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
                d => OneSSqlFunctions.BeginOfPeriod(d.CreatedDate, OneSTimePeriodKind.Quarter),
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
                d => OneSSqlFunctions.EndOfPeriod(d.CreatedDate, OneSTimePeriodKind.Month),
                SqlEmbeddedFunction.EndOfPeriod,
                OneSTimePeriodKind.Month
                );
        }

        /// <summary>
        /// Тестирование преобразования выражения ??
        /// </summary>
        [Test]
        public void TestTransformCoalesce()
        {
            // Act
            var result = Transform(d => d.Name ?? "Test");

            // Assert
            var caseExpression = AssertEx.IsInstanceAndCastOf<SqlCaseExpression>(result);

            Assert.AreEqual(1, caseExpression.Cases.Count);

            var condition = AssertEx.IsInstanceAndCastOf<SqlIsNullCondition>(caseExpression.Cases[0].Condition);
            Assert.IsTrue(condition.IsNull);
            AssertField(NAME_FIELD_NAME, condition.Expression);
            
            AssertLiteral("Test", caseExpression.Cases[0].Value);

            Assert.IsTrue(condition.Expression.Equals(caseExpression.DefaultValue));
        }

        /// <summary>
        /// Тестирование преобразования выражения тернарного оператора.
        /// </summary>
        [Test]
        public void TestTransformConditional()
        {
            // Act
            var result = Transform(d => d.Price > 20.5m ? d.Name : "Test");

            // Assert
            var caseExpression = AssertEx.IsInstanceAndCastOf<SqlCaseExpression>(result);

            Assert.AreEqual(1, caseExpression.Cases.Count);

            var condition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(caseExpression.Cases[0].Condition);

            Assert.AreEqual(SqlBinaryRelationType.Greater, condition.RelationType);

            AssertField(PRICE_FIELD_NAME, condition.FirstOperand);
            AssertLiteral(20.5m, condition.SecondOperand);

            AssertField(NAME_FIELD_NAME, caseExpression.Cases[0].Value);
            
            AssertLiteral("Test", caseExpression.DefaultValue);
        }
    }
}

