using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тестирование <see cref="OneSConnectionStringBuilder.KnownKeywordsCollection"/>.</summary>
    [TestFixture]
    public sealed class KnownKeywordsCollectionTests
    {
        private const string KEY_1 = "KeyFirst";
        private const string KEY_2 = "KeySecond";

        private const string EXCEPTED_KEY = "KeyThird";

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnectionStringBuilder.KnownKeywordsCollection _testingInstance;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        [SetUp]
        public void SetUp()
        {
            _testingInstance = new OneSConnectionStringBuilder.KnownKeywordsCollection(KEY_1, KEY_2);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSConnectionStringBuilder.KnownKeywordsCollection.Contains"/>
        /// </summary>
        /// <param name="key">Ключ передаваемый в тестируемый метод.</param>
        /// <param name="expectedNormalizeKey">Ожидаемый номализованный ключ.</param>
        /// <returns>Возвращает результат тестируемого метода.</returns>
        [Test]
        [TestCase(KEY_1, KEY_1, Result = true, Description = "Случай когда ключ есть в коллекции и они равны.")]
        [TestCase("keyFIRST", KEY_1, Result = true, Description = "Случай когда нормализованный ключ есть в коллекции.")]
        [TestCase(EXCEPTED_KEY, EXCEPTED_KEY, Result = false, Description = "Случай когда ключа нет в коллекции.")]
        public bool TestContains(string key, string expectedNormalizeKey)
        {
            string actualNormalizeKey;
            var result = _testingInstance.Contains(key, out actualNormalizeKey);

            Assert.AreEqual(expectedNormalizeKey, actualNormalizeKey);

            return result;
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSConnectionStringBuilder.KnownKeywordsCollection.GetNormalizeKeyword"/>
        /// </summary>
        /// <param name="key">Ключ передаваемый в тестируемый метод.</param>
        /// <returns>Возвращает результат тестируемого метода.</returns>
        [Test]
        [TestCase(KEY_1, Result = KEY_1, Description = "Случай когда ключ есть в коллекции и они равны.")]
        [TestCase("keyFIRST", Result = KEY_1, Description = "Случай когда нормализованный ключ есть в коллекции.")]
        [TestCase(EXCEPTED_KEY, Result = EXCEPTED_KEY, Description = "Случай когда ключа нет в коллекции.")]
        public string TestGetNormalizeKeyword(string key)
        {
            return _testingInstance.GetNormalizeKeyword(key);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSConnectionStringBuilder.KnownKeywordsCollection.Collection"/>.
        /// </summary>
        [Test]
        public void TestCollection()
        {
            CollectionAssert.AreEquivalent(new[] { KEY_1, KEY_2 }, _testingInstance.Collection);
        }
    }
}
