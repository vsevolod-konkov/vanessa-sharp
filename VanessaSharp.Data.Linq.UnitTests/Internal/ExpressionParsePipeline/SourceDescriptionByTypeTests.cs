using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>Тестирование <see cref="SourceDescriptionByType{T}"/>.</summary>
    [TestFixture]
    public sealed class SourceDescriptionByTypeTests
    {
        /// <summary>
        /// Тестирование <see cref="SourceDescriptionByType{T}.GetSourceName"/>.
        /// </summary>
        [Test]
        public void TestGetSourceName()
        {
            // Arrange
            const string SOURCE_NAME = "Test";

            var sourceResolverMock = new Mock<ISourceResolver>(MockBehavior.Strict);
            sourceResolverMock
                .Setup(r => r.ResolveSourceNameForTypedRecord<SomeData>())
                .Returns(SOURCE_NAME);

            // Act
            var result = SourceDescriptionByType<SomeData>.Instance.GetSourceName(sourceResolverMock.Object);

            // Assert
            Assert.AreEqual(SOURCE_NAME, result);
        }

        public sealed class SomeData
        {}
    }
}
