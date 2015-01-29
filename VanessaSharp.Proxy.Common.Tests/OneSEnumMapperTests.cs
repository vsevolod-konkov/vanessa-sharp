using System.Dynamic;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тестирование <see cref="OneSEnumMapper"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSEnumMapperTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSEnumMapper.ConvertComObjectToEnum"/>.
        /// </summary>
        [Ignore("Возникает непонятная ошибка DLR, связанная с ExpandoObject")]
        [Test]
        public void TestConvertComObjectToEnum()
        {
            // Arrange
            dynamic enumObject = new ExpandoObject();

            enumObject.DetailRecord = new object();
            enumObject.GroupTotal = new object();
            enumObject.TotalByHierarchy = new object();
            enumObject.Overall = new object();

            dynamic globalContextObj = new ExpandoObject();
            globalContextObj.SelectRecordType = enumObject;

            var oneSObjectDefinerMock = new Mock<IOneSObjectDefiner>(MockBehavior.Strict);
            oneSObjectDefinerMock
                .Setup(d => d.IsOneSObject(It.IsAny<object>()))
                .Returns<object>(o =>
                        (o is ExpandoObject) || (o != null && o.GetType() == typeof(object))
                    );

            var globalContext = new OneSGlobalContext(globalContextObj, 
                oneSObjectDefinerMock.Object,
                new Mock<IOneSTypeResolver>(MockBehavior.Strict).Object
                );

            var testedInstance = new OneSEnumMapper(globalContext);

            // Act
            var actualValue = testedInstance.ConvertComObjectToEnum(
                enumObject.TotalByHierarchy, typeof(SelectRecordType));

            // Assert
            Assert.AreEqual(SelectRecordType.TotalByHierarchy, actualValue);
        }
    }
}
