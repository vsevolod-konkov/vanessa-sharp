using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="QueryTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryTransformerTests : TestsBase
    {
        /// <summary>Тестирование построения SQL-запроса.</summary>
        [Test]
        public void TestBuildSql()
        {
            Assert.AreEqual(
                "SELECT field1, field2, field3 FROM source",
                QueryTransformer.BuildSql("source", new[] { "field1", "field2", "field3" })
                );
        }
        
        /// <summary>Тестирование преобразования запроса простой выборки записей.</summary>
        [Test]
        public void TestTransformSelectOneSDataRecord()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var query = new DataRecordsQuery(SOURCE_NAME);

            // Act
            var result = QueryTransformer.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса с выборкой элементов анонимного типа.
        /// </summary>
        [Test]
        public void TestTransformSelectTuple()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { StringField = r.GetString(FIRST_FIELD_NAME), IntField = r.GetInt32(SECOND_FIELD_NAME) });
            var query = CreateQuery(SOURCE_NAME, selectExpression);

            // Act
            var result = QueryTransformer.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT " + FIRST_FIELD_NAME + ", " + SECOND_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var itemTypeTrait = selectExpression.GetTraitOfOutputType();
            var product = AssertAndCastCollectionReadExpressionParseProduct(itemTypeTrait, result);
            var noSideItemReaderFactory = AssertAndCastNoSideEffectItemReaderFactory(product.ItemReaderFactory);
            var itemReader = noSideItemReaderFactory.ItemReader;

            // Тестирование полученного делегата чтения кортежа

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

        private static CustomDataTypeQuery<T> CreateQuery<T>(string source,
                                                     Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            return new CustomDataTypeQuery<T>(source, selectExpression);
        }

        private static CollectionReadExpressionParseProduct<T> AssertAndCastCollectionReadExpressionParseProduct<T>(
            Trait<T> trait, ExpressionParseProduct product)
        {
            return AssertAndCast<CollectionReadExpressionParseProduct<T>>(product);
        }

        private static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> factory)
        {
            return AssertAndCast<NoSideEffectItemReaderFactory<T>>(factory);
        }
    }
}
