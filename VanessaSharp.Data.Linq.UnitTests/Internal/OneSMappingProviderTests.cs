using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            [OneSDataColumn("Список", OneSDataColumnKind.TablePart)]
            public string[] List { get; set; }
        }

        public struct CorrectTablePartType
        {
            [OneSDataColumn("Наименование")]
            public string Name { get; set; }

            [OneSDataColumn("Количество")]
            public int Count { get; set; }
        }

        [OneSTablePartOwner(typeof(CorrectDataType))]
        public class TablePartDefinedOwner
        {}

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если проверяемый тип полностью корректен.
        /// </summary>
        [Test]
        public void TestCheckTypeWhenRootCorrectType()
        {
            _testedInstance.CheckDataType(OneSDataLevel.Root, typeof(CorrectDataType));
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/> уровня табличной части
        /// в случае если у проверяемого типа есть колонки типа табличной части.
        /// </summary>
        [Test]
        public void TestCheckTypeWhenTablePartTypeHasTablePartColumn()
        {
            var actualException = Assert.Throws<InvalidDataTypeException>(() =>
                _testedInstance.CheckDataType(OneSDataLevel.TablePart, typeof(CorrectDataType)));

            Assert.AreEqual(
                "Тип уровня табличной части имеет свойство \"List\" помеченное атрибутом \"VanessaSharp.Data.Linq.OneSDataColumnAttribute\", которое также является табличной частью.",
                actualException.Reason
                );
        }

        public struct TypeDoesntHaveDataSourceAttribute
        { }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае, если у проверяемого типа нет атрибута
        /// <see cref="OneSDataSourceAttribute"/>
        ///  и есть требование чтобы он был корневым типом.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidDataTypeException))]
        public void TestCheckRootDataTypeWhenTypeDoesntHaveDataSourceAttribute()
        {
            _testedInstance.CheckDataType(OneSDataLevel.Root, typeof(TypeDoesntHaveDataSourceAttribute));
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.CheckDataType"/>
        /// в случае если у проверяемого типа нет атрибута
        /// <see cref="OneSDataSourceAttribute"/>
        /// и он должен быть уровня табличной части.
        /// </summary>
        [Test]
        public void TestCheckTablePartDataTypeWhenCorrectType()
        {
            _testedInstance.CheckDataType(OneSDataLevel.TablePart, typeof(CorrectTablePartType));
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
        public void TestCheckDataTypeWhenTypeIsAbstract([Values(OneSDataLevel.Root, OneSDataLevel.TablePart)] Enum level)
        {
            _testedInstance.CheckDataType((OneSDataLevel)level, typeof(AbstractDataType));
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
        public void TestCheckDataTypeWhenTypeDoesntHaveConstructorWithoutArgs([Values(OneSDataLevel.Root, OneSDataLevel.TablePart)] Enum level)
        {
            _testedInstance.CheckDataType((OneSDataLevel)level, typeof(TypeDoesntHaveConstructorWithoutArgs));
        }

        private void AssertFieldMappingsForCorrectType(ReadOnlyCollection<OneSFieldMapping> actualFieldMappings)
        {
            Assert.AreEqual(3, actualFieldMappings.Count);

            AssertFieldMapping(actualFieldMappings, "Name", "Наименование", OneSDataColumnKind.Property);
            AssertFieldMapping(actualFieldMappings, "Price", "Цена", OneSDataColumnKind.Property);
            AssertFieldMapping(actualFieldMappings, "List", "Список", OneSDataColumnKind.TablePart);
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.GetRootTypeMapping"/>
        /// в случае корректного типа.
        /// </summary>
        [Test]
        public void TestGetRootTypeMappingWhenCorrectType()
        {
            // Act
            var result = _testedInstance.GetRootTypeMapping(typeof(CorrectDataType));

            // Assert
            Assert.AreEqual("Справочник.Тест", result.SourceName);
            AssertFieldMappingsForCorrectType(result.FieldMappings);
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.GetTablePartTypeMappings"/>
        /// в случае когда свойство типа является табличной частью.
        /// </summary>
        [Test]
        public void TestGetTablePartTypeMappingsWhenCorrectRootType()
        {
            // Act
            var result = _testedInstance.GetTablePartTypeMappings(typeof(CorrectDataType));

            // Assert
            AssertFieldMappingsForCorrectType(result.FieldMappings);
            Assert.IsNull(result.OwnerType);
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.GetTablePartTypeMappings"/>
        /// в случае когда корректного типа.
        /// </summary>
        [Test]
        public void TestGetTablePartTypeMappingsWhenCorrectType()
        {
            // Act
            var result = _testedInstance.GetTablePartTypeMappings(typeof(CorrectTablePartType));

            // Assert
            Assert.IsNull(result.OwnerType);

            var actualFieldMappings = result.FieldMappings;
            Assert.AreEqual(2, actualFieldMappings.Count);

            AssertFieldMapping(actualFieldMappings, "Name", "Наименование", OneSDataColumnKind.Property);
            AssertFieldMapping(actualFieldMappings, "Count", "Количество", OneSDataColumnKind.Property);
        }

        /// <summary>
        /// Тестирование <see cref="OneSMappingProvider.GetTablePartTypeMappings"/>
        /// для получение типа владельца табличной части.
        /// </summary>
        [Test]
        public void TestGetTablePartOwnerType()
        {
            Assert.IsTrue(_testedInstance.IsDataType(OneSDataLevel.TablePart, typeof(TablePartDefinedOwner)));
            
            // Act
            var result = _testedInstance.GetTablePartTypeMappings(typeof(TablePartDefinedOwner));

            // Assert
            Assert.AreSame(typeof(CorrectDataType), result.OwnerType);
        }

        /// <summary>
        /// Проверка фактического соответствия полей на ожидаемые значения.</summary>
        /// <param name="actualFieldMappings">Фактические соответствия членам CLR-типа полям 1С.</param>
        /// <param name="memberName">Имя члена типа в соответствии.</param>
        /// <param name="expectedFieldName">Ожидаемое наименование поля.</param>
        /// <param name="expectedKind">Ожидаемый тип.</param>
        private static void AssertFieldMapping(IEnumerable<OneSFieldMapping> actualFieldMappings,  string memberName, string expectedFieldName, OneSDataColumnKind expectedKind)
        {
            var actualFieldMapping = actualFieldMappings.Single(f => f.MemberInfo.Name == memberName);
            Assert.AreEqual(expectedFieldName, actualFieldMapping.FieldName);
            Assert.AreEqual(expectedKind, actualFieldMapping.DataColumnKind);
        }
    }
}
