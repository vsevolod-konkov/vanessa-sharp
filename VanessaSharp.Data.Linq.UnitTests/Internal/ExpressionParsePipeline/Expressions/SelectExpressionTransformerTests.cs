using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Тесты на <see cref="SelectExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class SelectExpressionTransformerTests : ExpressionTransformerTestsBase
    {
        private SelectionPartParseProduct<TResult> Transform<T, TResult>(Expression<Func<T, TResult>> testedExpression)
        {
            testedExpression = PreEvaluator.Evaluate(testedExpression);

            return SelectExpressionTransformer.Transform(MappingProvider, Context, testedExpression);
        }

        private SelectionPartParseProduct<TResult> Transform<TResult>(Expression<Func<OneSDataRecord, TResult>> testedExpression)
        {
            return Transform<OneSDataRecord, TResult>(testedExpression);
        }

        private SelectionPartParseProduct<TResult> Transform<TResult>(Expression<Func<SomeData, TResult>> testedExpression)
        {
            return Transform<SomeData, TResult>(testedExpression);
        }

            /// <summary>
        /// Тестирование выдачи исключения в случае если в выходной структуре используется целая запись.
        /// </summary>
        [Test]
        public void TestInvalidTransformWhenUseWholeParameter()
        {
            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => Transform(r => new { Id = r.GetInt32("Id"), Record = r }));

            // Assert
            Assert.AreEqual(
                "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.",
                exception.Message
                );
        }

        private static void AssertFields(IList<SqlExpression> testedExpressions, params string[] expectedFields)
        {
            Assert.AreEqual(expectedFields.Length, testedExpressions.Count);

            for (var index = 0; index < expectedFields.Length; index++)
                AssertField(expectedFields[index], testedExpressions[index]);
        }
        
        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей из записи.
        /// </summary>
        [Test]
        public void TestTransformDataRecordGetXxx()
        {
            // Arrange
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            // Act
            var result = Transform(r => new { StringField = r.GetString(FIRST_FIELD_NAME), IntField = r.GetInt32(SECOND_FIELD_NAME) });

            // Assert
            AssertFields(result.Columns, FIRST_FIELD_NAME, SECOND_FIELD_NAME);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.StringField, c => c.ToString(null), "Test")
                    .Field(1, i => i.IntField, c => c.ToInt32(null), 34)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей из записи.
        /// </summary>
        [Test]
        public void TestTransformDataRecordGetValue()
        {
            // Arrange
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            // Act
            var result = Transform(r => new { FirstField = r.GetValue(FIRST_FIELD_NAME), SecondField = r.GetValue(SECOND_FIELD_NAME) });

            // Assert
            AssertFields(result.Columns, FIRST_FIELD_NAME, SECOND_FIELD_NAME);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => (string)i.FirstField, c => c.ToString(null), "Test")
                    .Field(1, i => (int)i.SecondField, c => c.ToInt32(null), 34)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorTypedRecord()
        {
            // Act
            var result = Transform(d => new { d.Id, d.Name });

            // Assert
            AssertFields(result.Columns, ID_FIELD_NAME, NAME_FIELD_NAME);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.Id, c => c.ToInt32(null), 34)    
                    .Field(1, i => i.Name, c => c.ToString(null), "Test")
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой полей типизированной записи
        /// со слаботипизированными полями типа <see cref="object"/>
        /// и <see cref="OneSValue"/>.
        /// </summary>
        [Test]
        public void TestTransformSelectorTypedRecordWithWeakTyping()
        {
            var selectExpression = Trait
                .Of<SomeDataWithWeakTyping>()
                .SelectExpression(d => new { d.Id, d.Name });

            // Act
            var result = Transform(selectExpression);

            // Assert
            AssertFields(result.Columns, ID_FIELD_NAME, NAME_FIELD_NAME);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => (int)(OneSValue)i.Id, c => c.ToInt32(null), 34)
                    .Field(1, i => (string)i.Name, c => c.ToString(null), "Test")
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой произведения полей типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithMultiply()
        {
            // Act
            var result = Transform(d => new { d.Name, Summa = d.Quantity * d.Value });

            // Assert
            Assert.AreEqual(2, result.Columns.Count);

            AssertField(NAME_FIELD_NAME, result.Columns[0]);

            var operation = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(result.Columns[1]);
            
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Multiply, operation.OperationType);
            AssertField(QUANTITY_FIELD_NAME, operation.Left);
            AssertField(VALUE_FIELD_NAME, operation.Right);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.Name, c => c.ToString(null), "Test")
                    .Field(1, i => i.Summa, c => c.ToInt32(null), 35478)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой отрицания поля типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithNegate()
        {
            // Act
            var result = Transform(d => -d.Value);

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var operation = AssertEx.IsInstanceAndCastOf<SqlNegateExpression>(result.Columns[0]);
            AssertField(VALUE_FIELD_NAME, operation.Operand);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 1)
                    .Field(0, i => i, c => c.ToInt32(null), 35478)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой приведения к заданному типу типизированной записи.
        /// </summary>
        private void TestTransformSelectorWithCast<T>(SqlTypeKind expectedTypeKind, Expression<Func<IValueConverter, T>> convertMethod, T expectedValue)
        {
            // Act
            var result = Transform(d => (T)d.AddInfo);

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var operation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(result.Columns[0]);

            AssertField(ADD_INFO_FIELD_NAME, operation.Operand);
            Assert.AreEqual(expectedTypeKind, operation.SqlType.Kind);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 1)
                    .Field(0, i => i, convertMethod, expectedValue)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой приведения к булевому типу типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithBooleanCast()
        {
            TestTransformSelectorWithCast(SqlTypeKind.Boolean, c => c.ToBoolean(null), true);
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой приведения к типу даты типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithDateTimeCast()
        {
            TestTransformSelectorWithCast(SqlTypeKind.Date, c => c.ToDateTime(null), DateTime.Today);
        }

        /// <summary>
        /// Тестирование парсинга выборки привдения к записи на которую ссылается другая запись.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithTableCast()
        {
            // Act
            var result = Transform(d => (RefData)d.AddInfo);

            // Assert
            Assert.AreEqual(2, result.Columns.Count);

            var expectedFieldNames = new[] {NAME_FIELD_NAME, PRICE_FIELD_NAME};
            for (var i = 0; i < result.Columns.Count; i++)
            {
                var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(result.Columns[i]);
                Assert.AreEqual(expectedFieldNames[i], field.FieldName);

                var operation = AssertEx.IsInstanceAndCastOf<SqlCastExpression>(field.Table);

                Assert.AreEqual(SqlTypeKind.Table, operation.SqlType.Kind);
                var table = AssertEx.IsInstanceAndCastOf<SqlTableTypeDescription>(operation.SqlType);
                Assert.AreEqual(REFERENCE_TABLE, table.TableName);

                AssertField(ADD_INFO_FIELD_NAME, operation.Operand);
            }

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.Name, c => c.ToString(null), "test")
                    .Field(1, i => i.Price, c => c.ToDecimal(null), 23434.43m)
                .Test();
        }

        /// <summary>
        /// Тестирование <see cref="DateTime.DayOfWeek"/>
        /// </summary>
        [Test]
        public void TestTransformSelectorWithDayOfWeek()
        {
            // Act
            var result = Transform(d => d.CreatedDate.DayOfWeek);

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var function = AssertEx.IsInstanceAndCastOf<SqlEmbeddedFunctionExpression>(result.Columns[0]);
            Assert.AreEqual(SqlEmbeddedFunction.DayWeek, function.Function);
            Assert.AreEqual(1, function.Arguments.Count);

            AssertField(CREATED_DATE_FIELD_NAME, function.Arguments[0]);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 1)
                    .Field(0, i => i, c => c.ToInt32(null), 3, DayOfWeek.Wednesday)
                .Test();
        }

        /// <summary>
        /// Тестирование преобразования тернарного оператора выбора.
        /// </summary>
        [Test]
        public void TestTransformConditional()
        {
            // Act
            var result = Transform(d => (d.Price > 10.5m) ? d.Value : 14);

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var caseExpression = AssertEx.IsInstanceAndCastOf<SqlCaseExpression>(result.Columns[0]);
            Assert.AreEqual(1, caseExpression.Cases.Count);

            var condition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(caseExpression.Cases[0].Condition);
            
            AssertField(PRICE_FIELD_NAME, condition.FirstOperand);
            Assert.AreEqual(SqlBinaryRelationType.Greater, condition.RelationType);
            AssertLiteral(10.5m, condition.SecondOperand);

            AssertField(VALUE_FIELD_NAME, caseExpression.Cases[0].Value);
            AssertLiteral(14, caseExpression.DefaultValue);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 1)
                    .Field(0, i => i, c => c.ToInt32(null), 14324)
                .Test();
        }

        /// <summary>
        /// Тестирование преобразования тернарного оператора выбора.
        /// </summary>
        [Test]
        public void TestTransformSelectBooleanColumn()
        {
            // Act
            var result = Transform(d => new { HasOverDraft = (d.Price > 10.5m), d.Value });

            // Assert
            Assert.AreEqual(2, result.Columns.Count);

            var caseExpression = AssertEx.IsInstanceAndCastOf<SqlCaseExpression>(result.Columns[0]);
            Assert.AreEqual(1, caseExpression.Cases.Count);

            var condition = AssertEx.IsInstanceAndCastOf<SqlBinaryRelationCondition>(caseExpression.Cases[0].Condition);
            AssertField(PRICE_FIELD_NAME, condition.FirstOperand);
            Assert.AreEqual(SqlBinaryRelationType.Greater, condition.RelationType);
            AssertLiteral(10.5m, condition.SecondOperand);

            AssertLiteral(true, caseExpression.Cases[0].Value);
            
            AssertLiteral(false, caseExpression.DefaultValue);

            AssertField(VALUE_FIELD_NAME, result.Columns[1]);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.HasOverDraft, c => c.ToBoolean(null), true)
                    .Field(1, i => i.Value, c => c.ToInt32(null), 45)
                .Test();
        }

        /// <summary>
        /// Тестирование преобразования выборки нескольких столбцов из табличной части.
        /// </summary>
        [Test]
        public void TestTransformSelectTablePartWithSelectFewFields()
        {
            // Arrange
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            valueConverterMock
                .Setup(c => c.ToInt32(It.IsAny<object>()))
                .Returns<object>(o => (int)o);
            valueConverterMock
                .Setup(c => c.ToString(It.IsAny<object>()))
                .Returns<object>(o => (string)o);

            var sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);

            sqlResultReaderMock
                .Setup(r => r.Dispose());

            sqlResultReaderMock
                .Setup(r => r.Read())
                .Returns(true);

            sqlResultReaderMock
                .Setup(r => r.FieldCount)
                .Returns(2);

            sqlResultReaderMock
                .Setup(r => r.ValueConverter)
                .Returns(valueConverterMock.Object);

            const int expectedId = 1;
            const string expectedName = "Test";

            sqlResultReaderMock
                .Setup(r => r.GetValues(It.IsAny<object[]>()))
                .Callback<object[]>(b =>
                {
                    b[0] = expectedId;
                    b[1] = expectedName;
                });
            
            // Act
            var result =
                Transform(
                    r =>
                    r.GetTablePartRecords("TablePart")
                     .Select(p => new {Id = p.GetInt32("Id"), Name = p.GetString("Name")}));

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var fieldsGroup = AssertEx.IsInstanceAndCastOf<SqlFieldsGroupExpression>(result.Columns[0]);

            AssertField("TablePart", fieldsGroup.Table);

            Assert.AreEqual(2, fieldsGroup.Fields.Count);
            AssertFields(fieldsGroup.Fields, "Id", "Name");

            var items = result.SelectionFunc(valueConverterMock.Object, new object[] {sqlResultReaderMock.Object});
            var item = items.First();

            Assert.AreEqual(expectedId, item.Id);
            Assert.AreEqual(expectedName, item.Name);
        }
    }
}
