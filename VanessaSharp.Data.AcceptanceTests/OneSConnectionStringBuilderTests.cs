using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>Тестирование <see cref="OneSConnectionStringBuilder"/>.</summary>
    [TestFixture]
    public sealed class OneSConnectionStringBuilderTests
    {
        private const string FILE_KEY = "File";
        private const string USER_KEY = "Usr";
        private const string PASSWORD_KEY = "Pwd";

        /// <summary>Ожидаемые ключи в строке подключения.</summary>
        private static readonly ReadOnlyCollection<string> _knownKeys
            = new ReadOnlyCollection<string>(new[] { FILE_KEY, USER_KEY, PASSWORD_KEY });

        private OneSConnectionStringBuilder _testingInstance;

        [SetUp]
        public void SetUp()
        {
            _testingInstance = new OneSConnectionStringBuilder();
        }

        /// <summary>Тестирование свойства <see cref="DbConnectionStringBuilder.IsFixedSize"/>.</summary>
        [Test]
        public void TestIsFixedSize()
        {
            Assert.IsFalse(_testingInstance.IsFixedSize);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Item"/>
        /// после инициализации для известных полей.
        /// </summary>
        /// <param name="fieldKey">Ключ поля.</param>
        [Test]
        public void TestKnownFieldAfterInit([Values(FILE_KEY, USER_KEY, PASSWORD_KEY)] string fieldKey)
        {
            Assert.AreEqual(string.Empty, _testingInstance[fieldKey]);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Item"/>
        /// после инициализации для неизвестных полей.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUnknownFieldAfterInit()
        {
            var result = _testingInstance["Timeout"];
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Keys"/>, после инициализации.
        /// </summary>
        [Test]
        public void TestKeysAfterInit()
        {
            CollectionAssert.AreEquivalent(
                expected: _knownKeys,
                actual: _testingInstance.Keys);
        }

        /// <summary>
        /// Тестирование <see cref="OneSConnectionStringBuilder.Values"/>, после инициализации.
        /// </summary>
        [Test]
        public void TestValuesAfterInit()
        {
            CollectionAssert.AreEqual(
                expected: Enumerable.Repeat<object>(string.Empty, _knownKeys.Count),
                actual: _testingInstance.Values);
        }

        /// <summary>
        /// Тестирование строки подключения <see cref="DbConnectionStringBuilder.ConnectionString"/>
        /// после инициализации.
        /// </summary>
        [Test]
        public void TestConnectionStringAfterInit()
        {
            Assert.IsEmpty(_testingInstance.ToString());
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
            _testingInstance[fieldKey] = fieldValue;
            var strFieldValue = fieldValue.ToString(CultureInfo.InvariantCulture);

            // Assert

            // Проверка того, что поле добавилось
            Assert.AreEqual(strFieldValue, _testingInstance[fieldKey]);

            // Проверка ключей
            CollectionAssert.AreEquivalent(expectedKeys, _testingInstance.Keys);

            // Проверка значений
            CollectionAssert.AreEquivalent(expectedValues.Cast<IConvertible>().Select(c => c == null ? null : c.ToString(CultureInfo.InvariantCulture)) , _testingInstance.Values);

            // Проверка получаемой строки подключений
            TestConnectionString(fieldKey, strFieldValue);
        }

        /// <summary>Тестирование строки подключений.</summary>
        /// <param name="fieldKey">Ключ поля.</param>
        /// <param name="fieldValue">Ключ значения.</param>
        private void TestConnectionString(string fieldKey, string fieldValue)
        {
            Assert.AreEqual(string.Format("{0}={1}", fieldKey, fieldValue), _testingInstance.ConnectionString);
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
            propertySetter(_testingInstance, fieldValue);

            // Assert
            Assert.AreEqual(fieldValue, propertyGetter(_testingInstance));
            Assert.AreEqual(fieldValue, _testingInstance[fieldKey]);
            TestConnectionString(fieldKey, fieldValue);
        }
    }
}