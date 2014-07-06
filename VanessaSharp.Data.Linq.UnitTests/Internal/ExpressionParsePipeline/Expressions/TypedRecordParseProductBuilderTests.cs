using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="TypedRecordParseProductBuilder"/>
    /// </summary>
    [TestFixture]
    public sealed class TypedRecordParseProductBuilderTests
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

        /// <summary>
        /// Тестирование метода <see cref="TypedRecordParseProductBuilder.GetTypedRecordSourceName{T}"/>.
        /// </summary>
        [Test]
        public void TestGetTypedRecordSourceName()
        {
            // Arrange
            const string SOURCE_NAME = "Тест";
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>(SOURCE_NAME)
                .End();

            // Act
            var result = _testedInstance.GetTypedRecordSourceName<SomeData>();

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
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeData>("???")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.Id, "Идентификатор")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<SomeData>();

            // Assert
            CollectionAssert.AreEqual(
                new[] { "Наименование", "Идентификатор", "Цена" }.Select(fieldName => new SqlFieldExpression(fieldName)),
                result.Columns
                );

            ItemReaderTester
                .For(result.SelectionFunc, 3)
                    .Field(0, d => d.Name, c => c.ToString(null), "Тест")
                    .Field(1, d => d.Id, c => c.ToInt32(null), 34)
                    .Field(2, d => d.Price, c => c.ToDecimal(null), 56.23m)
                .Test();
        }

        /// <summary>
        /// Тестирование метода <see cref="TypedRecordParseProductBuilder.GetSelectPartParseProductForTypedRecord{T}"/>
        /// с полем типа <see cref="object"/>.
        /// </summary>
        [Test]
        public void TestGetSelectPartParseProductForTypedRecordWithWeakTyping()
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingFor<SomeDataWithWeakTyping>("???")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<SomeDataWithWeakTyping>();

            // Assert
            CollectionAssert.AreEqual(
                new[] { "Наименование", "Цена" }.Select(fieldName => new SqlFieldExpression(fieldName)),
                result.Columns
                );

            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, d => (string)(OneSValue)d.Name, c => c.ToString(null), "Тест")
                    .Field(1, d => (decimal)d.Price, c => c.ToDecimal(null), 454.56m)
                .Test();
        }

        /// <summary>
        /// Тип тестовой типизированной записи.
        /// </summary>
        public sealed class SomeData
        {
            public string Name;

            public int Id { get; set; }

            public decimal Price { get; set; }
        }

        /// <summary>
        /// Тип тестовой типизированной записи,
        /// с полями слабой типизации <see cref="object"/> и <see cref="OneSValue"/>.
        /// </summary>
        public sealed class SomeDataWithWeakTyping
        {
            public object Name { get; set; }

            public OneSValue Price { get; set; }
        }
    }
}
