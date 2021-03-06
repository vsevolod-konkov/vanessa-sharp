﻿using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSTypeConverter"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSTypeConverterTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSTypeConverter.GetTypeName"/>.
        /// </summary>
        [Test]
        public void TestGetTypeName()
        {
            // Arrange
            const string EXPECTED_TYPE_NAME = "Test";

            var globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);

            var oneSTypeMock = new Mock<IOneSType>(MockBehavior.Strict);
            oneSTypeMock
                .SetupGet(t => t.GlobalContext)
                .Returns(globalContextMock.Object);
            var oneSType = oneSTypeMock.Object;

            globalContextMock
                .Setup(ctx => ctx.String(oneSType))
                .Returns(EXPECTED_TYPE_NAME);

            // Act
            var result = OneSTypeConverter.Default.GetTypeName(oneSType);

            // Assert
            Assert.AreEqual(EXPECTED_TYPE_NAME, result);
        }
        
        /// <summary>
        /// Тестирование метода <see cref="OneSTypeConverter.TryConvertFrom"/>.
        /// </summary>
        [Test]
        [TestCase("Неизвестный тип", null)]
        [TestCase("Строка", typeof(string))]
        [TestCase("Число", typeof(double))]
        [TestCase("Булево", typeof(bool))]
        [TestCase("Дата", typeof(DateTime))]
        [TestCase("Null", typeof(DBNull))]
        [TestCase("Уникальный идентификатор", typeof(Guid))]
        [TestCase("Результат запроса", typeof(OneSDataReader))]
        public void TestTryConvertFrom(string oneSTypeString, Type expectedType)
        {
            // Arrange
            var globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);

            var oneSTypeMock = new Mock<IOneSType>(MockBehavior.Strict);
            oneSTypeMock
                .SetupGet(t => t.GlobalContext)
                .Returns(globalContextMock.Object)
                .Verifiable();

            var oneSType = oneSTypeMock.Object;

            globalContextMock
                .Setup(ctx => ctx.String(oneSType))
                .Returns(oneSTypeString)
                .Verifiable();
            
            // Act and Assert
            Assert.AreEqual(
                expectedType, 
                OneSTypeConverter.Default.TryConvertFrom(oneSType));

            globalContextMock
                .Verify(ctx => ctx.String(oneSType), Times.Once());
        }
    }
}
