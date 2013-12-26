using System;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Тестирование <see cref="OneSWrapFactory"/>.</summary>
    [TestFixture]
    public sealed class OneSWrapFactoryTestsThroughMapObject : OneSWrapFactoryTestsBase
    {
        /// <summary>Мок реализации <see cref="IOneSWrapMap"/>.</summary>
        private Mock<IOneSWrapMap> _mapMock;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        protected override OneSWrapFactory InitTestedInstance()
        {
            _mapMock = new Mock<IOneSWrapMap>(MockBehavior.Strict);

            return new OneSWrapFactory(_mapMock.Object);
        }

        private void SetupGetObjectCreator(Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> creator)
        {
            _mapMock
                .Setup(m => m.GetObjectCreator(It.IsAny<Type>()))
                .Returns(creator)
                .Verifiable();
        }

        private void VerifyGetObjectCreator(Type type)
        {
            _mapMock
                .Verify(m => m.GetObjectCreator(type), Times.Once());
        }

        private void SetupGetOneSObjectTypeName(string typeName)
        {
            _mapMock
                .Setup(m => m.GetOneSObjectTypeName(It.IsAny<Type>()))
                .Returns(typeName)
                .Verifiable();
        }

        private void VerifyGetOneSObjectTypeName(Type type)
        {
            _mapMock
                .Verify(m => m.GetOneSObjectTypeName(type), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapFactory.CreateWrap"/>
        /// в случае когда тип поддерживается тестируемой фабрикой.
        /// </summary>
        [Test]
        public override void TestCreateWrapWhenTypeSupported()
        {
            // Arrange
            object argObj = null;
            IOneSProxyWrapper argProxyWrapper = null;
            OneSGlobalContext argGlobalContext = null;
            OneSObject creatorResult = null;

            Func<object, IOneSProxyWrapper, OneSGlobalContext, OneSObject> creator = (o, w, ctx) =>
                {
                    argObj = o;
                    argProxyWrapper = w;
                    argGlobalContext = ctx;

                    creatorResult = new TestObject(o, w, ctx);

                    return creatorResult;
                };

            SetupGetObjectCreator(creator);

            // Arrange - Act - Assert
            object obj;
            CreateWrapParameters createWrapParameters;
            var result = TestCreateWrapWhenTypeSupported(out obj, out createWrapParameters);

            // Assert
            VerifyGetObjectCreator(typeof(ITestObject));
            Assert.AreSame(obj, argObj);
            Assert.AreSame(createWrapParameters.ProxyWrapper, argProxyWrapper);
            Assert.AreSame(createWrapParameters.GlobalContext, argGlobalContext);
            Assert.AreSame(creatorResult, result);
        }

        /// <summary>
        /// Тестирование <see cref="OneSWrapFactory.CreateWrap"/>
        /// в случае когда тип не поддерживается тестируемой фабрикой.
        /// </summary>
        [Test]
        public override void TestCreateWrapWhenTypeNotSupported()
        {
            // Arrange
            SetupGetObjectCreator(null);

            // Arrange - Act - Assert
            base.TestCreateWrapWhenTypeNotSupported();

            // Assert
            VerifyGetObjectCreator(typeof(IAnotherTestObject));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapFactory.GetTypeNameFor"/>
        /// в случае поддержки типа.
        /// </summary>
        [Test]
        public override void TestGetTypeNameForWhenTypeSupported()
        {
            // Arrange
            SetupGetOneSObjectTypeName(TEST_ONES_TYPE_NAME);

            // Arrange - Act - Assert
            base.TestGetTypeNameForWhenTypeSupported();

            // Assert
            VerifyGetOneSObjectTypeName(typeof(ITestObject));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSWrapFactory.GetTypeNameFor"/>
        /// в случае если тип не поддерживается.
        /// </summary>
        [Test]
        public override void TestGetTypeNameForWhenTypeNotSupported()
        {
            // Arrange
            SetupGetOneSObjectTypeName(null);

            // Arrange - Act - Assert
            base.TestGetTypeNameForWhenTypeNotSupported();

            // Assert
            VerifyGetOneSObjectTypeName(typeof(IAnotherTestObject));
        }
    }
}
