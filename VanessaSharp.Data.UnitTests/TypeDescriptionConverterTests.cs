using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>Тестирование <see cref="TypeDescriptionConverter"/>.</summary>
    [TestFixture]
    public sealed class TypeDescriptionConverterTests
    {
        /// <summary>
        /// Мок <see cref="IOneSTypeConverter"/>
        /// для тестируемого экземпляра.
        /// </summary>
        private Mock<IOneSTypeConverter> _oneSTypeConverterMock;

        /// <summary>Тестируемый экземпляр.</summary>
        private TypeDescriptionConverter _testedInstance;

        /// <summary>Мок массива с типами для описания типа.</summary>
        private DisposableMock<IOneSArray<IOneSType>> _typesArrayMock;

        /// <summary>Мок объекта описания типа.</summary>
        private Mock<ITypeDescription> _typeDescriptionMock;

        /// <summary>
        /// Карта соответствия типов 1С типам CLR.
        /// </summary>
        private readonly List<Tuple<IOneSType, Type>> _typesMap = new List<Tuple<IOneSType, Type>>();

        /// <summary>
        /// Список моков экземпляров для которых должен проверяться вызов
        /// <see cref="IDisposable.Dispose"/>.
        /// </summary>
        private readonly List<IDisposableMock> _disposableMocks = new List<IDisposableMock>();

        /// <summary>
        /// Добавление типа 1С в описание 
        /// конвертируемого в <typeparamref name="T"/>.
        /// </summary>
        private void AddConvertibleTypeTo<T>()
        {
            AddType(typeof(T));
        }

        /// <summary>Добавление пустого типа 1С.</summary>
        private void AddEmptyType()
        {
            AddConvertibleTypeTo<DBNull>();
        }

        /// <summary>
        /// Добавление типа 1С,
        /// который невозможно конвертировать в тип CLR.
        /// </summary>
        private void AddUnconvertibleType()
        {
            AddType(null);
        }

        /// <summary>
        /// Добавление типа CLR в тест с добавлением соответствия типу 1С.
        /// </summary>
        private void AddType(Type clrType)
        {
            AddMap(
                CreateVerifyableDisposableMock<IOneSType>().Object,
                clrType
                );
        }

        /// <summary>Добавление соответствия.</summary>
        private void AddMap(IOneSType oneSType, Type clrType)
        {
            _typesMap.Add(Tuple.Create(oneSType, clrType));
        }

        /// <summary>
        /// Создание мока, у которого должен проверяться вызов 
        /// метода <see cref="IDisposable.Dispose"/>.
        /// </summary>
        private DisposableMock<T> CreateVerifyableDisposableMock<T>()
            where T : class, IDisposable
        {
            var result = new DisposableMock<T>();

            _disposableMocks.Add(result);

            return result;
        }

        /// <summary>
        /// Тестирование метода <see cref="TypeDescriptionConverter.ConvertFrom"/>.
        /// </summary>
        /// <param name="expectedResult">Ожидаемый результат.</param>
        private void TestConvertFrom(Type expectedResult)
        {
            EndArrangeTest();

            ActAndAssertResultConvertFrom(expectedResult);

            VerifyCalls();
        }

        /// <summary>Завершение подготовки теста.</summary>
        private void EndArrangeTest()
        {
            SetupTryConvertFrom();
            SetupTypesArrayGet();
            SetupTypesArrayCount();
        }

        /// <summary>
        /// Установка моковой реализации
        /// <see cref="IOneSTypeConverter.TryConvertFrom"/>
        /// для всех добавленных типов методом <see cref="AddType"/>.
        /// </summary>
        private void SetupTryConvertFrom()
        {
            foreach (var map in _typesMap)
                SetupTryConvertFrom(map.Item1, map.Item2);
        }

        /// <summary>
        /// Установка моковой реализации
        /// <see cref="IOneSTypeConverter.TryConvertFrom"/>
        /// для данного соответствия.
        /// </summary>
        private void SetupTryConvertFrom(IOneSType oneSType, Type clrType)
        {
            _oneSTypeConverterMock
                .Setup(c => c.TryConvertFrom(oneSType))
                .Returns(clrType)
                .Verifiable();
        }

        /// <summary>
        /// Установка моковой реализации
        /// для <see cref="IOneSArray{T}.Get"/>
        /// для всех добавленных типов.
        /// </summary>
        private void SetupTypesArrayGet()
        {
            for (var i = 0; i < _typesMap.Count; i++)
            {
                var index = i;
                _typesArrayMock
                    .Setup(array => array.Get(index))
                    .Returns(_typesMap[index].Item1)
                    .Verifiable();
            }
        }

        /// <summary>
        /// Установка моковой реализации
        /// для <see cref="IOneSArray.Count"/>.
        /// </summary>
        private void SetupTypesArrayCount()
        {
            _typesArrayMock
                .Setup(array => array.Count())
                .Returns(_typesMap.Count)
                .Verifiable();
        }

        /// <summary>
        /// Вызов тестового метода и проверка полученного результата.
        /// </summary>
        /// <param name="expectedResult">
        /// Ожидаемый результат.
        /// </param>
        private void ActAndAssertResultConvertFrom(Type expectedResult)
        {
            Assert.AreEqual(
                expectedResult,
                _testedInstance.ConvertFrom(_typeDescriptionMock.Object)
                );
        }

        /// <summary>
        /// Проверка того, что были сделаны нужные вызовы.
        /// </summary>
        private void VerifyCalls()
        {
            VerifyTryConvertFrom();
            VerifyTypesArrayGet();
            VerifyDisposes();
        }

        /// <summary>
        /// Проверка вызовов <see cref="IOneSTypeConverter.TryConvertFrom"/>
        /// для всех добавленных типов.
        /// </summary>
        private void VerifyTryConvertFrom()
        {
            foreach (var map in _typesMap)
                VerifyTryConvertFrom(map.Item1);
        }

        /// <summary>
        /// Проверка вызовов <see cref="IOneSTypeConverter.TryConvertFrom"/>
        /// для данного типа.
        /// </summary>
        private void VerifyTryConvertFrom(IOneSType oneSType)
        {
            _oneSTypeConverterMock
                .Verify(c => c.TryConvertFrom(oneSType), Times.Once());
        }

        /// <summary>
        /// Проверка вызовов
        /// <see cref="IOneSArray{T}.Get"/>
        /// для всех добавленных типов.
        /// </summary>
        private void VerifyTypesArrayGet()
        {
            for (var i = 0; i < _typesMap.Count; i++)
            {
                var index = i;
                _typesArrayMock.Verify(array => array.Get(index), Times.Once());
            }
        }

        /// <summary>
        /// Проверка вызовов <see cref="IDisposable.Dispose"/>.
        /// </summary>
        private void VerifyDisposes()
        {
            foreach (var disposableMock in _disposableMocks)
                disposableMock.VerifyDispose();
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _oneSTypeConverterMock = new Mock<IOneSTypeConverter>(MockBehavior.Strict);
            _testedInstance = new TypeDescriptionConverter(_oneSTypeConverterMock.Object);
            _typesArrayMock = CreateVerifyableDisposableMock<IOneSArray<IOneSType>>();

            _typeDescriptionMock = new Mock<ITypeDescription>(MockBehavior.Strict);
            _typeDescriptionMock
                .Setup(t => t.Types)
                .Returns(_typesArrayMock.Object)
                .Verifiable();
        }

        /// <summary>Очистка после выполнения теста.</summary>
        [TearDown]
        public void TearDown()
        {
            _typesMap.Clear();
            _disposableMocks.Clear();
        }

        /// <summary>Тестирование в случае когда описание типа состоит из одного конвертируемого типа.</summary>
        [Test]
        public void TestWhenOnlyOneConvertableType([Values(typeof(string), typeof(Guid))] Type expectedType)
        {
            // Arrange
            AddType(expectedType);

            // Arrange-Act-Assert
            TestConvertFrom(expectedType);
        }

        /// <summary>Тестирование в случае когда описание типа состоит из одного неконвертируемого типа.</summary>
        [Test]
        public void TestWhenOnlyOneUnconvertableType()
        {
            // Arrange
            AddUnconvertibleType();

            // Arrange-Act-Assert
            TestConvertFrom(typeof(object));
        }

        /// <summary>
        /// Тестирование в случае когда описание типа состоит из одного конвертируемого типа
        /// и одного пустого типа.
        /// </summary>
        /// <param name="expectedType">Ожидаемый результат.</param>
        [Test]
        public void TestWhenTwoTypesAndOneConvertableTypeOneDbNull([Values(typeof(string), typeof(Guid))] Type expectedType)
        {
            // Arrange
            AddType(expectedType);
            AddEmptyType();

            // Arrange-Act-Assert
            TestConvertFrom(expectedType);
        }

        /// <summary>
        /// Тестирование в случае когда описание типа состоит из одного неконвертируемого типа
        /// и одного пустого типа.
        /// </summary>
        [Test]
        public void TestWhenTwoTypesAndOneUnconvertableTypeOneDbNull()
        {
            // Arrange
            AddUnconvertibleType();
            AddEmptyType();

            // Arrange-Act-Assert
            TestConvertFrom(typeof(object));
        }

        /// <summary>
        /// Тестирование в случае когда описание типа состоит из двух конвертируемых типа.
        /// </summary>
        [Test]
        public void TestWhenTwoConvertibleTypes()
        {
            // Arrange
            AddConvertibleTypeTo<string>();
            AddConvertibleTypeTo<int>();

            // Arrange-Act-Assert
            TestConvertFrom(typeof(object));
        }

        /// <summary>
        /// Тестирование в случае когда 
        /// описание типа состоит из нескольих конвертируемых типов,
        /// соответствующие одному типу CLR.
        /// </summary>
        [Test]
        public void TestWhenManyConvertibleTypesMatchSingleClrType([Values(typeof(string), typeof(Guid))] Type expectedType)
        {
            // Arrange
            for (var counter = 0; counter < 3; counter++)
                AddType(expectedType);    
            
            // Arrange-Act-Assert
            TestConvertFrom(expectedType);
        }

        /// <summary>
        /// Тестирование в случае 
        /// когда описание типа состоит из нескольких конвертируемых типов,
        /// соответствующие одному типу CLR и есть один пустой.
        /// </summary>
        [Test]
        public void TestWhenManyConvertibleTypesMatchSingleClrTypeAndOneEmptyType([Values(typeof(string), typeof(Guid))] Type expectedType)
        {
            // Arrange
            for (var counter = 0; counter < 3; counter++)
                AddType(expectedType);

            AddEmptyType();

            // Arrange-Act-Assert
            TestConvertFrom(expectedType);
        }

        /// <summary>
        /// Тестирование в случае когда описание типа состоит из 
        /// нескольких конвертируемых типов,
        /// соответствующие нескольким типам CLR.
        /// </summary>
        [Test]
        public void TestWhenManyConvertibleTypesMatchManyClrTypes()
        {
            // Arrange
            for (var counter = 0; counter < 2; counter++)
                AddConvertibleTypeTo<string>();

            for (var counter = 0; counter < 2; counter++)
                AddConvertibleTypeTo<int>();

            // Arrange-Act-Assert
            TestConvertFrom(typeof(object));
        }

        /// <summary>
        /// Тестирование в случае когда описание типа 
        /// состоит из нескольких конвертируемых 
        /// и неконвертируемых типов,
        /// </summary>
        [Test]
        public void TestWhenManyConvertibleAndUnconvertibleTypes()
        {
            // Arrange
            for (var counter = 0; counter < 2; counter++)
                AddConvertibleTypeTo<string>();

            for (var counter = 0; counter < 2; counter++)
                AddUnconvertibleType();

            // Arrange-Act-Assert
            TestConvertFrom(typeof(object));
        }

        /// <summary>
        /// Тестирование <see cref="TypeDescriptionConverter.GetDataTypeName"/>
        /// в случае наличия одного типа.
        /// </summary>
        [Test]
        public void TestGetDataTypeNameWhenSingleType()
        {
            // Arrange
            const string EXPECTED_DATA_TYPE_NAME = "Test";

            var valueType = CreateVerifyableDisposableMock<IOneSType>().Object;

            _typesArrayMock
                .Setup(a => a.Count())
                .Returns(1);
            _typesArrayMock
                .Setup(a => a.Get(0))
                .Returns(valueType);

            _oneSTypeConverterMock
                .Setup(c => c.GetTypeName(valueType))
                .Returns(EXPECTED_DATA_TYPE_NAME);

            // Act
            var result = _testedInstance.GetDataTypeName(_typeDescriptionMock.Object);

            // Assert
            Assert.AreEqual(EXPECTED_DATA_TYPE_NAME, result);
        }

        /// <summary>
        /// Тестирование <see cref="TypeDescriptionConverter.GetDataTypeName"/>
        /// в случае наличия двух типов.
        /// </summary>
        [Test]
        public void TestGetDataTypeNameWhenManyTypes()
        {
            // Arrange
            var valueType1 = CreateVerifyableDisposableMock<IOneSType>().Object;
            var valueType2 = CreateVerifyableDisposableMock<IOneSType>().Object;

            _typesArrayMock
                .Setup(a => a.Count())
                .Returns(2);
            _typesArrayMock
                .Setup(a => a.Get(0))
                .Returns(valueType1);
            _typesArrayMock
                .Setup(a => a.Get(1))
                .Returns(valueType2);

            _oneSTypeConverterMock
                .Setup(c => c.GetTypeName(valueType1))
                .Returns("Test1");
            _oneSTypeConverterMock
                .Setup(c => c.GetTypeName(valueType2))
                .Returns("Test2");

            // Act
            var result = _testedInstance.GetDataTypeName(_typeDescriptionMock.Object);

            // Assert
            Assert.AreEqual("Test1,Test2", result);
        }
    }
}
