using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="DataReaderFieldInfoFixedCollection"/>.
    /// </summary>
    [TestFixture]
    public sealed class DataReaderFieldInfoFixedCollectionTests
    {
        /// <summary>Генерация полей.</summary>
        /// <param name="count">Количество генерируемых полей.</param>
        /// <param name="nameGenerator">Генератор имени поля.</param>
        private static ReadOnlyCollection<DataReaderFieldInfo> GenerateFields(int count, Func<int, string> nameGenerator = null)
        {
            if (nameGenerator == null)
                nameGenerator = i => "Field" + i;
            
            return new ReadOnlyCollection<DataReaderFieldInfo>(
                Enumerable
                    .Range(0, count)
                    .Select(i => new DataReaderFieldInfo(nameGenerator(i), typeof(object)))
                    .ToArray()
                );
        }
        
        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoFixedCollection.Count"/>.
        /// </summary>
        [Test]
        public void TestCount()
        {
            // Arrange
            const int EXPECTED_COUNT = 3;

            var testedInstance = new DataReaderFieldInfoFixedCollection(
                GenerateFields(EXPECTED_COUNT));


            // Act
            var actualResult = testedInstance.Count;

            // Assert
            Assert.AreEqual(EXPECTED_COUNT, actualResult);
        }

        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoFixedCollection.Item"/>
        /// </summary>
        [Test]
        public void TestIndexator([Values(0, 1, 2)] int ordinal)
        {
            // Arrange
            const int COUNT = 5;

            var fields = GenerateFields(COUNT);
            var testedInstance = new DataReaderFieldInfoFixedCollection(fields);

            // Act
            var actualResult = testedInstance[ordinal];

            // Assert
            Assert.AreSame(fields[ordinal], actualResult);
        }

        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoFixedCollection.IndexOf"/>
        /// в случае если элемента с данным именем нет в коллекции.
        /// </summary>
        [Test]
        public void TestIndexOfWhenDoesNotExist()
        {
            // Arrange
            var testedInstance = new DataReaderFieldInfoFixedCollection(
                GenerateFields(5, i => "NormalName" + i));

            // Act
            var actualResult = testedInstance.IndexOf("SpecialName");

            // Assert
            Assert.AreEqual(-1, actualResult);
        }

        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoFixedCollection.IndexOf"/>
        /// в случае если есть элемент с заданным именем в коллекции.
        /// </summary>
        [Test]
        public void TestIndexOfWhenExists()
        {
            // Arrange
            const int EXPECTED_INDEX = 3;
            const string ARGUMENT = "SpecialName";

            Func<int, string> nameGenerator = i => 
                (i == EXPECTED_INDEX) 
                    ? ARGUMENT 
                    : "NormalName" + i;

            var testedInstance = new DataReaderFieldInfoFixedCollection(GenerateFields(6, nameGenerator));

            // Act
            var actualResult = testedInstance.IndexOf(ARGUMENT);

            // Assert
            Assert.AreEqual(EXPECTED_INDEX, actualResult);
        }
    }
}
