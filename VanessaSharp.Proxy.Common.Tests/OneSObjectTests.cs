using System;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Moq;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тесты на <see cref="OneSObject"/>.
    /// </summary>
    [TestFixture(Description = "Тесты на класс OneSObject")]
    public sealed class OneSObjectTests
    {
        /// <summary>
        /// Следует ли обертывать аргумент.
        /// </summary>
        [Values(true, false)]
        private bool _shouldBeWrap;
        
        /// <summary>Мок нижележащего объекта.</summary>
        private Mock<ISomeContract> _mockComObject;

        /// <summary>Тестируемый объект.</summary>
        private dynamic _testingObject;

        /// <summary>Ожидаемый аргумент нижележащим объектом.</summary>
        private object _argument;

        /// <summary>Обернутый аргумент.</summary>
        private object _wrappedArgument;

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
        /// <param name="shouldBeWrap">
        /// Следует ли обертывать аргумент,
        /// оболочкой поддерживающей интерфейс <see cref="IOneSProxy"/>.
        /// </param>
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
        private void TestCallFunction()
        {
            // Подготовка окружения
            var expectedResult = new object();
            _mockComObject.Setup(c => c.SomeFunction(_argument))
                         .Returns(expectedResult);

            InitTestingObject();

            // Выполнение
            object actualResult = testingObject.SomeFunction(expectedWrappingArg);

            // Проверка

            // Проверка, того что был вызван метод  исходными параметрами
            mockComObject.Verify(c => c.SomeFunction(It.IsAny<object>()), Times.Once());
            mockComObject.Verify(c => c.SomeFunction(expectedArg), Times.Once());

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
        /// Кроме этого проверяется вызов метода <see cref="IOnesWrapper.Wrap"/>
        /// у экземпляра обертывателя.
        /// </remarks>
        /// <param name="shouldBeWrap">
        /// Следует ли обертывать аргумент.
        /// </param>
        [Test(Description = "Тестирование вызова метода не возвращающего значение с проверкой разупаковки аргумента.")]
        private void TestCallAction([Values(true, false)] bool shouldBeWrap)
        {
            // Подготовка окружения
            var mockWrapper = new Mock<IOneSProxyWrapper>();
            mockWrapper.Setup(w => w.Wrap(It.IsAny<object>()))
                       .Returns<object>(o => new TestProxy(o));

            var expectedArg = new object();

            var mockComObject = new Mock<ISomeContract>();
            mockComObject.Setup(c => c.SomeAction(expectedArg));

            dynamic testingObject = new OneSObject(mockComObject.Object, mockWrapper.Object);
            var expectedWrappingArg = WrapArgument(shouldBeWrap, expectedArg);

            // Выполнение
            testingObject.SomeFunction(expectedWrappingArg);

            // Проверка

            // Проверка, того что был вызван метод  исходными параметрами
            mockComObject.Verify(c => c.SomeAction(It.IsAny<object>()), Times.Once());
            mockComObject.Verify(c => c.SomeAction(expectedArg), Times.Once());
        }

        #region Вспомогательные типы

        /// <summary>
        /// Некий контракт для тестирования.
        /// </summary>
        private interface ISomeContract
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