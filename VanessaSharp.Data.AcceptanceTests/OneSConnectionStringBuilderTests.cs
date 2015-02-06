using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>Тестирование <see cref="OneSConnectionStringBuilder"/>.</summary>
    [TestFixture]
    public sealed class OneSConnectionStringBuilderTests
    {
        private const string FILE_KEY = "File";
        private const string SERVER_KEY = "Srvr";
        private const string REFERENCE_KEY = "Ref";
        private const string USER_KEY = "Usr";
        private const string PASSWORD_KEY = "Pwd";

        /// <summary>Ожидаемые ключи в строке подключения, не зависимые от варианта поключения.</summary>
        private static readonly ReadOnlyCollection<string> _knownKeys
            = new ReadOnlyCollection<string>(new[] { FILE_KEY, SERVER_KEY, REFERENCE_KEY, USER_KEY, PASSWORD_KEY });

        #region Вспомогательные методы

        /// <summary>Формирование строки подключения.</summary>
        private static string GetConnectionString(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            const string SEPARATOR = ";";

            var stringBuilder = new StringBuilder();

            foreach (var pair in keyValuePairs.Where(pair => pair.Value != null))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(SEPARATOR);

                stringBuilder.AppendFormat("{0}={1}", pair.Key, pair.Value);
            }

            return stringBuilder.ToString();
        }

        /// <summary>Формирование строки подключения для известных полей.</summary>
        private static string GetConnectionStringFromKnownFields(string file = null, string user = null, string password = null)
        {
            var dictionary = new Dictionary<string, string>
                {
                    { FILE_KEY, file },
                    { USER_KEY, user},
                    { PASSWORD_KEY, password}
                };

            return GetConnectionString(dictionary);
        }

        /// <summary>Проверка строки подключения.</summary>
        private void AssertConnectionString(string expectedConnectionString)
        {
            Assert.AreEqual(
                expectedConnectionString,
                _testedInstance.ConnectionString
                );
        }

        /// <summary>Проверка строки подключения состоящей из одного поля.</summary>
        /// <param name="fieldKey">Ключ поля.</param>
        /// <param name="fieldValue">Ключ значения.</param>
        private void AssertConnectionStringOneField(string fieldKey, string fieldValue)
        {
            var dictionary = new Dictionary<string, string>
                {
                    { fieldKey, fieldValue }
                };
            
            AssertConnectionString(GetConnectionString(dictionary));
        }

        /// <summary>Проверка строки подключения состоящей из известных полей.</summary>
        private void AssertConnectionStringKnownFields(string file = null, string user = null, string password = null)
        {
            AssertConnectionString(GetConnectionStringFromKnownFields(file, user, password));
        }

        #endregion

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSConnectionStringBuilder _testedInstance;

        /// <summary>Инициализация тестов.</summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new OneSConnectionStringBuilder();
        }

        /// <summary>Тестирование свойства <see cref="DbConnectionStringBuilder.IsFixedSize"/>.</summary>
        [Test]
        public void TestIsFixedSize()
        {
            Assert.IsFalse(_testedInstance.IsFixedSize);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Item"/>
        /// после инициализации для известных полей.
        /// </summary>
        /// <param name="fieldKey">Ключ поля.</param>
        [Test]
        public void TestKnownFieldAfterInit([Values(FILE_KEY, SERVER_KEY, REFERENCE_KEY, USER_KEY, PASSWORD_KEY)] string fieldKey)
        {
            Assert.AreEqual(string.Empty, _testedInstance[fieldKey]);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Item"/>
        /// после инициализации для неизвестных полей.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUnknownFieldAfterInit()
        {
            var result = _testedInstance["Timeout"];
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Keys"/>, после инициализации.
        /// </summary>
        [Test]
        public void TestKeysAfterInit()
        {
            CollectionAssert.AreEquivalent(
                expected: _knownKeys,
                actual: _testedInstance.Keys);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Values"/>, после инициализации.
        /// </summary>
        [Test]
        public void TestValuesAfterInit()
        {
            CollectionAssert.AreEqual(
                expected: Enumerable.Repeat<object>(string.Empty, _knownKeys.Count),
                actual: _testedInstance.Values);
        }

        /// <summary>
        /// Тестирование строки подключения <see cref="DbConnectionStringBuilder.ConnectionString"/>
        /// после инициализации.
        /// </summary>
        [Test]
        public void TestConnectionStringAfterInit()
        {
            AssertConnectionString(string.Empty);
        }

        /// <summary>
        /// Тестирование экземпяра <see cref="OneSConnectionStringBuilder"/> после
        /// добавление поля.
        /// </summary>
        /// <typeparam name="TValue">Тип значения добавляемого поля.</typeparam>
        /// <param name="fieldKey">Ключ поля.</param>
        /// <param name="fieldValue">Значение поля.</param>
        /// <param name="expectedKeys">Ожидаемые ключи <see cref="OneSConnectionStringBuilder.Keys"/>.</param>
        /// <param name="expectedValues"></param>
        private void TestAfterAppendField<TValue>(string fieldKey, TValue fieldValue, IEnumerable expectedKeys, IEnumerable expectedValues)
        where TValue : IConvertible
        {
            // Act
            _testedInstance[fieldKey] = fieldValue;
            var strFieldValue = fieldValue.ToString(CultureInfo.InvariantCulture);

            // Assert

            // Проверка того, что поле добавилось
            Assert.AreEqual(strFieldValue, _testedInstance[fieldKey]);

            // Проверка ключей
            CollectionAssert.AreEquivalent(expectedKeys, _testedInstance.Keys);

            // Проверка значений
            CollectionAssert.AreEquivalent(expectedValues.Cast<IConvertible>().Select(c => c == null ? null : c.ToString(CultureInfo.InvariantCulture)), _testedInstance.Values);

            // Проверка получаемой строки подключений
            AssertConnectionStringOneField(fieldKey, strFieldValue);
        }

        /// <summary>Тестирование после добавления известного поля.</summary>
        [Test]
        public void TestAfterAppendKnownField()
        {
            const string FILE_NAME = @"C:\1C";

            TestAfterAppendField(
                fieldKey: FILE_KEY, 
                fieldValue: FILE_NAME,
 
                // Ключи не должны измениться
                expectedKeys: _knownKeys,

                // Одно значение не должно быть пустым
                expectedValues: Enumerable.Repeat<object>(string.Empty, _knownKeys.Count - 1).Concat(Enumerable.Repeat<object>(FILE_NAME, 1)));
        }

        /// <summary>Тестирование после добавление неизвестного поля.</summary>
        /// <remarks>
        /// Сделана возможность добавления любых полей на случай,
        /// если появилось новое поле в строке подключения к 1С, но провайдер еще не успел его поддержать.
        /// </remarks>
        [Test]
        public void TestAfterAppendUnknownField()
        {
            const int TIMEOUT = 1000;
            const string TIMEOUT_KEY = "Timeout";

            TestAfterAppendField(
                fieldKey: TIMEOUT_KEY,
                fieldValue: TIMEOUT,

                // Ключи не должны измениться
                expectedKeys: _knownKeys.Concat(Enumerable.Repeat(TIMEOUT_KEY, 1)),

                // Одно значение не должно быть пустым
                expectedValues: Enumerable.Repeat<object>(string.Empty, _knownKeys.Count).Concat(Enumerable.Repeat<object>(TIMEOUT, 1))
                );
        }

        /// <summary>
        /// Тестирование поля строки подключений, являющиейся специальным свойством <see cref="OneSConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="fieldKey">Ключ поля.</param>
        /// <param name="fieldValue">Значение поля.</param>
        /// <param name="propertyGetter">Получатель значения свойства.</param>
        /// <param name="propertySetter">Установщик значения свойства.</param>
        private void TestTypedField(string fieldKey, string fieldValue,
            Func<OneSConnectionStringBuilder, string> propertyGetter, Action<OneSConnectionStringBuilder, string> propertySetter)
        {
            // Act
            propertySetter(_testedInstance, fieldValue);

            // Assert
            Assert.AreEqual(fieldValue, propertyGetter(_testedInstance));
            Assert.AreEqual(fieldValue, _testedInstance[fieldKey]);
            AssertConnectionStringOneField(fieldKey, fieldValue);
        }

        /// <summary>Тестирование <see cref="OneSConnectionStringBuilder.Catalog"/>.</summary>
        [Test]
        public void TestCatalog()
        {
            TestTypedField(FILE_KEY, @"C:\1С", b => b.Catalog, (b, v) => b.Catalog = v);
        }

        /// <summary>Тестирование <see cref="OneSConnectionStringBuilder.User"/>.</summary>
        [Test]
        public void TestUser()
        {
            TestTypedField(USER_KEY, "Вася", b => b.User, (b, v) => b.User = v);
        }

        /// <summary>Тестирование <see cref="OneSConnectionStringBuilder.Password"/>.</summary>
        [Test]
        public void TestPassword()
        {
            TestTypedField(PASSWORD_KEY, "12345", b => b.Password, (b, v) => b.Password = v);
        }

        /// <summary>
        /// Тестирование генерации значения свойства <see cref="DbConnectionStringBuilder.ConnectionString"/>.
        /// </summary>
        [Test]
        public void TestBuildConnectionString()
        {
            const string FILE_NAME = @"C:\1CData";
            const string USER_NAME = "Иванов";
            const string PASSWORD = "12345";

            _testedInstance.Catalog = FILE_NAME;
            _testedInstance.User = USER_NAME;

            AssertConnectionStringKnownFields(file: FILE_NAME, user: USER_NAME);

            _testedInstance.Password = PASSWORD;
            AssertConnectionStringKnownFields(file: FILE_NAME, user: USER_NAME, password: PASSWORD);
        }

        /// <summary>Тестирование поведения построителя при задании некорректной строки соединения.</summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidConnectionString()
        {
            _testedInstance.ConnectionString = "белеберда";
        }

        /// <summary>Тестирование парсинга строки соединения.</summary>
        [Test]
        public void TestParse()
        {
            const string FILE_NAME = @"C:\1CData";
            const string USER_NAME = "Иванов";
            const string PASSWORD = "12345";

            _testedInstance.ConnectionString = GetConnectionStringFromKnownFields(file: FILE_NAME, user: USER_NAME, password: PASSWORD);

            Assert.AreEqual(FILE_NAME, _testedInstance[FILE_KEY]);
            Assert.AreEqual(FILE_NAME, _testedInstance.Catalog);

            Assert.AreEqual(USER_NAME, _testedInstance[USER_KEY]);
            Assert.AreEqual(USER_NAME, _testedInstance.User);

            Assert.AreEqual(PASSWORD, _testedInstance[PASSWORD_KEY]);
            Assert.AreEqual(PASSWORD, _testedInstance.Password);
        }

        /// <summary>Тестирование совместного использования <see cref="DbConnectionStringBuilder.ConnectionString"/> и других свойств.</summary>
        [Test]
        public void TestUseConnectionString()
        {
            const string FILE_NAME = @"C:\1CData";
            const string USER_1_NAME = "Иванов";

            _testedInstance.ConnectionString = GetConnectionStringFromKnownFields(file: FILE_NAME, user: USER_1_NAME);

            const string PASSWORD = "12345";
            _testedInstance.Password = PASSWORD;
            AssertConnectionStringKnownFields(file: FILE_NAME, user: USER_1_NAME, password: PASSWORD);

            const string USER_2_NAME = "Петров";
            _testedInstance.User = USER_2_NAME;
            AssertConnectionStringKnownFields(file: FILE_NAME, user: USER_2_NAME, password: PASSWORD);
        }
    }
}