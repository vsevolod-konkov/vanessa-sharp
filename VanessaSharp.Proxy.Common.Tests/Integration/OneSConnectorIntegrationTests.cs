using NUnit.Framework;
using System;

namespace VanessaSharp.Proxy.Common.Tests.Integration
{
    /// <summary>
    /// Интеграционные тесты на <see cref="OneSConnector"/>.
    /// </summary>
    [TestFixture(Description="Интеграционные тесты")]
    public sealed class OneSConnectorIntegrationTests
    {
        /// <summary>Тестирование соединения.</summary>
        [Test(Description="Тестирование соединения")]
        public void TestConnect()
        {
            using (var testingInstance = OneSConnector.CreateFromVersion(OneSVersion.V82))
            {
                var connectString = string.Format(
                    "File=\"{0}\";Usr=\"{1}\";", Constants.TestCatalog, Constants.TestUser);

                using (dynamic context = testingInstance.Connect(connectString))
                using (var sessionParameters = context.SessionParameters)
                using (var currentUser = sessionParameters.ТекущийПользователь)
                {
                    string currentUserName = currentUser.Code;
                    Assert.AreEqual(Constants.TestUser, currentUserName.Trim());
                }
            }
        }

        [Test]
        public void TestGuid()
        {
            var id = Guid.NewGuid();
            Console.WriteLine("CLR GUID: " + id.ToString());

            using (var testingInstance = OneSConnector.CreateFromVersion(OneSVersion.V82))
            {
                var connectString = string.Format(
                    "File=\"{0}\";Usr=\"{1}\";", Constants.TestCatalog, Constants.TestUser);

                using (dynamic context = testingInstance.Connect(connectString))
                {
                    using (var oneSUUID = context.NewObject("UUID", id.ToString()))
                    {
                        var str = context.String(oneSUUID);
                        Console.WriteLine("1C GUID: " + str);
                    }
                }
            }
        }
    }
}
