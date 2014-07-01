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
    public sealed class ExpressionParserComponentTests
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private readonly ExpressionParser _testedInstance = ExpressionParser.Default;

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

            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
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

            var recordProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
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

            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
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

            var recordProduct = AssertEx.IsInstanceAndCastCollectionReadExpressionParseProduct(trait, result);
            var itemReaderFactory = AssertEx.IsInstanceAndCastNoSideEffectItemReaderFactory(recordProduct.ItemReaderFactory);
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

        /// <summary>
        /// Проверка корректного создания фабрики читателя типизированного кортежа.
        /// </summary>
        /// <param name="testedItemReaderFactory">
        /// Тестируемая фабрика читателя типизированного кортежа
        /// </param>
        private static void AssertTypedTupleReaderFactory(IItemReaderFactory<AnyDataType> testedItemReaderFactory)
        {
            var noSideEffectReaderFactory = AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<AnyDataType>>(testedItemReaderFactory);
            AssertTypedTupleReader(noSideEffectReaderFactory.ItemReader);
        }

        /// <summary>Проверка корректного создания читателя типизированного кортежа.</summary>
        /// <param name="testedItemReader">Тестируемый читатель типизированного кортежа.</param>
        private static void AssertTypedTupleReader(Func<IValueConverter, object[], AnyDataType> testedItemReader)
        {
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

            var actualItem = testedItemReader(valueConverterMock.Object, values);

            Assert.IsNotNull(actualItem);
            Assert.AreEqual(TEST_NAME, actualItem.Name);
            Assert.AreEqual(TEST_ID, actualItem.Id);
            Assert.AreEqual(TEST_PRICE, actualItem.Price);
            Assert.AreEqual(testStartDate, actualItem.StartDate);
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
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<AnyDataType>>(result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест",
                product.Command.Sql
                );

            Assert.AreEqual(0, product.Command.Parameters.Count);
            AssertTypedTupleReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи c фильтрацией.</summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithFilter()
        {
            // Arrange
            var query = TestHelperQueryProvider.QueryOf<AnyDataType>();
            query = query.Where(d => d.Name == "Тест");
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<AnyDataType>>(result);

            Assert.AreEqual(1, product.Command.Parameters.Count);
            var parameter = product.Command.Parameters[0];

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест WHERE Наименование = &" + parameter.Name,
                product.Command.Sql
                );


            Assert.AreEqual("Тест", parameter.Value);
            AssertTypedTupleReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга запроса, который выбирает типизированные записи c выборкой отдельных полей в анонимном типе.
        /// </summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithSelect()
        {
            // Arrange
            var query0 = TestHelperQueryProvider.QueryOf<AnyDataType>();
            var query = query0.Select(d => new { d.Id, d.Price });
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            var trait = query.GetTrait();

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastCollectionReadExpressionParseProduct(trait, result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена FROM Справочник.Тест",
                product.Command.Sql
                );
            Assert.AreEqual(0, product.Command.Parameters.Count);

            var typedFactory = AssertEx.IsInstanceAndCastNoSideEffectItemReaderFactory(product.ItemReaderFactory);
            var testedItemReader = typedFactory.ItemReader;

            // TODO: Копипаста

            const int TEST_ID = 10;
            const decimal TEST_PRICE = 34.55m;

            var values = new object[] { TEST_ID, TEST_PRICE };

            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            valueConverterMock
                .Setup(c => c.ToInt32(It.IsAny<object>()))
                .Returns<object>(o => (int)o)
                .Verifiable();

            valueConverterMock
                .Setup(c => c.ToDecimal(It.IsAny<object>()))
                .Returns<object>(o => (decimal)o)
                .Verifiable();

            var actualItem = testedItemReader(valueConverterMock.Object, values);

            Assert.IsNotNull(actualItem);
            Assert.AreEqual(TEST_ID, actualItem.Id);
            Assert.AreEqual(TEST_PRICE, actualItem.Price);
            
            valueConverterMock.Verify(c => c.ToInt32(values[0]));
            valueConverterMock.Verify(c => c.ToDecimal(values[1]));
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи c сортировкой.</summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithSorting()
        {
            // Arrange
            var query = TestHelperQueryProvider
                .QueryOf<AnyDataType>()
                .OrderBy(d => d.Price)
                .ThenByDescending(d => d.StartDate);
            
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<AnyDataType>>(result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест ORDER BY Цена, ДатаНачала DESC",
                product.Command.Sql
                );
            Assert.AreEqual(0, product.Command.Parameters.Count);

            AssertTypedTupleReaderFactory(product.ItemReaderFactory);
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