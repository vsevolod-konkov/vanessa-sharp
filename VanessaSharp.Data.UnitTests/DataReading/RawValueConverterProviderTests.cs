using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="RawValueConverterProvider"/>.
    /// </summary>
    [TestFixture]
    public sealed class RawValueConverterProviderTests
    {
        private Mock<IOneSObjectSpecialConverter> _specialConverterMock;

        private RawValueConverterProvider _testedInstance;

        [SetUp]
        public void SetUp()
        {
            _specialConverterMock = new Mock<IOneSObjectSpecialConverter>(MockBehavior.Strict);
            _testedInstance = new RawValueConverterProvider(_specialConverterMock.Object);
        }

        /// <summary>
        /// Тестирование <see cref="RawValueConverterProvider.GetRawValueConverter"/>,
        /// в случае если целевой тип <see cref="Guid"/>.
        /// </summary>
        [Test]
        public void TestGetRawValueConverterWhenGuid()
        {
            // Arrange
            var expectedResult = Guid.NewGuid();

            _specialConverterMock
                .Setup(c => c.ToGuid(It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var rawValue = new object();
            var converter = _testedInstance.GetRawValueConverter(typeof(Guid));
            Assert.IsNotNull(converter);
            var actualResult = converter(rawValue);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);

            _specialConverterMock
                .Verify(c => c.ToGuid(rawValue), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="RawValueConverterProvider.GetRawValueConverter"/>,
        /// в случае если целевой тип <see cref="OneSDataReader"/>.
        /// </summary>
        [Test]
        public void TestGetRawValueConverterWhenOneSDataReader()
        {
            // Arrange
            var expectedResult = OneSDataReader.CreateTablePartDataReader(
                new Mock<IQueryResult>(MockBehavior.Strict).Object);

            _specialConverterMock
                .Setup(c => c.ToDataReader(It.IsAny<object>()))
                .Returns(expectedResult);

            // Act
            var rawValue = new object();
            var converter = _testedInstance.GetRawValueConverter(typeof(OneSDataReader));
            Assert.IsNotNull(converter);
            var actualResult = converter(rawValue);

            // Assert
            Assert.AreSame(expectedResult, actualResult);

            _specialConverterMock
                .Verify(c => c.ToDataReader(rawValue), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="RawValueConverterProvider.GetRawValueConverter"/>,
        /// в иных случаях.
        /// </summary>
        [Test]
        public void TestGetRawValueConverterWhen(
            [Values(typeof(int), typeof(string), typeof(double))] Type type)
        {
            // Arrange
            var rawValue = new object();

            // Act
            var converter = _testedInstance.GetRawValueConverter(type);
            
            // Assert
            Assert.IsNull(converter);
        }
    }
}
