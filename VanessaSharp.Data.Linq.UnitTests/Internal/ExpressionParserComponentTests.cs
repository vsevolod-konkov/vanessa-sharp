using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тесты на экземпляр <see cref="ExpressionParser"/>.
    /// </summary>
    [TestFixture]
    public sealed class ExpressionParserComponentTests : QueryableExpressionTransformTestBase
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

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            var expression = GetGetRecordsExpression(SOURCE_NAME);

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(product);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга выражения выборки полей из записей источника.
        /// </summary>
        [Test]
        public void TestParseSelectRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string STRING_FIELD_NAME = "[string_field]";
            const string INT32_FIELD_NAME = "[int_field]";

            var selectExpression = Trait.Of<OneSDataRecord>()
                     .SelectExpression(r => new {StringField = r.GetString(STRING_FIELD_NAME), IntField = r.GetInt32(INT32_FIELD_NAME)});

            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(SOURCE_NAME, q => q.Select(selectExpression));
            var trait = selectExpression.GetTraitOfOutputType();

            // Act
            var product = _testedInstance.Parse(testedExpression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT " + STRING_FIELD_NAME + ", " + INT32_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCastCollectionReadExpressionParseProduct(trait, product);
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

            var item = itemReader(valueConverterMock.Object, new[] {stringValue, intValue});

            Assert.AreEqual(stringValue, item.StringField);
            Assert.AreEqual(intValue, item.IntField);
        }

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsWhereExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string STRING_FIELD_NAME = "[string_field]";
            const string FILTER_VALUE = "[filter]";

            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(
                    SOURCE_NAME,
                    q => q.Where(r => r.GetString(STRING_FIELD_NAME) == FILTER_VALUE));

            // Act
            var product = _testedInstance.Parse(testedExpression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME + " WHERE " + STRING_FIELD_NAME + " = &p1", command.Sql);
            Assert.AreEqual(1, command.Parameters.Count);

            var parameter = command.Parameters[0];
            Assert.AreEqual("p1", parameter.Name);
            Assert.AreEqual(FILTER_VALUE, parameter.Value);

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(product);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsSortExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string SORT_FIELD_1_NAME = "[sort_field_1]";
            const string SORT_FIELD_2_NAME = "[sort_field_2]";
            const string SORT_FIELD_3_NAME = "[sort_field_3]";

            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(
                    SOURCE_NAME,
                    q => q.OrderBy(r => r.GetInt32(SORT_FIELD_1_NAME)).OrderByDescending(r => r.GetInt32(SORT_FIELD_2_NAME)).ThenBy(r => r.GetString(SORT_FIELD_3_NAME)));

            // Act
            var product = _testedInstance.Parse(testedExpression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME + " ORDER BY " + SORT_FIELD_2_NAME + " DESC, " + SORT_FIELD_3_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(product);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }
    }
}