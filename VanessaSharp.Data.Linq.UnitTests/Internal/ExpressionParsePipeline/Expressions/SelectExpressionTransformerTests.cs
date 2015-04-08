using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тесты на <see cref="SelectExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class SelectExpressionTransformerTests
    {
        private Mock<IOneSMappingProvider> _mappingProviderMock;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
        }

        /// <summary>
        /// Тестирование выдачи исключения в случае если в выходной структуре используется целая запись.
        /// </summary>
        [Test]
        public void TestInvalidTransformWhenUseWholeParameter()
        {
            // Arrange
            var selectExpression =
                Trait.Of<OneSDataRecord>().SelectExpression(r => new {Id = r.GetInt32("Id"), Record = r});

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => SelectExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression));

            // Assert
            Assert.AreEqual(
                "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.",
                exception.Message
                );
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

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = r.GetString(FIRST_FIELD_NAME), IntField = r.GetInt32(SECOND_FIELD_NAME) });

            // Act
            var result = SelectExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEqual(
                new[] { new SqlFieldExpression(FIRST_FIELD_NAME), new SqlFieldExpression(SECOND_FIELD_NAME) }, 
                result.Columns);

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

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { FirstField = r.GetValue(FIRST_FIELD_NAME), SecondField = r.GetValue(SECOND_FIELD_NAME) });

            // Act
            var result = SelectExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEqual(
                new[] { new SqlFieldExpression(FIRST_FIELD_NAME), new SqlFieldExpression(SECOND_FIELD_NAME) },
                result.Columns);

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
            // Arrange
            const string ID_FIELD_NAME = "Идентификатор";
            const string NAME_FIELD_NAME = "Наименование";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("?")
                    .FieldMap(d => d.Id, ID_FIELD_NAME)
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                .End();

            var selectExpression = Trait
                .Of<SomeData>()
                .SelectExpression(d => new { d.Id, d.Name });

            // Act
            var result = SelectExpressionTransformer
                .Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { new SqlFieldExpression(ID_FIELD_NAME), new SqlFieldExpression(NAME_FIELD_NAME) },
                result.Columns);

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
            // Arrange
            const string ID_FIELD_NAME = "Идентификатор";
            const string NAME_FIELD_NAME = "Наименование";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeDataWithWeakTyping>("?")
                    .FieldMap(d => d.Id, ID_FIELD_NAME)
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                .End();

            var selectExpression = Trait
                .Of<SomeDataWithWeakTyping>()
                .SelectExpression(d => new { d.Id, d.Name });

            // Act
            var result = SelectExpressionTransformer
                .Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { new SqlFieldExpression(ID_FIELD_NAME), new SqlFieldExpression(NAME_FIELD_NAME) },
                result.Columns);

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
            // Arrange
            const string PRICE_FIELD_NAME = "цена";
            const string VALUE_FIELD_NAME = "значение";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("?")
                    .FieldMap(d => d.Name, "наименование")
                    .FieldMap(d => d.Price, PRICE_FIELD_NAME)
                    .FieldMap(d => d.Value, VALUE_FIELD_NAME)
                .End();

            var selectExpression = Trait
                .Of<SomeData>()
                .SelectExpression(d => new { Name = d.Name, Summa = d.Price * d.Value });

            // Act
            var result = SelectExpressionTransformer
                .Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            Assert.AreEqual(2, result.Columns.Count);
            Assert.IsInstanceOf<SqlFieldExpression>(result.Columns[0]);

            var operation = AssertEx.IsInstanceAndCastOf<SqlBinaryOperationExpression>(result.Columns[1]);
            Assert.AreEqual(SqlBinaryArithmeticOperationType.Multiply, operation.OperationType);

            var left = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Left);
            Assert.AreEqual(PRICE_FIELD_NAME, left.FieldName);

            var right = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Right);
            Assert.AreEqual(VALUE_FIELD_NAME, right.FieldName);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, i => i.Name, c => c.ToString(null), "Test")
                    .Field(1, i => i.Summa, c => c.ToDecimal(null), 354.78m)
                .Test();
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра
        /// анонимного типа выборкой отрицания поля типизированной записи.
        /// </summary>
        [Test]
        public void TestTransformSelectorWithNegate()
        {
            // Arrange
            const string VALUE_FIELD_NAME = "значение";

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("?")
                    .FieldMap(d => d.Value, VALUE_FIELD_NAME)
                .End();

            var selectExpression = Trait
                .Of<SomeData>()
                .SelectExpression(d => -d.Value);

            // Act
            var result = SelectExpressionTransformer
                .Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            Assert.AreEqual(1, result.Columns.Count);

            var operation = AssertEx.IsInstanceAndCastOf<SqlNegateExpression>(result.Columns[0]);

            var operand = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(operation.Operand);
            Assert.AreEqual(VALUE_FIELD_NAME, operand.FieldName);

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester
                .For(result.SelectionFunc, 1)
                    .Field(0, i => i, c => c.ToDecimal(null), 354.78m)
                .Test();
        }

        #region Тестовые типы

        /// <summary>
        /// Тестовый тип записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;

            public decimal Value;
        }

        /// <summary>
        /// Тестовый тип записи со слаботипизированными полями.
        /// </summary>
        public sealed class SomeDataWithWeakTyping
        {
            public object Id;

            public OneSValue Name;

            public decimal Price;
        }


        #endregion
    }
}
