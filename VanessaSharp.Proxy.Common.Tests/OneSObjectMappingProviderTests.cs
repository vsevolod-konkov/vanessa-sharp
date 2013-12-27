using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Тестирование <see cref="OneSObjectMappingProvider"/>.</summary>
    [TestFixture]
    public sealed class OneSObjectMappingProviderTests
    {
        /// <summary>Тестовое имя типа в 1С.</summary>
        private const string TEST_TYPE_NAME = "TestObject";

        /// <summary>Класс для тестирования атрибута <see cref="OneSObjectMapping"/>.</summary>
        public sealed class TestObject
        { }

        /// <summary>
        /// Тестовый интерфейс с правильно заданным атрибутом <see cref="OneSObjectMapping"/>.
        /// </summary>
        [OneSObjectMapping(WrapType = typeof(TestObject), OneSTypeName = TEST_TYPE_NAME)]
        public interface IValidTestInterface
        {}

        /// <summary>
        /// Тестовый интерфейс с правильно заданным атрибутом <see cref="OneSObjectMapping"/>,
        /// но с неопределенным именем типа в 1С.
        /// </summary>
        [OneSObjectMapping(WrapType = typeof(TestObject))]
        public interface IValidButNotDefinedTypeNameTestInterface
        {}

        /// <summary>
        /// Тестовый интерфейс с неправильно заданным атрибутом <see cref="OneSObjectMapping"/>,
        /// так как не задан тип обертки.
        /// </summary>
        [OneSObjectMapping(OneSTypeName = TEST_TYPE_NAME)]
        public interface IInvalidTestInterface
        {}

        /// <summary>
        /// Некоторый интерфейс у которого не задан атрибут
        /// <see cref="OneSObjectMapping"/>.
        /// </summary>
        public interface ISomeInterfaceIsNotSetAttribute 
        {}

        /// <summary>
        /// Тестирование <see cref="OneSObjectMappingProvider.GetObjectMapping"/>
        /// в случае когда атрибут задан корректно.
        /// </summary>
        [Test]
        public void TestGetObjectMappingWhenAttributeIsValid()
        {
            var result = OneSObjectMappingProvider.GetObjectMapping(typeof(IValidTestInterface));
            
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(IValidTestInterface), result.InterfaceType);
            Assert.AreEqual(typeof(TestObject), result.WrapType);
            Assert.AreEqual(TEST_TYPE_NAME, result.OneSTypeName);
        }

        /// <summary>
        /// Тестирование <see cref="OneSObjectMappingProvider.GetObjectMapping"/>
        /// в случае когда атрибут задан корректно,
        /// но не задано имя типа.
        /// </summary>
        [Test]
        public void TestGetObjectMappingWhenAttributeIsValidButNotDefinedTypeName()
        {
            var result = OneSObjectMappingProvider.GetObjectMapping(typeof(IValidButNotDefinedTypeNameTestInterface));

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(IValidButNotDefinedTypeNameTestInterface), result.InterfaceType);
            Assert.AreEqual(typeof(TestObject), result.WrapType);
            Assert.IsNull(result.OneSTypeName);
        }

        /// <summary>
        /// Тестирование <see cref="OneSObjectMappingProvider.GetObjectMapping"/>
        /// в случае когда атрибут задан некорректно.
        /// </summary>
        [Test]
        public void TestGetObjectMappingWhenAttributeIsInvalid()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                OneSObjectMappingProvider.GetObjectMapping(typeof(IInvalidTestInterface)));

            Assert.AreEqual(
                string.Format(
                    "Неверно задан атрибут \"{0}\" у типа \"{1}\". Свойство WrapType не может быть равным null.",
                    typeof(OneSObjectMappingAttribute), typeof(IInvalidTestInterface)),
                exception.Message);
        }

        /// <summary>
        /// Тестирование <see cref="OneSObjectMappingProvider.GetObjectMapping"/>
        /// в случае когда атрибут не задан.
        /// </summary>
        [Test]
        public void TestGetObjectMappingWhenAttributeIsNotDefined()
        {
            var result = OneSObjectMappingProvider.GetObjectMapping(typeof(ISomeInterfaceIsNotSetAttribute));

            Assert.IsNull(result);
        }

        /// <summary>Проверка соответсвий.</summary>
        /// <typeparam name="TValue">Тип значения в соответствии.</typeparam>
        /// <param name="expectedDictionary">Карта ожидаемых соответствий.</param>
        /// <param name="testedMappings">Проверяемые соответствия.</param>
        /// <param name="valueSelector">Выборщик значения из соответствия.</param>
        private static void AssertMappings<TValue>(Dictionary<Type, TValue> expectedDictionary,
                                            IEnumerable<OneSObjectMapping> testedMappings,
                                            Func<OneSObjectMapping, TValue> valueSelector)
        {
            var testedMappingsArray = testedMappings.ToArray();
            
            CollectionAssert.AreEquivalent(
                expectedDictionary.Keys,
                testedMappingsArray.Select(m => m.InterfaceType).ToArray()
                );

            foreach (var mapping in testedMappingsArray)
            {
                Assert.AreEqual(expectedDictionary[mapping.InterfaceType], valueSelector(mapping));
            }
        }

        /// <summary>
        /// Тестирование <see cref="OneSObjectMappingProvider.GetObjectMappings()"/>.
        /// </summary>
        [Test]
        public void TestGetObjectMappings()
        {
            // Act
            var result = OneSObjectMappingProvider.GetObjectMappings().ToArray();
            
            // Assert
            var expectedMapTypes = new Dictionary<Type, Type>
                {
                    {typeof (IQuery), typeof (OneSQuery)},
                    {typeof (IQueryResult), typeof (OneSQueryResult)},
                    {typeof (IQueryResultColumnsCollection), typeof (OneSQueryResultColumnsCollection)},
                    {typeof (IQueryResultColumn), typeof (OneSQueryResultColumn)},
                    {typeof(IQueryResultSelection), typeof(OneSQueryResultSelection)},
                    {typeof (IValueType), typeof (OneSValueType)}
                };
            AssertMappings(expectedMapTypes, result, m => m.WrapType);


            var expectedMapNames = new Dictionary<Type, string>
                {
                    { typeof(IQuery), "Query"}
                };
            AssertMappings(expectedMapNames, result.Where(m => m.OneSTypeName != null), m => m.OneSTypeName);
        }
    }
}
