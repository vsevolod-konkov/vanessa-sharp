using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSParameterTests
{
    /// <summary>Тесты на <see cref="OneSParameter"/>.</summary>
    [TestFixture]
    public sealed class ChangePropertiesTests
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSParameter _testedInstance;

        /// <summary>
        /// Инициализация теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new OneSParameter();
        }

        /// <summary>
        /// Получение значений перечисления за исключением одного значения
        /// <paramref name="excludedValue"/>.
        /// </summary>
        /// <typeparam name="T">Тип перечисления.</typeparam>
        private static IEnumerable<T> GetEnumValuesWithoutOne<T>(T excludedValue)
            where T : struct
        {
            object boxedExcludedValue = excludedValue;

            return Enum
                .GetValues(typeof(T))
                .Cast<object>()
                .Where(v => !v.Equals(boxedExcludedValue))
                .Cast<T>();
        }

        /// <summary>
        /// Получение экземпляров <see cref="DbType"/>
        /// отличных от <see cref="DbType.Object"/>.
        /// </summary>
        private static IEnumerable<DbType> GetOtherDbTypes()
        {
            return GetEnumValuesWithoutOne(DbType.Object);
        }

        /// <summary>
        /// Тестирование установки значения свойства <see cref="OneSParameter.DbType"/>
        /// отличного от <see cref="DbType.Object"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetOtherDbType([ValueSource("GetOtherDbTypes")] DbType dbType)
        {
            _testedInstance.DbType = dbType;
        }

        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.DbType"/>
        /// значением <see cref="DbType.Object"/>.
        /// </summary>
        [Test]
        public void TestSetObjectDbType()
        {
            _testedInstance.DbType = DbType.Object;
            Assert.AreEqual(DbType.Object, _testedInstance.DbType);
        }

        /// <summary>
        /// Получение экземпляров <see cref="ParameterDirection"/>
        /// отличных от <see cref="ParameterDirection.Input"/>.
        /// </summary>
        private static IEnumerable<ParameterDirection> GetOtherParameterDirection()
        {
            return GetEnumValuesWithoutOne(ParameterDirection.Input);
        }

        /// <summary>
        /// Тестирование установки значения свойства <see cref="OneSParameter.Direction"/>
        /// отличного от <see cref="ParameterDirection.Input"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetOtherDirection([ValueSource("GetOtherParameterDirection")] ParameterDirection direction)
        {
            _testedInstance.Direction = direction;
        }

        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.Direction"/>
        /// значением <see cref="ParameterDirection.Input"/>.
        /// </summary>
        [Test]
        public void TestSetInputDirection()
        {
            _testedInstance.Direction = ParameterDirection.Input;
            Assert.AreEqual(ParameterDirection.Input, _testedInstance.Direction);
        }

        /// <summary>
        /// Тестирование установки значения <see cref="OneSParameter.IsNullable"/>.
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false, ExpectedException = typeof(NotSupportedException))]
        public void TestSetIsNullable(bool isNullable)
        {
            _testedInstance.IsNullable = isNullable;
            Assert.AreEqual(isNullable, _testedInstance.IsNullable);
        }

        /// <summary>
        /// Тестирование установки имени параметра <see cref="OneSParameter.ParameterName"/>.
        /// </summary>
        [Test]
        public void TestSetParameterName()
        {
            const string PARAMETER_NAME = "Параметр1";

            _testedInstance.ParameterName = PARAMETER_NAME;
            Assert.AreEqual(PARAMETER_NAME, _testedInstance.ParameterName);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameter.ResetDbType"/>.
        /// </summary>
        [Test]
        public void TestResetDbType()
        {
            _testedInstance.ResetDbType();

            Assert.AreEqual(DbType.Object, _testedInstance.DbType);
        }

        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.Size"/>.
        /// </summary>
        [Test]
        [TestCase(0)]
        [TestCase(2, ExpectedException = typeof(NotSupportedException))]
        [TestCase(4, ExpectedException = typeof(NotSupportedException))]
        public void TestSetSize(int size)
        {
            _testedInstance.Size = size;

            Assert.AreEqual(size, _testedInstance.Size);
        }

        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.SourceColumn"/>.
        /// </summary>
        [Test]
        public void TestSetSourceColumn()
        {
            const string SOURCE_COLUMN = "Column1";

            _testedInstance.SourceColumn = SOURCE_COLUMN;
            Assert.AreEqual(SOURCE_COLUMN, _testedInstance.SourceColumn);
        }

        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.SourceColumnNullMapping"/>.
        /// </summary>
        [Test]
        public void TestSetSourceColumnNullMapping([Values(false, true)] bool sourceColumnNullMapping)
        {
            _testedInstance.SourceColumnNullMapping = sourceColumnNullMapping;
            Assert.AreEqual(sourceColumnNullMapping, _testedInstance.SourceColumnNullMapping);
        }

        private static IEnumerable<DataRowVersion> GetSourceVersionValues()
        {
            return Enum
                .GetValues(typeof (DataRowVersion))
                .Cast<DataRowVersion>();
        }
        
        /// <summary>
        /// Тестирование установки свойства <see cref="OneSParameter.SourceVersion"/>.
        /// </summary>
        [Test]
        public void TestSetSourceVersion([ValueSource("GetSourceVersionValues")] DataRowVersion sourceVersion)
        {
            _testedInstance.SourceVersion = sourceVersion;
            Assert.AreEqual(sourceVersion, _testedInstance.SourceVersion);
        }

        private static IEnumerable<TestCaseData> GetTestCasesForSetValues()
        {
            const string STRING_PARAMETER_VALUE = "Значение";
            const int NUMBER_PARAMETER_VALUE = 15;

            return new[]
                {
                    new TestCaseData(STRING_PARAMETER_VALUE, STRING_PARAMETER_VALUE),  
                    new TestCaseData(string.Empty, string.Empty),
                    new TestCaseData(NUMBER_PARAMETER_VALUE, NUMBER_PARAMETER_VALUE),
                    new TestCaseData(null, null),
                    new TestCaseData(DBNull.Value, null)
                            .SetDescription("Тестирование того, что DBNull всегда преобразуется в null")
                };
        }

        [Test]
        [TestCaseSource("GetTestCasesForSetValues")]
        public void TestSetValue(object value, object expectedValue)
        {
            _testedInstance.Value = value;

            Assert.AreEqual(expectedValue, _testedInstance.Value);
        }
    }
}
