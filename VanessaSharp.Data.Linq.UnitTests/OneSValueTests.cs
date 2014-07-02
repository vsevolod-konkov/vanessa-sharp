using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование <see cref="OneSValue"/>.</summary>
    [TestFixture]
    public sealed class OneSValueTests
    {
        /// <summary>Мок <see cref="IValueConverter"/>.</summary>
        private Mock<IValueConverter> _valueConverterMock;

        /// <summary>Сырое значение.</summary>
        private object _rawValue;

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSValue _testedInstance;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _valueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);
            _rawValue = null;
            _testedInstance = null;
        }

        /// <summary>Создание тестируемого экземпляра.</summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        private void CreateTestedInstance(object rawValue)
        {
            _rawValue = rawValue;
            _testedInstance = new OneSValue(rawValue, _valueConverterMock.Object);
        }

        /// <summary>
        /// Тестирование <see cref="OneSValue.RawValue"/>
        /// в случае если исходный объект не является нулевым.
        /// </summary>
        [Test]
        public void TestToObjectWhenIsNotNull()
        {
            CreateTestedInstance(new object());

            Assert.AreSame(_rawValue, _testedInstance.RawValue);
            Assert.IsFalse(_testedInstance.IsNull);
        }

        /// <summary>
        /// Тестирование <see cref="OneSValue.RawValue"/>
        /// в случае если исходный объект является нулевым.
        /// </summary>
        [Test]
        public void TestToObjectWhenIsNull()
        {
            CreateTestedInstance(null);

            Assert.IsNull(_testedInstance.RawValue);
            Assert.IsTrue(_testedInstance.IsNull);
        }

        /// <summary>
        /// Тестирование конвертации экземпляра <see cref="OneSValue"/>
        /// в значение типа <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Тип, в значение которого преобразуется тестируемый экземпляр.</typeparam>
        /// <param name="expectedValue">Ожидаемое значение.</param>
        /// <param name="convertedAction">
        /// Выражение выполнения метода <see cref="IValueConverter"/>, необходимое для тестируемого действия.
        /// </param>
        /// <param name="testedAction">Тестируемое действие.</param>
        private void TestConvertTo<T>(T expectedValue, Expression<Func<IValueConverter, T>> convertedAction, Func<OneSValue, T> testedAction)
        {
            // Arrange
            _valueConverterMock
                .Setup(convertedAction)
                .Returns(expectedValue);

            var rawValue = new object();
            CreateTestedInstance(rawValue);

            // Act
            var actualValue = testedAction(_testedInstance);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);

            var verifiedLambda = SubstituteValue(convertedAction, rawValue);

            _valueConverterMock
                .Verify(verifiedLambda, Times.Once());
        }

        /// <summary>
        /// Замена в выражении вызова метода конвертера <see cref="IValueConverter"/>
        /// аргумента на сырое значение, которое должно быть преобразовано.
        /// </summary>
        /// <typeparam name="T">Тип значения к которому делается преобразование метода <see cref="IValueConverter"/>.</typeparam>
        /// <param name="convertedAction">Выражение вызова метода конвертора.</param>
        /// <param name="rawValue">Сырое значение.</param>
        private static Expression<Func<IValueConverter, T>> SubstituteValue<T>(
            Expression<Func<IValueConverter, T>> convertedAction, object rawValue)
        {
            var convertMethod = ((MethodCallExpression)convertedAction.Body).Method;

            var parameter = Expression.Parameter(typeof(IValueConverter), "c");
            
            return Expression.Lambda<Func<IValueConverter, T>>(
                Expression.Call(parameter, convertMethod, Expression.Constant(rawValue)),
                parameter);
        }

        /// <summary>Тестирование конвертации в строку.</summary>
        [Test]
        public void TestConvertString()
        {
            TestConvertTo("Test", c => c.ToString(It.IsAny<object>()), v => (string)v);
        }

        /// <summary>Тестирование конвертации в <see cref="char"/>.</summary>
        [Test]
        public void TestConvertChar()
        {
            TestConvertTo('Z', c => c.ToChar(It.IsAny<object>()), v => (char)v);
        }

        /// <summary>Тестирование конвертации в <see cref="byte"/>.</summary>
        [Test]
        public void TestConvertByte()
        {
            TestConvertTo((byte)0x10, c => c.ToByte(It.IsAny<object>()), v => (byte)v);
        }

        /// <summary>Тестирование конвертации в <see cref="short"/>.</summary>
        [Test]
        public void TestConvertInt16()
        {
            TestConvertTo((short)10, c => c.ToInt16(It.IsAny<object>()), v => (short)v);
        }

        /// <summary>Тестирование конвертации в <see cref="int"/>.</summary>
        [Test]
        public void TestConvertInt32()
        {
            TestConvertTo(10, c => c.ToInt32(It.IsAny<object>()), v => (int)v);
        }

        /// <summary>Тестирование конвертации в <see cref="long"/>.</summary>
        [Test]
        public void TestConvertInt64()
        {
            TestConvertTo(4325325656346L, c => c.ToInt64(It.IsAny<object>()), v => (long)v);
        }

        /// <summary>Тестирование конвертации в <see cref="float"/>.</summary>
        [Test]
        public void TestConvertFloat()
        {
            TestConvertTo(14.54f, c => c.ToFloat(It.IsAny<object>()), v => (float)v);
        }

        /// <summary>Тестирование конвертации в <see cref="double"/>.</summary>
        [Test]
        public void TestConvertDouble()
        {
            TestConvertTo(14.54, c => c.ToDouble(It.IsAny<object>()), v => (double)v);
        }

        /// <summary>Тестирование конвертации в <see cref="decimal"/>.</summary>
        [Test]
        public void TestConvertDecimal()
        {
            TestConvertTo(14.54m, c => c.ToDecimal(It.IsAny<object>()), v => (decimal)v);
        }

        /// <summary>Тестирование конвертации в <see cref="bool"/>.</summary>
        [Test]
        public void TestConvertBoolean([Values(false, true)] bool value)
        {
            TestConvertTo(value, c => c.ToBoolean(It.IsAny<object>()), v => (bool)v);
        }

        /// <summary>Тестирование конвертации в <see cref="DateTime"/>.</summary>
        [Test]
        public void TestConvertDateTime()
        {
            TestConvertTo(DateTime.Now, c => c.ToDateTime(It.IsAny<object>()), v => (DateTime)v);
        }
    }
}
