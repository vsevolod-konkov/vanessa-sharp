using System;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Базовый класс для тестирования <see cref="OneSWrapFactory"/>.</summary>
    public abstract class OneSWrapFactoryTestsBase
    {
        #region Вспомогательные типы для тестов

        /// <summary>Тестовый интерфейс.</summary>
        protected interface ITestObject : IDisposable
        { }

        /// <summary>Тестовый класс.</summary>
        protected sealed class TestObject : OneSContextBoundObject, ITestObject
        {
            public TestObject(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
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
        protected interface IAnotherTestObject : IDisposable
        { }

        #endregion

        /// <summary>Имя тестового объекта в 1С.</summary>
        protected const string TEST_ONES_TYPE_NAME = "TestObject";

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSWrapFactory TestedInstance { get; private set; }

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        protected abstract OneSWrapFactory InitTestedInstance();

        /// <summary>Получение параметров для создания обертки.</summary>
        protected static CreateWrapParameters GetCreateWrapParameters(Type requiredInterface)
        {
            var proxyWrapper = new Mock<IOneSProxyWrapper>(MockBehavior.Strict).Object;
            var globalContext = new OneSGlobalContext(new object());

            return new CreateWrapParameters(requiredInterface, proxyWrapper, globalContext);
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            TestedInstance = InitTestedInstance();
        }


        /// <summary>
        /// Тестирование <see cref="OneSWrapFactory.CreateWrap"/>
        /// в случае когда тип поддерживается тестируемой фабрикой.
        /// </summary>
        protected OneSObject TestCreateWrapWhenTypeSupported(out object obj, out CreateWrapParameters createWrapParameters)
        {
            // Arrange
            obj = new object();
            createWrapParameters = GetCreateWrapParameters(typeof(ITestObject));

            // Act
            var result = TestedInstance.CreateWrap(obj, createWrapParameters);

            // Assert
            Assert.IsInstanceOf<TestObject>(result);
            var testObject = (TestObject)result;
            Assert.AreSame(obj, testObject.ComObject);
            Assert.AreSame(createWrapParameters.ProxyWrapper, testObject.ProxyWrapper);
            Assert.AreSame(createWrapParameters.GlobalContext, testObject.GlobalContext);

            return result;
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapFactory.CreateWrap"/>
        /// в случае когда тип поддерживается тестируемой фабрикой.
        /// </summary>
        [Test]
        public virtual void TestCreateWrapWhenTypeSupported()
        {
            object obj;
            CreateWrapParameters createWrapParameters;
            TestCreateWrapWhenTypeSupported(out obj, out createWrapParameters);
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapFactory.CreateWrap"/>
        /// в случае когда тип не поддерживается тестируемой фабрикой.
        /// </summary>
        [Test]
        public virtual void TestCreateWrapWhenTypeNotSupported()
        {
            // Arrange
            var obj = new object();
            var createWrapParameters = GetCreateWrapParameters(typeof(IAnotherTestObject));

            // Act
            Assert.Throws<NotSupportedException>(() =>
                TestedInstance.CreateWrap(obj, createWrapParameters));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapFactory.GetTypeNameFor"/>
        /// в случае поддержки типа.
        /// </summary>
        [Test]
        public virtual void TestGetTypeNameForWhenTypeSupported()
        {
            // Act
            var result = TestedInstance.GetTypeNameFor(typeof(ITestObject));

            // Assert
            Assert.AreEqual(TEST_ONES_TYPE_NAME, result);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapFactory.GetTypeNameFor"/>
        /// в случае если тип не поддерживается.
        /// </summary>
        [Test]
        public virtual void TestGetTypeNameForWhenTypeNotSupported()
        {
            // Act
            Assert.Throws<InvalidOperationException>(() =>
                TestedInstance.GetTypeNameFor(typeof(IAnotherTestObject)));
        }
    }
}
