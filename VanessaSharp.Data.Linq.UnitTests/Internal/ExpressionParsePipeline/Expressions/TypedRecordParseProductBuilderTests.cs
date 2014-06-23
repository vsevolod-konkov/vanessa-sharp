using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="TypedRecordParseProductBuilder"/>
    /// </summary>
    [TestFixture]
    public sealed class TypedRecordParseProductBuilderTests : TestsBase
    {
        private Mock<IOneSMappingProvider> _mappingProviderMock;

        private TypedRecordParseProductBuilder _testedInstance;

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
            _testedInstance = new TypedRecordParseProductBuilder(_mappingProviderMock.Object);
        }

        public sealed class AnyData
        {
            public string Name;

            public int Id { get; set; }

            public decimal Price { get; set; }
        }

        private static OneSFieldMapping CreateFieldMapping<T>(Expression<Func<T, object>> memberExpression,
                                                              string fieldName)
        {
            var memberInfo = GetMemberExpression(memberExpression.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return memberExpression;

            return GetMemberExpression(((UnaryExpression)expression).Operand);
        }

        /// <summary>
        /// Тестирование метода <see cref="TypedRecordParseProductBuilder.GetTypedRecordSourceName{T}"/>.
        /// </summary>
        [Test]
        public void TestGetTypedRecordSourceName()
        {
            // Arrange
            const string SOURCE_NAME = "Тест";
            var anyDataType = typeof(AnyData);
            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(anyDataType))
                .Returns(new OneSTypeMapping(SOURCE_NAME,
                                             new ReadOnlyCollection<OneSFieldMapping>(new OneSFieldMapping[0])));

            // Act
            var result = _testedInstance.GetTypedRecordSourceName<AnyData>();

            // Assert
            Assert.AreEqual(SOURCE_NAME, result);
        }

        /// <summary>
        /// Тестирование метода <see cref="TypedRecordParseProductBuilder.GetSelectPartParseProductForTypedRecord{T}"/>.
        /// </summary>
        [Test]
        public void TestGetSelectPartParseProductForTypedRecord()
        {
            // Arrange
            var typeMapping = new OneSTypeMapping(
                "???",
                new ReadOnlyCollection<OneSFieldMapping>(
                    new []
                        {
                            CreateFieldMapping<AnyData>(d => d.Name, "Наименование"),
                            CreateFieldMapping<AnyData>(d => d.Id, "Идентификатор"),
                            CreateFieldMapping<AnyData>(d => d.Price, "Цена")
                        }));

            var anyDataType = typeof(AnyData);
            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(anyDataType))
                .Returns(typeMapping);

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<AnyData>();

            // Assert
            Assert.AreEqual(3, result.Columns.Count);

            AssertFieldExpression("Наименование", result.Columns[0]);
            AssertFieldExpression("Идентификатор", result.Columns[1]);
            AssertFieldExpression("Цена", result.Columns[2]);

            const string TEST_NAME = "Тест";
            const int TEST_ID = 34;
            const decimal TEST_PRICE = 56.23m;

            var values = new object[] {TEST_NAME, TEST_ID, TEST_PRICE};

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

            var actualItem = result.SelectionFunc(valueConverterMock.Object, values);

            Assert.IsNotNull(actualItem);
            Assert.AreEqual(TEST_NAME, actualItem.Name);
            Assert.AreEqual(TEST_ID, actualItem.Id);
            Assert.AreEqual(TEST_PRICE, actualItem.Price);
        }

        private static void AssertFieldExpression(string expectedFieldName, SqlExpression columnExpression)
        {
            var fieldExpression = AssertAndCast<SqlFieldExpression>(columnExpression);
            Assert.AreEqual(expectedFieldName, fieldExpression.FieldName);
        }
    }
}
