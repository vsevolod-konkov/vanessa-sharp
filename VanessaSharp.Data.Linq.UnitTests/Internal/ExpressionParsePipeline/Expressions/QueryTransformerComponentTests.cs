using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Компонентное тестирование <see cref="QueryTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryTransformerComponentTests : QueryTransformerTestsBase
    {
        private Mock<IOneSMappingProvider> _mappingProviderMock;

        private QueryTransformer _testedInstance;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
            _testedInstance = new QueryTransformer(_mappingProviderMock.Object);
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
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT " + FIRST_FIELD_NAME + ", " + SECOND_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var noSideItemReaderFactory = AssertAndCastNoSideEffectItemReaderFactory(result.ItemReaderFactory);
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

        /// <summary>Тестирование преобразования запроса простой выборки и фильтрации записей.</summary>
        [Test]
        public void TestTransformFilterOneSDataRecord()
        {
            const string FILTER_VALUE = "filter_value";

            // Arrange
            var query = CreateQuery("[source]", whereExpression: r => r.GetString("filter_field") == FILTER_VALUE);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            
            Assert.AreEqual(1, command.Parameters.Count);
            var parameter = command.Parameters[0];
            Assert.AreEqual(FILTER_VALUE, parameter.Value);

            Assert.AreEqual(
                "SELECT * FROM [source] WHERE filter_field = &" + parameter.Name, command.Sql);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(parseProduct.ItemReaderFactory);
        }

        /// <summary>Тестирование преобразования запроса простой выборки и сортировки записей.</summary>
        [TestCase("Ascending", "", Description = "Тестирование в случае сортировки по убыванию")]
        [TestCase("Descending", " DESC", Description = "Тестирование в случае сортировки по возрастанию")]
        public void TestTransformOrderByOneSDataRecord(string sortKindString, string expectedSqlPart)
        {
            var sortKind = (SortKind)Enum.Parse(typeof(SortKind), sortKindString);

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32("sort_field");
            var query = CreateQuery("[source]", null, new SortExpression(sortKeyExpression, sortKind));

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                "SELECT * FROM [source] ORDER BY sort_field" + expectedSqlPart, command.Sql);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса фильтрации типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformFilterTypedTuple()
        {
            // Arrange
            var typeMapping = new OneSTypeMapping(
                "ТестовыйИсточник",
                new ReadOnlyCollection<OneSFieldMapping>(
                    new[]
                        {
                            CreateFieldMapping(d => d.Id, "Идентификатор"),
                            CreateFieldMapping(d => d.Name, "Наименование"),
                            CreateFieldMapping(d => d.Price, "Цена")
                        }));

            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof(AnyData)))
                .Returns(typeMapping);
            
            const string FILTER_VALUE = "filter_value";

            Expression<Func<AnyData, bool>> filterExpression = d => d.Name == FILTER_VALUE;
            
            var query = CreateTupleQuery(filterExpression: filterExpression);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(1, command.Parameters.Count);
            var parameter = command.Parameters[0];
            Assert.AreEqual(FILTER_VALUE, parameter.Value);

            Assert.AreEqual(
                "SELECT Идентификатор, Наименование, Цена FROM ТестовыйИсточник WHERE Наименование = &" + parameter.Name, command.Sql);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<AnyData>>(result);
            Assert.IsInstanceOf<NoSideEffectItemReaderFactory<AnyData>>(parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса сортировки типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformSorterTypedTuple()
        {
            // Arrange
            var typeMapping = new OneSTypeMapping(
                "ТестовыйИсточник",
                new ReadOnlyCollection<OneSFieldMapping>(
                    new[]
                        {
                            CreateFieldMapping(d => d.Id, "Идентификатор"),
                            CreateFieldMapping(d => d.Name, "Наименование"),
                            CreateFieldMapping(d => d.Price, "Цена")
                        }));

            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof(AnyData)))
                .Returns(typeMapping);

            Expression<Func<AnyData, string>> sortKeyExpression1 = d => d.Name;
            Expression<Func<AnyData, decimal>> sortKeyExpression2 = d => d.Price;

            var query = CreateTupleQuery(
                sorters:
                new ReadOnlyCollection<SortExpression>(new []
                    {
                        new SortExpression(sortKeyExpression1, SortKind.Descending),
                        new SortExpression(sortKeyExpression2, SortKind.Ascending), 
                    }));

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(
                "SELECT Идентификатор, Наименование, Цена FROM ТестовыйИсточник ORDER BY Наименование DESC, Цена", command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var parseProduct = AssertAndCast<CollectionReadExpressionParseProduct<AnyData>>(result);
            Assert.IsInstanceOf<NoSideEffectItemReaderFactory<AnyData>>(parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса выборки типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformSelectTypedTuple()
        {
            // Arrange
            var typeMapping = new OneSTypeMapping(
                "ТестовыйИсточник",
                new ReadOnlyCollection<OneSFieldMapping>(
                    new[]
                        {
                            CreateFieldMapping(d => d.Id, "Идентификатор"),
                            CreateFieldMapping(d => d.Name, "Наименование"),
                            CreateFieldMapping(d => d.Price, "Цена")
                        }));

            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof(AnyData)))
                .Returns(typeMapping);

            var selectExpression = Trait.Of<AnyData>().SelectExpression(d => new {d.Id, d.Price});

            var query = CreateTupleQuery(selectExpression);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(
                "SELECT Идентификатор, Цена FROM ТестовыйИсточник", command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var factory = AssertAndCastNoSideEffectItemReaderFactory(result.ItemReaderFactory);
            
            // Test Item Reader
            // TODO: Копипаста
            var itemReader = factory.ItemReader;

            // Arrange
            const int INT32_VALUE = 34;
            const decimal NUMBER_VALUE = 45.65m;

            
            var values = new object[] { INT32_VALUE, NUMBER_VALUE };
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            
            valueConverterMock
                .Setup(c => c.ToInt32(values[0]))
                .Returns(INT32_VALUE)
                .Verifiable();
            valueConverterMock
                .Setup(c => c.ToDecimal(values[1]))
                .Returns(NUMBER_VALUE)
                .Verifiable();

            // Act
            var item = itemReader(valueConverterMock.Object, values);


            // Assert
            Assert.AreEqual(INT32_VALUE, item.Id);
            Assert.AreEqual(NUMBER_VALUE, item.Price);

            valueConverterMock
                .Verify(c => c.ToInt32(values[0]), Times.Once());
            valueConverterMock
                .Verify(c => c.ToDecimal(values[1]), Times.Once());
            
        }

        // TODO Копипаста
        private static OneSFieldMapping CreateFieldMapping<T>(Expression<Func<AnyData, T>> accessor, string fieldName)
        {
            var memberInfo = ((MemberExpression)accessor.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }
    }
}
