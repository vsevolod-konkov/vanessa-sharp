using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataRecord"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDataRecordTests
    {
        /// <summary>Конвертер значений.</summary>
        private static readonly IValueConverter _valueConverter
            = new Mock<IValueConverter>(MockBehavior.Strict).Object;

        private OneSValue _(object value)
        {
            return new OneSValue(value, _valueConverter);
        }

        /// <summary>Создание записи.</summary>
        private OneSDataRecord CreateRecord(IEnumerable<KeyValuePair<string, object>> fieldAndValues)
        {
            return CreateRecord(To(fieldAndValues, _));
        }
        
        /// <summary>Создание записи.</summary>
        private static OneSDataRecord CreateRecord(IEnumerable<KeyValuePair<string, OneSValue>> fieldAndValues)
        {
            var fields = GetFields(fieldAndValues).ToReadOnly();
            var values = GetValues(fieldAndValues).ToReadOnly();

            return new OneSDataRecord(fields, values);
        }

        private static IEnumerable<KeyValuePair<string, TOutput>> To<TInput, TOutput>(
            IEnumerable<KeyValuePair<string, TInput>> pairs, Func<TInput, TOutput> selector)
        {
            return pairs.Select(p => new KeyValuePair<string, TOutput>(p.Key, selector(p.Value)));
        }

        private static IEnumerable<string> GetFields<T>(IEnumerable<KeyValuePair<string, T>> fieldAndValues)
        {
            return fieldAndValues.Select(p => p.Key);
        }

        private static IEnumerable<T> GetValues<T>(IEnumerable<KeyValuePair<string, T>> fieldAndValues)
        {
            return fieldAndValues.Select(p => p.Value);
        }

            /// <summary>
        /// Тестирование <see cref="OneSDataRecord.Fields"/>.
        /// </summary>
        [Test]
        public void TestFields()
        {
            var fieldsAndValues = new Dictionary<string, object>
                    {
                        { "Id", null },
                        { "Name", null },
                        { "Value", null }
                    };

            var testedInstance = CreateRecord(fieldsAndValues);

            CollectionAssert.AreEqual(GetFields(fieldsAndValues), testedInstance.Fields);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.GetValues(object[])"/>
        /// и <see cref="OneSDataRecord.GetValues(OneSValue[])"/>.
        /// </summary>
        private static int TestGetValues<T>(IDictionary<string, T> fieldsAndValues, Func<T[], int> testedAction, int delta)
        {
            // Arrange
            var actualValuesCount = fieldsAndValues.Count;
            var bufferSize = actualValuesCount + delta;
            var buffer = new T[bufferSize];

            // Act
            var result = testedAction(buffer);

            // Assert
            CollectionAssert.AreEqual(
                expected: GetValues(fieldsAndValues).Take(result),
                actual: buffer.Take(result)
                );

            return result - actualValuesCount;
        }

        /// <summary>Тестирование <see cref="OneSDataRecord.GetValues(object[])"/>.</summary>
        [Test]
        [TestCase(-2, Result = -2)]
        [TestCase(0, Result = 0)]
        [TestCase(2, Result = 0)]
        public int TestGetValues(int delta)
        {
            // Arrange
            var fieldsAndValues = new Dictionary<string, object>
                    {
                        { "Id", 2 },
                        { "Name", "Test" },
                        { "Value", null }
                    };
            var testedInstance = CreateRecord(fieldsAndValues);

            // Act & Assert
            return TestGetValues(fieldsAndValues, testedInstance.GetValues, delta);
        }

        /// <summary>Тестирование <see cref="OneSDataRecord.GetValues(OneSValue[])"/>.</summary>
        [Test]
        [TestCase(-2, Result = -2)]
        [TestCase(0, Result = 0)]
        [TestCase(2, Result = 0)]
        public int TestGetOneSValues(int delta)
        {
            // Arrange
            var fieldsAndValues = new Dictionary<string, OneSValue>
                    {
                        { "Id", _(2) },
                        { "Name", _("Test") },
                        { "Value", _(null) }
                    };
            var testedInstance = CreateRecord(fieldsAndValues);

            return TestGetValues(fieldsAndValues, testedInstance.GetValues, delta);
        }

        /// <summary>Генерация колонок и значений.</summary>
        /// <param name="count">Количество колонок.</param>
        /// <param name="fieldNames">Имена колонок.</param>
        /// <param name="values">Значения.</param>
        private void GenerateFieldsAndValues(int count, out IList<string> fieldNames, out ReadOnlyCollection<OneSValue> values)
        {
            fieldNames = Enumerable.Range(0, count).Select(i => "Field" + i).ToArray();
            values = Enumerable.Range(0, count).Select(i => new object()).Select(_).ToReadOnly();
        }

        /// <summary>Генерация колонок, значений и записи.</summary>
        /// <param name="count">Количество колонок.</param>
        /// <param name="values">Значения.</param>
        private OneSDataRecord GenerateFieldsAndValues(int count, out ReadOnlyCollection<OneSValue> values)
        {
            IList<string> fieldNames;
            GenerateFieldsAndValues(count, out fieldNames, out values);

            return new OneSDataRecord(fieldNames.ToReadOnly(), values);
        }

        /// <summary>Тестирование получения по индексу.</summary>
        /// <param name="testedAction">Тестируемое действие.</param>
        private void TestGetValueByindex(Func<OneSDataRecord, int, OneSValue> testedAction)
        {
            // Arrange
            ReadOnlyCollection<OneSValue> values;
            var testedInstance = GenerateFieldsAndValues(4, out values);

            const int TESTED_INDEX = 2;

            // Act
            var actualValue = testedAction(testedInstance, TESTED_INDEX);

            // Assert
            Assert.AreSame(values[TESTED_INDEX], actualValue);
        }

        /// <summary>Тестирование <see cref="OneSDataRecord.GetValue(int)"/>.</summary>
        [Test]
        public void TestGetValueByIndex()
        {
            TestGetValueByindex((testedInstance, index) => testedInstance.GetValue(index));
        }

        /// <summary>Тестирование <see cref="OneSDataRecord.Item(int)"/>.</summary>
        [Test]
        public void TestItemByIndex()
        {
            TestGetValueByindex((testedInstance, index) => testedInstance[index]);
        }

        /// <summary>
        /// Тестирование получения значения по имени колонки
        /// в случае, если колонка с таким именем существует.
        /// </summary>
        private void TestGetValueByNameWhenColumnExists(Func<OneSDataRecord, string, OneSValue> testedAction)
        {
            // Arrange
            IList<string> fieldNames;
            ReadOnlyCollection<OneSValue> values;
            GenerateFieldsAndValues(4, out fieldNames, out values);

            const int TESTED_INDEX = 2;
            const string TESTED_NAME = "TestField";

            fieldNames[TESTED_INDEX] = TESTED_NAME;
            var expectedValue = values[TESTED_INDEX];
            var testedInstance = new OneSDataRecord(fieldNames.ToReadOnly(), values);

            // Act
            var actualValue = testedAction(testedInstance, TESTED_NAME);

            // Assert
            Assert.AreSame(expectedValue, actualValue);
        }

        /// <summary>
        /// Тестирование получения значения по имени колонки
        /// в случае, если колонка с таким именем не существует.
        /// </summary>
        private void TestGetValueByNameWhenColumnIsNotExist(Func<OneSDataRecord, string, OneSValue> testedAction)
        {
            // Arrange
            ReadOnlyCollection<OneSValue> values;
            var testedInstance = GenerateFieldsAndValues(4, out values);

            // Act
            Assert.Throws<KeyNotFoundException>(() =>
                {
                    var actualValue = testedAction(testedInstance, "TestField");
                });
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.GetValue(string)"/>
        /// в случае, если колонка с таким именем существует.
        /// </summary>
        [Test]
        public void TestGetValueByNameWhenColumnExists()
        {
            TestGetValueByNameWhenColumnExists((testedInstance, name) 
                => testedInstance.GetValue(name));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.GetValue(string)"/>
        /// в случае, если колонка с таким именем не существует.
        /// </summary>
        [Test]
        public void TestGetValueByNameWhenColumnIsNotExist()
        {
            TestGetValueByNameWhenColumnIsNotExist((testedInstance, name) => testedInstance.GetValue(name));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.Item(string)"/>
        /// в случае, если колонка с таким именем существует.
        /// </summary>
        [Test]
        public void TestItemByNameWhenColumnExists()
        {
            TestGetValueByNameWhenColumnExists((testedInstance, name)
                => testedInstance[name]);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.Item(string)"/>
        /// в случае, если колонка с таким именем не существует.
        /// </summary>
        [Test]
        public void TestItemByNameWhenColumnIsNotExist()
        {
            TestGetValueByNameWhenColumnIsNotExist((testedInstance, name) => testedInstance[name]);
        }
    }
}
