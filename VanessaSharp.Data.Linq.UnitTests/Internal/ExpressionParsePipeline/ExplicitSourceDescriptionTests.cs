using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>Тестирование <see cref="ExplicitSourceDescription"/>.</summary>
    [TestFixture]
    public sealed class ExplicitSourceDescriptionTests
    {
        /// <summary>
        /// Тестирование реализации <see cref="ISourceDescription.GetSourceName"/>.
        /// </summary>
        [Test]
        public void TestGetSourceName()
        {
            // Arrange
            const string SOURCE_NAME = "TEST";
            var testedInstance = new ExplicitSourceDescription(SOURCE_NAME);
            Assert.AreEqual(SOURCE_NAME, testedInstance.SourceName);

            var sourceResolver = new Mock<ISourceResolver>(MockBehavior.Strict).Object;

            // Act
            ISourceDescription sourceDescription = testedInstance;
            var result = sourceDescription.GetSourceName(sourceResolver);

            // Arrange
            Assert.AreEqual(SOURCE_NAME, result);
        }
    }
}
