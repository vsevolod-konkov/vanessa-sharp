using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using NUnit.Framework;
using Moq;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тесты на <see cref="OneSObject"/>.
    /// </summary>
    [TestFixture(false)]
    [TestFixture(true)]
    public sealed class OneSObjectTests
    {
        /// <summary>
        /// Следует ли обертывать аргумент.
        /// </summary>
        private readonly bool _shouldBeWrap;
        
        /// <summary>Мок нижележащего объекта.</summary>
        private Mock<ISomeContract> _mockComObject;

        /// <summary>Тестируемый объект.</summary>
        private dynamic _testingObject;

        /// <summary>Ожидаемый аргумент нижележащим объектом.</summary>
        private object _argument;

        /// <summary>Обернутый аргумент.</summary>
        private object _wrappedArgument;

        /// <summary>Конструктор.</summary>
        /// <param name="shouldBeWrap">Следует ли обертывать аргумент.</param>
        public OneSObjectTests(bool shouldBeWrap)
        {
            _shouldBeWrap = shouldBeWrap;
        }


        /// <summary>
        /// Общая инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mockComObject = new Mock<ISomeContract>();
            InitArgument();
        }

        /// <summary>
        /// Инициализация тестируемого объекта.
        /// </summary>
        private void InitTestingObject()
        {
            Assert.IsNotNull(_mockComObject);

            var mockWrapper = new Mock<IOneSProxyWrapper>();
            mockWrapper.Setup(w => w.Wrap(It.IsAny<object>()))
                       .Returns<object>(o => new TestProxy(o));

            _testingObject = new OneSObject(_mockComObject.Object, mockWrapper.Object);
        }

        /// <summary>
        /// Инициализация аргумента.
        /// </summary>
        private void InitArgument()
        {
            _argument = new object();
            _wrappedArgument = WrapArgument(_shouldBeWrap, _argument);
        }
        
        /// <summary>
        /// Обертывает аргумент.
        /// </summary>
        private static object WrapArgument(bool shouldBeWrap, object arg)
        {
            return shouldBeWrap
                ? new TestProxy(arg)
                : arg;
        }

        /// <summary>
        /// Тестирование вызова метода экземпляра объекта <see cref="OneSObject"/>,
        /// возвращающий значение с проверкой разупаковки аргумента, если это необходимо.
        /// </summary>
        /// <remarks>
        /// Тест проверяет, что при вызове метода у аргументов метода вызывается
        /// метод <see cref="IOneSProxy.Unwrap"/>, если они поддерживают интерфейс
        /// <see cref="IOneSProxy"/>.
        /// Кроме этого проверяется вызов метода <see cref="IOnesWrapper.Wrap"/>
        /// у экземпляра обертывателя.
        /// </remarks>
        [Test(Description = "Тестирование вызова метода возвращающего значение с проверкой разупаковки аргумента.")]
        public void TestCallFunction()
        {
            // Подготовка окружения
            var expectedResult = new object();
            _mockComObject.Setup(c => c.SomeFunction(It.IsAny<object>()))
                          .Returns(expectedResult);

            InitTestingObject();

            // Выполнение
            object actualResult = _testingObject.SomeFunction(_wrappedArgument);

            // Проверка

            // Проверка, того что был вызван метод  исходными параметрами
            _mockComObject.Verify(c => c.SomeFunction(It.IsAny<object>()), Times.Once());
            _mockComObject.Verify(c => c.SomeFunction(_argument), Times.Once());

            // Проверка полученного результата
            Assert.IsInstanceOf<TestProxy>(actualResult);
            Assert.AreSame(expectedResult, ((IOneSProxy)actualResult).Unwrap());
        }

        /// <summary>
        /// Тестирование вызова метода экземпляра объекта <see cref="OneSObject"/>,
        /// не возвращающий значение с проверкой разупаковки аргумента, если это необходимо.
        /// </summary>
        /// <remarks>
        /// Тест проверяет, что при вызове метода у аргументов метода вызывается
        /// метод <see cref="IOneSProxy.Unwrap"/>, если они поддерживают интерфейс
        /// <see cref="IOneSProxy"/>.
        /// </remarks>
        [Test(Description = "Тестирование вызова метода не возвращающего значение с проверкой разупаковки аргумента.")]
        public void TestCallAction()
        {
            // Подготовка окружения
            InitTestingObject();

            // Выполнение
            _testingObject.SomeAction(_wrappedArgument);

            // Проверка

            // Проверка, того что был вызван метод
            _mockComObject.Verify(c => c.SomeAction(It.IsAny<object>()), Times.Once());
            _mockComObject.Verify(c => c.SomeAction(_argument), Times.Once());
        }

        /// <summary>
        /// Тестирование получения значения свойства у экземпляра объекта <see cref="OneSObject"/>.
        /// </summary>
        /// <remarks>
        /// Кроме этого проверяется вызов метода <see cref="IOnesWrapper.Wrap"/>
        /// у экземпляра обертывателя.
        /// </remarks>
        [Test(Description = "Тестирование получения значения свойства.")]
        public void TestGetProperty()
        {
            // Подготовка окружения
            var expectedResult = new object();
            _mockComObject.SetupGet(c => c.SomeProperty).Returns(expectedResult);
            
            InitTestingObject();

            // Выполнение
            var actualResult = _testingObject.SomeProperty;

            // Проверка

            // Проверка, того что был вызван метод
            _mockComObject.VerifyGet(c => c.SomeProperty, Times.Once());

            // Проверка полученного результата
            Assert.IsInstanceOf<TestProxy>(actualResult);
            Assert.AreSame(expectedResult, ((IOneSProxy)actualResult).Unwrap());
        }

        /// <summary>
        /// Тестирование установки значения свойства у экземпляра объекта <see cref="OneSObject"/>.
        /// </summary>
        /// <remarks>
        /// Тест проверяет, что при установке значения у нового значения вызывается
        /// метод <see cref="IOneSProxy.Unwrap"/>, если они поддерживает интерфейс
        /// <see cref="IOneSProxy"/>.
        /// </remarks>
        [Test(Description = "Тестирование получения значения свойства.")]
        public void TestSetProperty()
        {
            // Подготовка окружения
            InitTestingObject();

            // Выполнение
            _testingObject.SomeProperty = _wrappedArgument;

            // Проверка

            // Проверка, того что был вызван метод
            _mockComObject.VerifySet(c => c.SomeProperty = It.IsAny<object>(), Times.Once());
            _mockComObject.VerifySet(c => c.SomeProperty = _argument, Times.Once());
        }

        /// <summary>
        /// Тестирование получения значения индексатора у экземпляра объекта <see cref="OneSObject"/>.
        /// </summary>
        /// <remarks>
        /// Тест проверяет, что при вызове индекатора у аргументов индекса вызывается
        /// метод <see cref="IOneSProxy.Unwrap"/>, если они поддерживают интерфейс
        /// <see cref="IOneSProxy"/>.
        /// Кроме этого проверяется вызов метода <see cref="IOnesWrapper.Wrap"/>
        /// у экземпляра обертывателя.
        /// </remarks>
        [Test(Description = "Тестирование получения значения индексатора.")]
        public void TestGetIndex()
        {
            // Подготовка окружения
            var expectedResult = new object();
            _mockComObject.SetupGet(c => c[It.IsAny<object>()]).Returns(expectedResult);

            InitTestingObject();

            // Выполнение
            var actualResult = _testingObject[_wrappedArgument];

            // Проверка

            // Проверка, того что был вызван метод
            _mockComObject.Verify(c => c[It.IsAny<object>()], Times.Once());
            _mockComObject.Verify(c => c[_argument], Times.Once());

            // Проверка полученного результата
            Assert.IsInstanceOf<TestProxy>(actualResult);
            Assert.AreSame(expectedResult, ((IOneSProxy)actualResult).Unwrap());
        }

        /// <summary>
        /// Тестирование установки значения индексатора у экземпляра объекта <see cref="OneSObject"/>.
        /// </summary>
        /// <remarks>
        /// Тест проверяет, что при вызове индекатора у аргументов индекса и нового значения вызывается
        /// метод <see cref="IOneSProxy.Unwrap"/>, если они поддерживают интерфейс
        /// <see cref="IOneSProxy"/>.
        /// </remarks>
        /// <param name="shouldBeWrap">Следует ли обертывать новое значение.</param>
        [Test(Description = "Тестирование установки значения индексатора.")]
        public void TestSetIndex([Values(true, false)] bool shouldBeWrap)
        {
            // Подготовка окружения
            InitTestingObject();
            var newValue = new object();
            var wrappedNewValue = WrapArgument(shouldBeWrap, newValue);

            // Выполнение
            _testingObject[_wrappedArgument] = wrappedNewValue;

            // Проверка
            // Проверка, того что был вызван метод
            _mockComObject.VerifySet(c => c[_argument] = newValue, Times.Once());
        }

        #region Вспомогательные типы

        /// <summary>
        /// Некий контракт для тестирования.
        /// </summary>
        public interface ISomeContract
        {
            /// <summary>
            /// Некоторая функция для тестирования.
            /// </summary>
            /// <param name="arg">Аргумент функции.</param>
            /// <returns></returns>
            object SomeFunction(object arg);

            /// <summary>
            /// Некоторое действие для тестирования.
            /// </summary>
            /// <param name="arg">Аргумент для тестирования.</param>
            void SomeAction(object arg);

            /// <summary>
            /// Некоторое свойство для тестирования.
            /// </summary>
            object SomeProperty { get; set; }

            /// <summary>
            /// Индексатор для тестирования.
            /// </summary>
            /// <param name="index">Параметер индекса.</param>
            object this[object index] { get; set; }
        }

        /// <summary>Тестовый прокси.</summary>
        private sealed class TestProxy : IOneSProxy
        {
            /// <summary>
            /// Оригинальный объект.
            /// </summary>
            private readonly object _originObject;

            /// <summary>
            /// Конструктор принимающий оригинальный объект.
            /// </summary>
            /// <param name="originObject">
            /// Оригинальный объект
            /// </param>
            public TestProxy(object originObject)
            {
                _originObject = originObject;
            }

            object IOneSProxy.Unwrap()
            {
                return _originObject;
            }
        }

        #endregion
    }
}