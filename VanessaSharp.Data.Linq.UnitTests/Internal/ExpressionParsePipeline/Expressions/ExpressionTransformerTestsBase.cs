using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    public abstract class ExpressionTransformerTestsBase
    {
        internal IOneSMappingProvider MappingProvider
        {
            get { return _mappingProviderMock.Object; }    
        }
        private Mock<IOneSMappingProvider> _mappingProviderMock;

        internal QueryParseContext Context
        {
            get { return _context; }
        }
        private QueryParseContext _context;
        
        protected const string ID_FIELD_NAME = "id";
        protected const string NAME_FIELD_NAME = "name";
        protected const string PRICE_FIELD_NAME = "price";
        protected const string QUANTITY_FIELD_NAME = "quantity";
        protected const string VALUE_FIELD_NAME = "value";
        protected const string CREATED_DATE_FIELD_NAME = "created_date";
        protected const string ADD_INFO_FIELD_NAME = "add_info";
        protected const string REF_FIELD_NAME = "reference";

        protected const string ADD_INFO_TABLE_NAME = "add_info_table";
        protected const string REFERENCE_TABLE = "some_table";

        [SetUp]
        public void SetUpMappingProvider()
        {
            _context = new QueryParseContext();

            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);

            _mappingProviderMock
                .Setup(p => p.IsDataType(It.IsAny<Type>()))
                .Returns(false);

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("?")
                    .FieldMap(d => d.Id, ID_FIELD_NAME)
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                    .FieldMap(d => d.Price, PRICE_FIELD_NAME)
                    .FieldMap(d => d.Quantity, QUANTITY_FIELD_NAME)
                    .FieldMap(d => d.Value, VALUE_FIELD_NAME)
                    .FieldMap(d => d.CreatedDate, CREATED_DATE_FIELD_NAME)
                    .FieldMap(d => d.AddInfo, ADD_INFO_FIELD_NAME)
                    .FieldMap(d => d.Reference, REF_FIELD_NAME)
                .End();

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<AdditionalInfo>(ADD_INFO_TABLE_NAME)
                .End();

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<RefData>(REFERENCE_TABLE)
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                    .FieldMap(d => d.Price, PRICE_FIELD_NAME)
                .End();

            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeDataWithWeakTyping>("?")
                    .FieldMap(d => d.Id, ID_FIELD_NAME)
                    .FieldMap(d => d.Name, NAME_FIELD_NAME)
                .End();
        }

        /// <summary>
        /// Проверка выражения, на то что оно является литералом с ожидаемым значением.
        /// </summary>
        /// <param name="expectedValue">Ожидаемое значение.</param>
        /// <param name="testedExpression">Проверяемое выражение.</param>
        internal static void AssertLiteral(object expectedValue, SqlExpression testedExpression)
        {
            var literal = AssertEx.IsInstanceAndCastOf<SqlLiteralExpression>(testedExpression);
            Assert.AreEqual(expectedValue, literal.Value);
        }

        /// <summary>
        /// Проверка выражения, на то, что оно является выражением доступа к полю таблицы.
        /// </summary>
        /// <param name="expectedFieldName">Ожидаемое имя поля.</param>
        /// <param name="testedExpression">Проверяемое выражение.</param>
        internal static void AssertField(string expectedFieldName, SqlExpression testedExpression)
        {
            var field = AssertEx.IsInstanceAndCastOf<SqlFieldExpression>(testedExpression);

            Assert.IsInstanceOf<SqlDefaultTableExpression>(field.Table);
            Assert.AreEqual(expectedFieldName, field.FieldName);
        }

        /// <summary>
        /// Тестовый тип записи.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;

            public int Quantity;

            public int Value;

            public DateTime CreatedDate;

            public object AddInfo;

            public object Reference;
        }

        public sealed class AdditionalInfo
        {}

        /// <summary>
        /// Тестовый тип записи для тестирования ссылки на нее.
        /// </summary>
        public sealed class RefData
        {
            public string Name;

            public decimal Price;
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
    }
}
