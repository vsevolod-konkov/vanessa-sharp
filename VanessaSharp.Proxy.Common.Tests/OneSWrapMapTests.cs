using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Тестирование <see cref="OneSWrapMap"/>.</summary>
    [TestFixture]
    public sealed class OneSWrapMapTests
    {
        #region Вспомогательные типы для тестирования

        private interface IValidObjectForTest
        {
             object ComObject { get; }
        }

        /// <summary>Тестовый интерфейс.</summary>
        private interface ITestObject : IDisposable
        { }

        /// <summary>Валидный тестовый класс.</summary>
        private sealed class ValidTestObject : OneSContextBoundObject, ITestObject, IValidObjectForTest
        {
            public ValidTestObject(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
                : base(comObject, proxyWrapper, globalContext)
            {
                _comObject = comObject;
            }

            public object ComObject
            {
                get { return _comObject; }
            }
            private readonly object _comObject;
        }

        /// <summary>Другой тестовый интерфейс.</summary>
        private interface IOtherTestObject : IDisposable
        { }

        /// <summary>
        /// Тестовый класс поддерживающий интерфейс, но ненаследующий от <see cref="OneSContextBoundObject"/>.
        /// </summary>
        private sealed class TestObjectIsNotDerivedContextBoundObject : ITestObject
        {
            public void Dispose()
            {}
        }

        /// <summary>
        /// Некоторый тестовый обобщенный интерфейс.
        /// </summary>
        private interface ITestGenericObject<T1, T2> : IDisposable
        {}

        /// <summary>
        /// Валидный тестовый обобщенный класс.
        /// </summary>
        private sealed class ValidTestGenericObject<T1, T2>
            : OneSContextBoundObject, ITestGenericObject<T2, T1>, IValidObjectForTest
        {
            public ValidTestGenericObject(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
                : base(comObject, proxyWrapper, globalContext)
            {
                _comObject = comObject;
            }

            public object ComObject
            {
                get { return _comObject; }
            }
            private readonly object _comObject;
        }

        /// <summary>
        /// Тестовый класс наследующий от <see cref="OneSContextBoundObject"/>
        /// но не поддерживающий нужный интерфейс <see cref="ITestObject"/>.
        /// </summary>
        private sealed class TestObjectIsNotSupportInterface : OneSContextBoundObject
        {
            public TestObjectIsNotSupportInterface(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext) 
                : base(comObject, proxyWrapper, globalContext)
            {}
        }

        /// <summary>Тестовый класс, который не имеет подходящего конструктора.</summary>
        private sealed class TestObjectHasNotMatchedConstructor : OneSContextBoundObject, ITestObject
        {
            public TestObjectHasNotMatchedConstructor(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext, decimal number)
                : base(comObject, proxyWrapper, globalContext)
            {}
        }

        #endregion

        /// <summary>Тестовое имя типа в 1С.</summary>
        private const string TEST_TYPE_NAME = "TestObject";

        /// <summary>
        /// Создание карты оберток для тестирования с 
        /// одним соответствием.
        /// </summary>
        private static OneSWrapMap CreateNotEmptyWrapMap(string typeName = TEST_TYPE_NAME)
        {
            var testedInstance = new OneSWrapMap();
            testedInstance.AddObjectMapping(new OneSObjectMapping(
                typeof(ITestObject),
                typeof(ValidTestObject),
                typeName
                ));

            return testedInstance;
        }

        /// <summary>Проверка результата тестирования.</summary>
        /// <param name="testedCreator">Проверяемый делегат создания.</param>
        private static void AssertCreator<T>(
            Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> testedCreator)
        where T : OneSContextBoundObject, IValidObjectForTest
        {
            Assert.IsNotNull(testedCreator);

            var comObject = new object();
            var proxyWrapper = new Mock<IOneSProxyWrapper>(MockBehavior.Strict).Object;
            var globalContext = new OneSGlobalContext(new object());

            var result = testedCreator(comObject, proxyWrapper, globalContext);
            
            Assert.IsInstanceOf<T>(result);
            var testObject = (T)result;
            Assert.AreSame(comObject, testObject.ComObject);
            Assert.AreSame(proxyWrapper, testObject.ProxyWrapper);
            Assert.AreSame(globalContext, testObject.GlobalContext);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.CheckAndBuildCreator"/>
        /// в случае когда тип обертки не наследуется от <see cref="OneSContextBoundObject"/>.
        /// </summary>
        [Test]
        public void TestCheckAndBuildCreatorWhenWrapTypeIsNotDerivedContextBoundObject()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                                    OneSWrapMap
                                        .CheckAndBuildCreator(
                                            typeof(ITestObject),
                                            typeof(TestObjectIsNotDerivedContextBoundObject)));

            // Assert
            Assert.AreEqual(
                string.Format(
                "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                + " так как указанный тип обертки \"{1}\" не наследуется от \"{2}\"",
                typeof(ITestObject),
                typeof(TestObjectIsNotDerivedContextBoundObject),
                typeof(OneSContextBoundObject)
                ),
                exception.Message);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.CheckAndBuildCreator"/>
        /// в случае когда тип обертки не поддерживает требуемый интерфейс.
        /// </summary>
        [Test]
        public void TestCheckAndBuildCreatorWhenWrapTypeIsNotSupportedRequiredInterface()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                                    OneSWrapMap
                                        .CheckAndBuildCreator(
                                            typeof(ITestObject),
                                            typeof(TestObjectIsNotSupportInterface)));

            // Assert
            Assert.AreEqual(
                string.Format(
                "Невозможно создать экземпляр типизированной обертки над объектом 1С поддерживающим интерфейс \"{0}\""
                + " так как указанный тип обертки \"{1}\" этот интерфейс не поддерживает.",
                typeof(ITestObject),
                typeof(TestObjectIsNotSupportInterface)),
                exception.Message);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.CheckAndBuildCreator"/>
        /// в случае когда тип обертки не поддерживает имеет подходящего конструктора.
        /// </summary>
        [Test]
        public void TestCheckAndBuildCreatorWhenWrapTypeHasNotMatchedConstructor()
        {
            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                                    OneSWrapMap
                                        .CheckAndBuildCreator(
                                            typeof(ITestObject),
                                            typeof(TestObjectHasNotMatchedConstructor)));

            // Assert
            Assert.AreEqual(
                string.Format(
                "Невозможно создать экземпляр типизированной обертки над объектом 1С"
                + " так как указанный тип обертки \"{0}\" не имеет подходящего конструктора."
                + " Тип должен иметь публичный конструктор у которого кроме аргументов с типами: \"{1}\", \"{2}\", \"{3}\" других не имеется.",
                typeof(TestObjectHasNotMatchedConstructor),
                typeof(object),
                typeof(IOneSProxyWrapper),
                typeof(OneSGlobalContext)),
                exception.Message);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.CheckAndBuildCreator"/>
        /// в случае когда аргументы корректны.
        /// </summary>
        [Test]
        public void TestCheckAndBuildCreatorWhenIsAllCorrect()
        {
            // Act
            var creator = OneSWrapMap.CheckAndBuildCreator(typeof(ITestObject), typeof(ValidTestObject));

            // Assert
            AssertCreator<ValidTestObject>(creator);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.GetObjectCreator"/>
        /// после инициализации.
        /// </summary>
        [Test]
        public void TestGetObjectCreatorAfterInit()
        {
            // Arrange
            var testedInstance = new OneSWrapMap();

            // Act и Assert
            Assert.IsNull(testedInstance.GetObjectCreator(typeof(ITestObject)));
            Assert.IsNull(testedInstance.GetOneSObjectTypeName(typeof(ITestObject)));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.GetObjectCreator"/>.
        /// </summary>
        [Test]
        public void TestGetObjectCreatorWhenTypeIsNotSupported()
        {
            // Arrange
            var testedInstance = CreateNotEmptyWrapMap();

            // Act и Assert
            Assert.IsNull(testedInstance.GetObjectCreator(typeof(IOtherTestObject)));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.GetObjectCreator"/>
        /// в случае корректного типа.
        /// </summary>
        [Test]
        public void TestGetObjectCreatorWhenTypeIsCorrect()
        {
            // Arrange
            var testedInstance = CreateNotEmptyWrapMap();

            // Act
            var creator = testedInstance.GetObjectCreator(typeof(ITestObject));

            // Assert
            AssertCreator<ValidTestObject>(creator);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapMap.GetObjectCreator"/>
        /// если корректный обобщенный тип.
        /// </summary>
        [Test]
        public void TestGetObjectCreatorWhenGenericTypeIsCorrect()
        {
            // Arrange
            var testedInstance = CreateNotEmptyWrapMap();
            testedInstance.AddObjectMapping(new OneSObjectMapping(
                typeof(ITestGenericObject<,>), typeof(ValidTestGenericObject<,>), null));

            // Act
            var creator = testedInstance.GetObjectCreator(typeof(ITestGenericObject<int, string>));

            // Assert
            AssertCreator<ValidTestGenericObject<string, int>>(creator);
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapMap.GetOneSObjectTypeName"/>
        /// </summary>
        /// <param name="expectedResult">Ожидаемый результат метода.</param>
        [Test]
        public void TestGetOneSObjectTypeName([Values(TEST_TYPE_NAME, null)] string expectedResult)
        {
            // Arrange
            var testedInstance = CreateNotEmptyWrapMap(expectedResult);

            // Act
            var actualResult = testedInstance.GetOneSObjectTypeName(typeof(ITestObject));

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapMap.GetOneSObjectTypeName"/>
        /// когда тип не поддерживается.
        /// </summary>
        [Test]
        public void TestGetOneSObjectTypeNameWhenTypeIsNotSupported()
        {
            // Arrange
            var testedInstance = CreateNotEmptyWrapMap();

            // Act
            var actualResult = testedInstance.GetOneSObjectTypeName(typeof(IOtherTestObject));

            // Assert
            Assert.IsNull(actualResult);
        }
    }
}
