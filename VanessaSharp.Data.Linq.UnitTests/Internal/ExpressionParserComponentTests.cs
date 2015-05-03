using System;
using System.Linq;
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

        private static CollectionReadExpressionParseProduct<T> IsInstanceAndCastCollectionReadExpressionParseProduct<T>(
            Trait<T> trait, ExpressionParseProduct product)
        {
            return AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<T>>(product);
        }

        private static NoSideEffectItemReaderFactory<T> IsInstanceAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> itemReaderFactory)
        {
            return AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<T>>(itemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает целые записи.</summary>
        [Test]
        public void TestParseWhenGetDataRecords()
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
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT * FROM " + SOURCE_NAME + " WHERE " + FILTER_FIELD_NAME + " = \"" + FILTER_VALUE + "\" ORDER BY " + SORT_FIELD_2_NAME + " DESC, " + SORT_FIELD_3_NAME,
                actual: command.Sql
                );

            var recordProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает избранные поля в кортеж анонимного типа.</summary>
        [Test]
        public void TestParseWhenSelectDataRecords()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string FILTER_FIELD_NAME = "[filter_field]";
            const string FILTER_VALUE = "[filter_value]";

            const string SORT_FIELD_1_NAME = "[sort_field_1]";
            const string SORT_FIELD_2_NAME = "[sort_field_2]";

            const string STRING_FIELD_NAME = "[string_field]";
            const string INT32_FIELD_NAME = "[int_field]";

            var selector = Trait.Of<OneSDataRecord>()
                     .SelectExpression(r => new { StringField = r.GetString(STRING_FIELD_NAME), IntField = r.GetInt32(INT32_FIELD_NAME) });
            var trait = selector.GetTraitOfOutputType();

            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
                    .Where(r => r.GetString(FILTER_FIELD_NAME) == FILTER_VALUE)
                    .OrderBy(r => r.GetInt32(SORT_FIELD_1_NAME))
                    .ThenByDescending(r => r.GetString(SORT_FIELD_2_NAME))
                    .Select(selector)
                    );

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT " + STRING_FIELD_NAME + ", " + INT32_FIELD_NAME + " FROM " + SOURCE_NAME + " WHERE " + FILTER_FIELD_NAME + " = \"" + FILTER_VALUE + "\" ORDER BY " + SORT_FIELD_1_NAME + ", " + SORT_FIELD_2_NAME + " DESC",
                actual: command.Sql
                );

            var recordProduct = IsInstanceAndCastCollectionReadExpressionParseProduct(trait, result);
            var itemReaderFactory = IsInstanceAndCastNoSideEffectItemReaderFactory(recordProduct.ItemReaderFactory);
            
            ItemReaderTester
                .For(itemReaderFactory.ItemReader, 2)
                .Field(0, r => r.StringField, c => c.ToString(null), "test")
                .Field(1, r => r.IntField, c => c.ToInt32(null), 13)
                .Test();
        }

        /// <summary>
        /// Проверка корректного создания фабрики читателя типизированных записей.
        /// </summary>
        /// <param name="testedItemReaderFactory">
        /// Тестируемая фабрика читателя типизированных записей.
        /// </param>
        private static void AssertTypedRecordsReaderFactory(IItemReaderFactory<SomeData> testedItemReaderFactory)
        {
            var noSideEffectReaderFactory = AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<SomeData>>(testedItemReaderFactory);
            AssertTypedTupleReader(noSideEffectReaderFactory.ItemReader);
        }

        /// <summary>Проверка корректного создания читателя типизированного кортежа.</summary>
        /// <param name="testedItemReader">Тестируемый читатель типизированного кортежа.</param>
        private static void AssertTypedTupleReader(Func<IValueConverter, object[], SomeData> testedItemReader)
        {
            ItemReaderTester
                .For(testedItemReader, 4)
                    .Field(0, d => d.Id, c => c.ToInt32(null), 10)
                    .Field(1, d => d.Price, c => c.ToDecimal(null), 34.55m)
                    .Field(2, d => d.StartDate, c => c.ToDateTime(null), new DateTime(2001, 09, 23))
                    .Field(3, d => d.Name, c => c.ToString(null), "Лыжи")
                .Test();
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи.</summary>
        [Test]
        public void TestParseWhenGetTypedRecords()
        {
            // Arrange
            var query = TestHelperQueryProvider.QueryOf<SomeData>();
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<SomeData>>(result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест",
                product.Command.Sql
                );

            Assert.AreEqual(0, product.Command.Parameters.Count);
            AssertTypedRecordsReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи c фильтрацией.</summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithFilter()
        {
            // Arrange
            var query = TestHelperQueryProvider.QueryOf<SomeData>();
            query = query.Where(d => d.Name == "Тест");
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<SomeData>>(result);

            Assert.AreEqual(0, product.Command.Parameters.Count);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест WHERE Наименование = \"Тест\"",
                product.Command.Sql
                );

            AssertTypedRecordsReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга запроса, который выбирает типизированные записи c выборкой отдельных полей в анонимном типе.
        /// </summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithSelect()
        {
            // Arrange
            var selector = Trait.Of<SomeData>().SelectExpression(d => new {d.Id, d.Price});
            var trait = selector.GetTraitOfOutputType();

            var testedExpression = QueryableExpression
                                        .For<SomeData>()
                                        .Query(q => q.Select(selector));

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = IsInstanceAndCastCollectionReadExpressionParseProduct(trait, result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена FROM Справочник.Тест",
                product.Command.Sql
                );
            Assert.AreEqual(0, product.Command.Parameters.Count);

            var typedFactory = IsInstanceAndCastNoSideEffectItemReaderFactory(product.ItemReaderFactory);

            ItemReaderTester
                .For(typedFactory.ItemReader, 2)
                    .Field(0, d => d.Id, c => c.ToInt32(null), 10)
                    .Field(1, d => d.Price, c => c.ToDecimal(null), 34.55m)
                .Test();
        }

        /// <summary>Тестирование парсинга запроса, который выбирает типизированные записи c сортировкой.</summary>
        [Test]
        public void TestParseWhenGetTypedRecordsWithSorting()
        {
            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q
                    .OrderBy(d => d.Price)
                    .ThenByDescending(d => d.StartDate));

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<SomeData>>(result);

            Assert.AreEqual(
                "SELECT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест ORDER BY Цена, ДатаНачала DESC",
                product.Command.Sql
                );
            Assert.AreEqual(0, product.Command.Parameters.Count);

            AssertTypedRecordsReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Distinct{TSource}(System.Linq.IQueryable{TSource})"/>
        /// над получением записей данных.
        /// </summary>
        [Test]
        public void TestWhenDistinctDataRecord()
        {
            // Arrange
            var testedExpression = QueryableExpression
                .ForDataRecords("Справочник.Тест")
                .Query(q => q.Distinct());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT DISTINCT * FROM Справочник.Тест",
                actual: command.Sql
                );

            var recordProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Distinct{TSource}(System.Linq.IQueryable{TSource})"/>
        /// над получением выборки записей данных.
        /// </summary>
        [Test]
        public void TestWhenDistinctSelectDataRecord()
        {
            var selector = Trait.Of<OneSDataRecord>()
                                .SelectExpression(r => new {Id = r.GetInt32("Идентификатор"), Name = r.GetString("Наименование")});
            
            // Arrange
            var testedExpression = QueryableExpression
                .ForDataRecords("Справочник.Тест")
                .Query(q => q
                    .Select(selector)
                    .Distinct());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT DISTINCT Идентификатор, Наименование FROM Справочник.Тест",
                actual: command.Sql
                );

            var recordProduct = IsInstanceAndCastCollectionReadExpressionParseProduct(selector.GetTraitOfOutputType(), result);
            var factory = IsInstanceAndCastNoSideEffectItemReaderFactory(recordProduct.ItemReaderFactory);
            
            ItemReaderTester.For(factory.ItemReader, 2)
                .Field(0, r => r.Id, c => c.ToInt32(null), 5)
                .Field(1, r => r.Name, c => c.ToString(null), "test")
            .Test();
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Distinct{TSource}(System.Linq.IQueryable{TSource})"/>
        /// над получением типизированных записей данных.
        /// </summary>
        [Test]
        public void TestWhenDistinctTypedRecord()
        {
            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q
                    .Distinct());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT DISTINCT Идентификатор, Цена, ДатаНачала, Наименование FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<SomeData>>(result);
            AssertTypedRecordsReaderFactory(product.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Distinct{TSource}(System.Linq.IQueryable{TSource})"/>
        /// над получением выборки типизированных записей данных.
        /// </summary>
        [Test]
        public void TestWhenDistinctSelectTypedRecord()
        {
            // Arrange
            var selector = Trait.Of<SomeData>().SelectExpression(r => new {r.Id, r.Name});
            
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q
                    .Select(selector)
                    .Distinct());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT DISTINCT Идентификатор, Наименование FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = IsInstanceAndCastCollectionReadExpressionParseProduct(selector.GetTraitOfOutputType(), result);
            var factory = IsInstanceAndCastNoSideEffectItemReaderFactory(product.ItemReaderFactory);

            ItemReaderTester
                .For(factory.ItemReader, 2)
                    .Field(0, r => r.Id, c => c.ToInt32(null), 2)
                    .Field(1, r => r.Name, c => c.ToString(null), "test")
                .Test();
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>
        /// для <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestWhenCountDataRecords()
        {
            // Arrange
            var testedExpression = QueryableExpression
                .ForDataRecords("Справочник.Тест")
                .ScalarQuery(q => q, q1 => q1.Count());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT COUNT(*) FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = AssertEx.IsInstanceAndCastOf<ScalarReadExpressionParseProduct<int>>(result);
            
            ConverterTester.Test(product.Converter, c => c.ToInt32(null), 3534);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>
        /// для типизированных записей.
        /// </summary>
        [Test]
        public void TestWhenCountTypedRecords()
        {
            // TODO: CopyPaste

            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .ScalarQuery(q => q, q1 => q1.Count());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT COUNT(*) FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = AssertEx.IsInstanceAndCastOf<ScalarReadExpressionParseProduct<int>>(result);

            ConverterTester.Test(product.Converter, c => c.ToInt32(null), 3534);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>
        /// для полей.
        /// </summary>
        [Test]
        public void TestWhenCountField()
        {
            // TODO: CopyPaste

            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .ScalarQuery(q => q.Select(d => d.Price), q1 => q1.Count());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT COUNT(Цена) FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = AssertEx.IsInstanceAndCastOf<ScalarReadExpressionParseProduct<int>>(result);

            ConverterTester.Test(product.Converter, c => c.ToInt32(null), 3534);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>
        /// для полей.
        /// </summary>
        [Test]
        public void TestWhenDistinctCountField()
        {
            // TODO: CopyPaste

            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .ScalarQuery(q => q.Select(d => d.Price).Distinct(), q1 => q1.LongCount());

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT COUNT(DISTINCT Цена) FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = AssertEx.IsInstanceAndCastOf<ScalarReadExpressionParseProduct<long>>(result);

            ConverterTester.Test(product.Converter, c => c.ToInt64(null), 3534L);
        }

        /// <summary>
        /// Тестирование метода <see cref="Queryable.Take{TSource}"/>
        /// над получением выборки типизированных записей данных.
        /// </summary>
        [Test]
        public void TestWhenTakeSelectTypedRecord()
        {
            // Arrange
            var selector = Trait.Of<SomeData>().SelectExpression(r => new { r.Id, r.Name });

            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q
                    .Select(selector)
                    .Take(5));

            // Act
            var result = _testedInstance.Parse(testedExpression);

            // Assert
            var command = result.Command;
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                expected: "SELECT TOP 5 Идентификатор, Наименование FROM Справочник.Тест",
                actual: command.Sql
                );

            var product = IsInstanceAndCastCollectionReadExpressionParseProduct(selector.GetTraitOfOutputType(), result);
            var factory = IsInstanceAndCastNoSideEffectItemReaderFactory(product.ItemReaderFactory);

            ItemReaderTester
                .For(factory.ItemReader, 2)
                    .Field(0, r => r.Id, c => c.ToInt32(null), 2)
                    .Field(1, r => r.Name, c => c.ToString(null), "test")
                .Test();
        }

        public abstract class DataBase
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }
        }

        [OneSDataSource("Справочник.Тест")]
        public sealed class SomeData : DataBase
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