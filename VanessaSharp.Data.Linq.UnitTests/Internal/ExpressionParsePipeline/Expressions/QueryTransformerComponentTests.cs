using System;
using System.Collections.Generic;
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

        private const string SOURCE_NAME = "[source]";

        private static IQuery<OneSDataRecord, T> CreateDataRecordsQuery<T>(string source,
                                                                            Expression<Func<OneSDataRecord, T>> selector,
                                                                            Expression<Func<OneSDataRecord, bool>> filter = null,
                                                                            params SortExpression[] sorters)
        {
            return CreateQuery(new ExplicitSourceDescription(source), selector, filter, sorters);
        }

        private static IQuery<OneSDataRecord, OneSDataRecord> CreateDataRecordsQuery(
            string source,
            Expression<Func<OneSDataRecord, bool>> filter = null,
            params SortExpression[] sorters)
        {
            return CreateDataRecordsQuery<OneSDataRecord>(source, null, filter, sorters);
        }

        private static IQuery<SomeData, T> CreateTypedRecordsQuery<T>(Expression<Func<SomeData, T>> selectExpression,
                                                                       Expression<Func<SomeData, bool>> filterExpression = null,
                                                                       params SortExpression[] sorters)
        {
            return CreateQuery(SourceDescriptionByType<SomeData>.Instance, selectExpression, filterExpression, sorters);
        }

        private static IQuery<SomeData, SomeData> CreateTypedRecordsQuery(
            Expression<Func<SomeData, bool>> filterExpression = null,
            params SortExpression[] sorters)
        {
            return CreateTypedRecordsQuery<SomeData>(null, filterExpression, sorters);
        }

        private static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
           IItemReaderFactory<T> factory)
        {
            return AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<T>>(factory);
        }

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
            _mappingProviderMock
                .Setup(p => p.IsDataType(It.IsAny<Type>()))
                .Returns(false);

            _testedInstance = new QueryTransformer(_mappingProviderMock.Object);
        }
        
        /// <summary>
        /// Тестирование преобразования запроса с выборкой элементов анонимного типа.
        /// </summary>
        [Test]
        public void TestTransformSelectDataRecords()
        {
            // Arrange
            
            const string FIRST_FIELD_NAME = "[string_field]";
            const string SECOND_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                                        .SelectExpression(r => new { First = r.GetString(FIRST_FIELD_NAME), Second = r.GetInt32(SECOND_FIELD_NAME) });
            var query = CreateDataRecordsQuery(SOURCE_NAME, selectExpression);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT " + FIRST_FIELD_NAME + ", " + SECOND_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var noSideItemReaderFactory = AssertAndCastNoSideEffectItemReaderFactory(result.ItemReaderFactory);
            var itemReader = noSideItemReaderFactory.ItemReader;

            // Тестирование полученного делегата чтения кортежа
            ItemReaderTester.For(itemReader, 2)
                .Field(0, i => i.First, c => c.ToString(null), "Test")
                .Field(1, i => i.Second, c => c.ToInt32(null), 34)
                .Test();
        }

        /// <summary>Тестирование преобразования запроса простой выборки и фильтрации записей.</summary>
        [Test]
        public void TestTransformFilterDataRecords()
        {
            const string FILTER_FIELD = "[filter_field]";
            const string FILTER_VALUE = "filter_value";

            // Arrange
            var query = CreateDataRecordsQuery(SOURCE_NAME, r => r.GetString(FILTER_FIELD) == FILTER_VALUE);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;
            
            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                "SELECT * FROM "+ SOURCE_NAME + " WHERE " + FILTER_FIELD + " = \"" + FILTER_VALUE + "\"", command.Sql);

            var parseProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(parseProduct.ItemReaderFactory);
        }

        /// <summary>Тестирование преобразования запроса простой выборки и сортировки записей.</summary>
        [TestCase("Ascending", "", Description = "Тестирование в случае сортировки по убыванию")]
        [TestCase("Descending", " DESC", Description = "Тестирование в случае сортировки по возрастанию")]
        public void TestTransformOrderByDataRecords(string sortKindString, string expectedSqlPart)
        {
            var sortKind = (SortKind)Enum.Parse(typeof(SortKind), sortKindString);

            // Arrange
            const string SORT_FIELD = "[sort_field]";
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(SORT_FIELD);
            var query = CreateDataRecordsQuery(SOURCE_NAME, null, new SortExpression(sortKeyExpression, sortKind));

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(0, command.Parameters.Count);

            Assert.AreEqual(
                "SELECT * FROM "+ SOURCE_NAME + " ORDER BY " + SORT_FIELD + expectedSqlPart, command.Sql);

            var parseProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<OneSDataRecord>>(result);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(parseProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование преобразования запроса типизированных кортежей.
        /// </summary>
        private void TestTransformTypedRecords(
            Func<IList<SqlParameter>, string> expectedSqlPart,
            int expectedSqlParametersCount,
            Action<IList<SqlParameter>> sqlParamtersTester = null,
            Expression<Func<SomeData, bool>> filter = null,
            params SortExpression[] sorters
            )
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("ТестовыйИсточник")
                    .FieldMap(d => d.Id, "Идентификатор")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            var query = CreateTypedRecordsQuery(filter, sorters);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(expectedSqlParametersCount, command.Parameters.Count);
            
            var expectedSql = "SELECT Идентификатор, Наименование, Цена FROM ТестовыйИсточник " 
                + expectedSqlPart(command.Parameters);
            Assert.AreEqual(expectedSql, command.Sql);

            if (sqlParamtersTester != null)
                sqlParamtersTester(command.Parameters);

            var parseProduct = AssertEx.IsInstanceAndCastOf<CollectionReadExpressionParseProduct<SomeData>>(result);
            var typedFactory = AssertEx.IsInstanceAndCastOf<NoSideEffectItemReaderFactory<SomeData>>(parseProduct.ItemReaderFactory);

            ItemReaderTester.For(typedFactory.ItemReader, 3)
                .Field(0, d => d.Id, c => c.ToInt32(null), 45)
                .Field(1, d => d.Name, c => c.ToString(null), "Тестовый")
                .Field(2, d => d.Price, c => c.ToDecimal(null), 45.5m)
                .Test();
        }

        /// <summary>
        /// Тестирование преобразования запроса фильтрации типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformFilterTypedRecords()
        {
            const string FILTER_VALUE = "filter_value";
            Expression<Func<SomeData, bool>> filter = d => d.Name == FILTER_VALUE;

            TestTransformTypedRecords(
                parameters => "WHERE Наименование = \"" + FILTER_VALUE + "\"",
                0,
                filter: filter
                );
        }

        /// <summary>
        /// Тестирование преобразования запроса сортировки типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformSorterTypedRecords()
        {
            Expression<Func<SomeData, string>> sortKeyExpression1 = d => d.Name;
            Expression<Func<SomeData, decimal>> sortKeyExpression2 = d => d.Price;
            
            TestTransformTypedRecords(
                parameters => "ORDER BY Наименование DESC, Цена",
                0,
                sorters: new [] { 
                    new SortExpression(sortKeyExpression1, SortKind.Descending),
                    new SortExpression(sortKeyExpression2, SortKind.Ascending)
                    }
                );
        }

        /// <summary>
        /// Тестирование преобразования запроса выборки типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformSelectTypedRecords()
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("ТестовыйИсточник")
                    .FieldMap(d => d.Id, "Идентификатор")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            var selector = Trait.Of<SomeData>()
                .SelectExpression(d => new {d.Id, d.Price});

            var query = CreateTypedRecordsQuery(selector);

            // Act
            var result = _testedInstance.Transform(query);

            // Assert
            var command = result.Command;

            Assert.AreEqual(
                "SELECT Идентификатор, Цена FROM ТестовыйИсточник", command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var factory = AssertAndCastNoSideEffectItemReaderFactory(result.ItemReaderFactory);
            
            ItemReaderTester.For(factory.ItemReader, 2)
                .Field(0, d => d.Id, c => c.ToInt32(null), 34)
                .Field(1, d => d.Price, c => c.ToDecimal(null), 45.65m)
                .Test();
        }

        /// <summary>
        /// Тестирование преобразования запроса получения суммы поля <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestTransformSumFieldDataRecords()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> selectExpression = r => r.GetInt32("quantity");

            var query = CreateScalarQuery<OneSDataRecord, int, int>(
                                          AggregateFunction.Summa, new ExplicitSourceDescription(SOURCE_NAME),
                                          selectExpression);

            // Act
            var result = _testedInstance.TransformScalar(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT SUM(quantity) FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var rawValue = new object();
            const int EXPECTED_VALUE = 435345;
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToInt32(rawValue))
                .Returns(EXPECTED_VALUE);

            Assert.AreEqual(EXPECTED_VALUE, result.Converter(valueConverterMock.Object, rawValue));
            valueConverterMock
                .Verify(c => c.ToInt32(rawValue), Times.Once());
        }

        /// <summary>
        /// Тестирование преобразования запроса получения суммы поля <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestTransformAvgFieldDataRecords()
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("ТестовыйИсточник")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            Expression<Func<SomeData, decimal>> selectExpression = r => r.Price;

            var query = CreateScalarQuery<SomeData, decimal, decimal>(
                                          AggregateFunction.Average,
                                          SourceDescriptionByType<SomeData>.Instance,
                                          selectExpression);

            // Act
            var result = _testedInstance.TransformScalar(query);

            // Assert
            var command = result.Command;
            Assert.AreEqual("SELECT AVG(Цена) FROM ТестовыйИсточник", command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var rawValue = new object();
            const decimal EXPECTED_VALUE = 435345.55m;
            var valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            valueConverterMock
                .Setup(c => c.ToDecimal(rawValue))
                .Returns(EXPECTED_VALUE);

            Assert.AreEqual(EXPECTED_VALUE, result.Converter(valueConverterMock.Object, rawValue));
            valueConverterMock
                .Verify(c => c.ToDecimal(rawValue), Times.Once());
        }
    }
}
