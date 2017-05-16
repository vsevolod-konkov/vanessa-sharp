using System.Collections.Generic;
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
                .BeginSetupGetTypeMappingForRoot<SomeData>(SOURCE_NAME)
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
                .BeginSetupGetTypeMappingForRoot<SomeData>("???")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.Id, "Идентификатор")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<SomeData>();

            // Assert
            CollectionAssert.AreEqual(
                new[] { "Наименование", "Идентификатор", "Цена" }.Select(fieldName => new SqlFieldExpression(SqlDefaultTableExpression.Instance, fieldName)),
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
                .BeginSetupGetTypeMappingForRoot<SomeDataWithWeakTyping>("???")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<SomeDataWithWeakTyping>();

            // Assert
            CollectionAssert.AreEqual(
                new[] { "Наименование", "Цена" }.Select(fieldName => new SqlFieldExpression(SqlDefaultTableExpression.Instance, fieldName)),
                result.Columns
                );

            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, d => (string)(OneSValue)d.Name, c => c.ToString(null), "Тест")
                    .Field(1, d => (decimal)d.Price, c => c.ToDecimal(null), 454.56m)
                .Test();
        }

        /// <summary>
        /// Тестирование метода <see cref="TypedRecordParseProductBuilder.GetSelectPartParseProductForTypedRecord{T}"/>
        /// с полем типизированной табличной части.
        /// </summary>
        [Test]
        public void TestGetSelectPartParseProductForTypeWithTableParts()
        {
            // Arrange
            _mappingProviderMock
                .BeginSetupGetTypeMappingForRoot<SomeComplexData>("???")
                    .FieldMap(d => d.Name, "Наименование")
                    .FieldMap(d => d.EnumerableTablePart, "Перечисление", OneSDataColumnKind.TablePart)
                .End();

            _mappingProviderMock
                .BeginSetupGetTypeMappingForTablePart<SomeData>()
                    .FieldMap(d => d.Id, "Идентификатор")
                    .FieldMap(d => d.Price, "Цена")
                .End();

            // Act
            var result = _testedInstance.GetSelectPartParseProductForTypedRecord<SomeComplexData>();

            // Assert
            Assert.AreEqual(2, result.Columns.Count);

            Assert.AreEqual(
                new SqlFieldExpression(SqlDefaultTableExpression.Instance, "Наименование"),
                result.Columns[0]
                );
            
            Assert.AreEqual(
                new SqlFieldsGroupExpression(new SqlFieldExpression(SqlDefaultTableExpression.Instance, "Перечисление"), 
                    new[]{ "Идентификатор", "Цена" }.Select(name => new SqlFieldExpression(SqlDefaultTableExpression.Instance, name)).ToArray()),
                result.Columns[1]
                );

            ItemReaderTester
                .For(result.SelectionFunc, 2)
                    .Field(0, d => d.Name, c => c.ToString(null), "Тест")
                    .BeginTablePart(1, d => d.EnumerableTablePart, 2)
                        .Field(0, d => d.Id, c => c.ToInt32(null), 4)
                        .Field(1, d => d.Price, c => c.ToDecimal(null), 454.56m)
                    .EndTablePart
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

        /// <summary>
        /// Тип тестовой записи с перечислением типизированной табличной частью.
        /// </summary>
        public sealed class SomeComplexData
        {
            public string Name { get; set; }

            public IEnumerable<SomeData> EnumerableTablePart { get; set; } 
        }
    }
}
