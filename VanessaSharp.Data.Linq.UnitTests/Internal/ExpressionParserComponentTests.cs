using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тесты на экземпляр <see cref="ExpressionParser"/>.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionParserComponentTests : TestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private readonly ExpressionParser _testedInstance = ExpressionParser.Default;

        private static CollectionReadExpressionParseProduct<T> AssertAndCastCollectionReadExpressionParseProduct<T>(
            Trait<T> trait, ExpressionParseProduct product)
        {
            return AssertAndCast<CollectionReadExpressionParseProduct<T>>(product);
        }

        private static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
            Trait<T> trait, IItemReaderFactory<T> itemReaderFactory)
        {
            return AssertAndCast<NoSideEffectItemReaderFactory<T>>(itemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает целые записи.</summary>
        [Test]
        public void TestParseWhenSelectRecords()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string FILTER_FIELD_NAME = "[filter_field]";
            const string FILTER_VALUE = "[filter_value]";

            const string SORT_FIELD_1_NAME = "[sort_field_1]";
            const string SORT_FIELD_2_NAME = "[sort_field_2]";
            const string SORT_FIELD_3_NAME = "[sort_field_3]";

            var testedExpression = TestHelperQueryProvider
                                .BuildTestQueryExpression(SOURCE_NAME,
                                q => q
                                    .Where(r => r.GetString(FILTER_FIELD_NAME) == FILTER_VALUE)
                                    .OrderBy(r => r.GetInt32(SORT_FIELD_1_NAME))
                                    .OrderByDescending(r => r.GetString(SORT_FIELD_2_NAME))
                                    .ThenBy(r => r.GetInt32(SORT_FIELD_3_NAME))
                                );

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(1, command.Parameters.Count);
            Assert.AreEqual(FILTER_VALUE, command.Parameters[0].Value);

            var parameterName = command.Parameters[0].Name;
            Assert.AreEqual(
                expected: "SELECT * FROM " + SOURCE_NAME + " WHERE " + FILTER_FIELD_NAME + " = &" + parameterName + " ORDER BY " + SORT_FIELD_2_NAME + " DESC, " + SORT_FIELD_3_NAME,
                actual: command.Sql
                );

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает избранные поля в кортеж анонимного типа.</summary>
        [Test]
        public void TestParseWhenSelectTuple()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string FILTER_FIELD_NAME = "[filter_field]";
            const string FILTER_VALUE = "[filter_value]";

            const string SORT_FIELD_1_NAME = "[sort_field_1]";
            const string SORT_FIELD_2_NAME = "[sort_field_2]";

            const string STRING_FIELD_NAME = "[string_field]";
            const string INT32_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                     .SelectExpression(r => new { StringField = r.GetString(STRING_FIELD_NAME), IntField = r.GetInt32(INT32_FIELD_NAME) });
            var trait = selectExpression.GetTraitOfOutputType();

            var testedExpression = TestHelperQueryProvider
                                .BuildTestQueryExpression(SOURCE_NAME,
                                q => q
                                    .Where(r => r.GetString(FILTER_FIELD_NAME) == FILTER_VALUE)
                                    .OrderBy(r => r.GetInt32(SORT_FIELD_1_NAME))
                                    .ThenByDescending(r => r.GetString(SORT_FIELD_2_NAME))
                                    .Select(selectExpression)
                                );

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(1, command.Parameters.Count);
            Assert.AreEqual(FILTER_VALUE, command.Parameters[0].Value);

            var parameterName = command.Parameters[0].Name;
            Assert.AreEqual(
                expected: "SELECT " + STRING_FIELD_NAME + ", " + INT32_FIELD_NAME + " FROM " + SOURCE_NAME + " WHERE " + FILTER_FIELD_NAME + " = &" + parameterName + " ORDER BY " + SORT_FIELD_1_NAME + ", " + SORT_FIELD_2_NAME + " DESC",
                actual: command.Sql
                );

            var recordProduct = AssertAndCastCollectionReadExpressionParseProduct(trait, result);
            var itemReaderFactory = AssertAndCastNoSideEffectItemReaderFactory(trait, recordProduct.ItemReaderFactory);
            var itemReader = itemReaderFactory.ItemReader;

            // Verify Item Reader
            object stringValue = "string";
            object intValue = 13;

            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToString(stringValue))
                .Returns((string)stringValue);
            valueConverterMock
                .Setup(c => c.ToInt32(intValue))
                .Returns((int)intValue);

            var item = itemReader(valueConverterMock.Object, new[] { stringValue, intValue });

            Assert.AreEqual(stringValue, item.StringField);
            Assert.AreEqual(intValue, item.IntField);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи.</summary>
        [Test]
        public void TestParseWhenGetTypedRecords()
        {
            // Arrange
            var query = TestHelperQueryProvider.QueryOf<AnyDataType>();
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertAndCast<CollectionReadExpressionParseProduct<AnyDataType>>(result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест",
                product.Command.Sql
                );

            Assert.AreEqual(0, product.Command.Parameters.Count);

            var itemReaderFactory = AssertAndCast<NoSideEffectItemReaderFactory<AnyDataType>>(product.ItemReaderFactory);
            var itemReader = itemReaderFactory.ItemReader;

            // Assert Item Reader
            const string TEST_NAME = "Лыжи";
            const int TEST_ID = 10;
            const decimal TEST_PRICE = 34.55m;
            var testStartDate = new DateTime(2001, 09, 23);

            var values = new object[]
                {
                    TEST_ID, TEST_PRICE, testStartDate, TEST_NAME 
                };

            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            valueConverterMock
                .Setup(c => c.ToString(It.IsAny<object>()))
                .Returns<object>(o => (string)o);
            valueConverterMock
                .Setup(c => c.ToInt32(It.IsAny<object>()))
                .Returns<object>(o => (int)o);
            valueConverterMock
                .Setup(c => c.ToDecimal(It.IsAny<object>()))
                .Returns<object>(o => (decimal)o);
            valueConverterMock
                .Setup(c => c.ToDateTime(It.IsAny<object>()))
                .Returns<object>(o => (DateTime)o);

            var actualItem = itemReader(valueConverterMock.Object, values);

            Assert.IsNotNull(actualItem);
            Assert.AreEqual(TEST_NAME, actualItem.Name);
            Assert.AreEqual(TEST_ID, actualItem.Id);
            Assert.AreEqual(TEST_PRICE, actualItem.Price);
            Assert.AreEqual(testStartDate, actualItem.StartDate);
        }

        public abstract class DataTypeBase
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }
        }

        [OneSDataSource("Справочник.Тест")]
        public sealed class AnyDataType : DataTypeBase
        {
            [OneSDataColumn("Идентификатор")]
            public int Id { get; set; }

            [OneSDataColumn("Цена")]
            public decimal Price { get; set; }

            [OneSDataColumn("ДатаНачала")]
            public DateTime StartDate { get; set; }
        }
    }
}