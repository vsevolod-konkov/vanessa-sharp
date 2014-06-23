using System.Linq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тестирование <see cref="OneSMappingProvider"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSMappingProviderTests
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSMappingProvider _testedInstance;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new OneSMappingProvider();
        }

        public abstract class DataTypeBase
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }
        }

        [OneSDataSource("Справочник.Тест")]
        public sealed class CorrectDataType : DataTypeBase
        {
            [OneSDataColumn("Цена")]
            public decimal Price { get; set; }
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если проверяемый тип полностью корректен.
        /// </summary>
        [Test]
        public void TestCheckDataTypeWhenCorrectType()
        {
            _testedInstance.CheckDataType(typeof(CorrectDataType));
        }

        public struct TypeDoesntHaveDataSourceAttribute
        { }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если у проверяемого типа нет атрибута
        /// <see cref="OneSDataSourceAttribute"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidDataTypeException))]
        public void TestCheckDataTypeWhenTypeDoesntHaveDataSourceAttribute()
        {
            _testedInstance.CheckDataType(typeof(TypeDoesntHaveDataSourceAttribute));
        }

        [OneSDataSource("Справочник.Тест")]
        public abstract class AbstractDataType
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если проверяемый тип абстрактен.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidDataTypeException))]
        public void TestCheckDataTypeWhenTypeIsAbstract()
        {
            _testedInstance.CheckDataType(typeof(AbstractDataType));
        }

        [OneSDataSource("Справочник.Тест")]
        public sealed class TypeDoesntHaveConstructorWithoutArgs
        {
            public TypeDoesntHaveConstructorWithoutArgs(int num)
            {}
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если у проверяемого типа нет публичного конструктора без аргументов.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidDataTypeException))]
        public void TestCheckDataTypeWhenTypeDoesntHaveConstructorWithoutArgs()
        {
            _testedInstance.CheckDataType(typeof(TypeDoesntHaveConstructorWithoutArgs));
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.GetTypeMapping"/>
        /// в случае корректного типа.
        /// </summary>
        [Test]
        public void TestGetTypeMappingWhenCorrectType()
        {
            // Act
            var result = _testedInstance.GetTypeMapping(typeof(CorrectDataType));

            // Assert
            Assert.AreEqual("Справочник.Тест", result.SourceName);

            Assert.AreEqual(2, result.FieldMappings.Count);

            var nameFieldMapping = result.FieldMappings.Single(f => f.MemberInfo.Name == "Name");
            Assert.AreEqual("Наименование", nameFieldMapping.FieldName);

            var priceFieldMapping = result.FieldMappings.Single(f => f.MemberInfo.Name == "Price");
            Assert.AreEqual("Цена", priceFieldMapping.FieldName);
        }
    }
}
