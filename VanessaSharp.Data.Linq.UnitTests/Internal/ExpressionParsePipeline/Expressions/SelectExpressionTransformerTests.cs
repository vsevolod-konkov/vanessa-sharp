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
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей из типизированного кортежа.
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
        /// Тестовый тип записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }
    }
}
