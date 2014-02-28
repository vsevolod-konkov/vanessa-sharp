using NUnit.Framework;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSParameter"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSParameterTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSParameter.ParameterNameChanging"/>.
        /// </summary>
        [Test]
        public void TestParameterNameChanging()
        {
            // Arrange
            const string OLD_PARAM_NAME = "Параметр1";
            const string NEW_PARAM_NAME = "Параметр2";

            var testingInstance = new OneSParameter(OLD_PARAM_NAME);

            bool wasRaiseEvent = false;
            testingInstance.ParameterNameChanging += (sender, args) =>
                {
                    wasRaiseEvent = true;

                    Assert.AreSame(testingInstance, sender);
                    Assert.AreEqual(OLD_PARAM_NAME, args.OldName);
                    Assert.AreEqual(NEW_PARAM_NAME, args.NewName);
                };

            // Act
            testingInstance.ParameterName = NEW_PARAM_NAME;

            // Assert
            Assert.IsTrue(wasRaiseEvent);
        }
    }
}
