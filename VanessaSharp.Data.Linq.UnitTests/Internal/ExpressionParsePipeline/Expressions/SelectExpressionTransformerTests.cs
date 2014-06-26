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
    /// <summary>
    /// Тесты на <see cref="SelectExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class SelectExpressionTransformerTests : TestsBase
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
        public void TestTransformGetXxx()
        {
            // Arrange
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = r.GetString(FIRST_FIELD_NAME), IntField = r.GetInt32(SECOND_FIELD_NAME) });

            // Act
            var result = SelectExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { new SqlFieldExpression(FIRST_FIELD_NAME), new SqlFieldExpression(SECOND_FIELD_NAME) }, 
                result.Columns);

            // Тестирование полученного делегата чтения кортежа
            var itemReader = result.SelectionFunc;

            // Arrange
            const string STRING_VALUE = "Test";
            const int INT32_VALUE = 34;

            var values = new object[] { STRING_VALUE, INT32_VALUE };
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToString(values[0]))
                .Returns(STRING_VALUE)
                .Verifiable();
            valueConverterMock
                .Setup(c => c.ToInt32(values[1]))
                .Returns(INT32_VALUE)
                .Verifiable();

            // Act
            var item = itemReader(valueConverterMock.Object, values);

            // Assert
            Assert.AreEqual(STRING_VALUE, item.StringField);
            Assert.AreEqual(INT32_VALUE, item.IntField);
            valueConverterMock
                .Verify(c => c.ToString(values[0]), Times.Once());
            valueConverterMock
                .Verify(c => c.ToInt32(values[1]), Times.Once());
        }

        /// <summary>
        /// Тестирование парсинга выражения получение экземпляра анонимного типа выборкой полей из типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransformTypedTuple()
        {
            // Arrange
            const string ID_FIELD_NAME = "Идентификатор";
            const string NAME_FIELD_NAME = "Наименование";

            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof (SomeData)))
                .Returns(
                    new OneSTypeMapping("?", new ReadOnlyCollection<OneSFieldMapping>(
                                                 new[]
                                                     {
                                                         CreateFieldMapping(d => d.Id, ID_FIELD_NAME),
                                                         CreateFieldMapping(d => d.Name, NAME_FIELD_NAME)
                                                     }))
                );

            var selectExpression = Trait.Of<SomeData>()
                                        .SelectExpression(d => new { d.Id, d.Name });

            // Act
            var result = SelectExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), selectExpression);

            // Assert
            CollectionAssert.AreEquivalent(
                new[] { new SqlFieldExpression(ID_FIELD_NAME), new SqlFieldExpression(NAME_FIELD_NAME) },
                result.Columns);

            // Тестирование полученного делегата чтения кортежа
            var itemReader = result.SelectionFunc;

            // Arrange
            const int ID_VALUE = 34;
            const string NAME_VALUE = "TestName";
            

            var values = new [] { new object(), new object() };
            
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToInt32(values[0]))
                .Returns(ID_VALUE)
                .Verifiable();
            valueConverterMock
                .Setup(c => c.ToString(values[1]))
                .Returns(NAME_VALUE)
                .Verifiable();

            // Act
            var item = itemReader(valueConverterMock.Object, values);

            // Assert
            Assert.AreEqual(ID_VALUE, item.Id);
            Assert.AreEqual(NAME_VALUE, item.Name);
            
            valueConverterMock
                .Verify(c => c.ToInt32(values[0]), Times.Once());
            valueConverterMock
                .Verify(c => c.ToString(values[1]), Times.Once());
        }

        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }

        private static OneSFieldMapping CreateFieldMapping<T>(Expression<Func<SomeData, T>> memberAccessExpression,
                                                              string fieldName)
        {
            var memberInfo = ((MemberExpression)memberAccessExpression.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }
    }
}
