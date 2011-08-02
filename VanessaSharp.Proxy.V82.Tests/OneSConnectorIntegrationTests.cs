using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using VanessaSharp.Proxy.V82;

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
                const string userName = "Абдулов (директор)";
                var connectString = string.Format(
                    "File=\"{0}\";Usr=\"{1}\";", Path.Combine(Environment.CurrentDirectory, "DbExample"), userName);

                var context = testingInstance.Connect(connectString);
                try
                {
                    Assert.NotNull(context);
                    var sessionParameters = context.SessionParameters;
                    try
                    {
                        var currentUser = sessionParameters.ТекущийПользователь;
                        try
                        {
                            string currentUserName = currentUser.Code;
                            Assert.AreEqual(userName, currentUserName.Trim());
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(currentUser);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(sessionParameters);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(context);
                }
            }
        }
    }
}
