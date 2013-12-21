using NUnit.Framework;
using System;
using System.IO;

namespace VanessaSharp.Proxy.V82.Tests
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
            using (var testingInstance = new OneSConnector())
            {
                const string USER_NAME = "Абдулов (директор)";
                var connectString = string.Format(
                    "File=\"{0}\";Usr=\"{1}\";", Path.Combine(Environment.CurrentDirectory, "DbExample"), USER_NAME);

                using (dynamic context = testingInstance.Connect(connectString))
                using (var sessionParameters = context.SessionParameters)
                using (var currentUser = sessionParameters.ТекущийПользователь)
                {
                    string currentUserName = currentUser.Code;
                    Assert.AreEqual(USER_NAME, currentUserName.Trim());
                }
            }
        }
    }
}
