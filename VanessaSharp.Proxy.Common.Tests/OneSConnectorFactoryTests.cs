using System;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Тесты на <see cref="OneSConnectorFactory"/>.</summary>
    [TestFixture]
    public sealed class OneSConnectorFactoryTests
    {
        /// <summary>Тестирование метода <see cref="OneSConnectorFactory"/>.</summary>
        [Test]
        public void TestCreate()
        {
            var appDomain = AppDomain.CreateDomain("test", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                appDomain.DoCallBack(() =>
                {
                    var connector = OneSConnectorFactory.Default.Create(new ConnectorCreationParams { Version = OneSVersion.V82 });
                    connector.Dispose();
                });
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }
    }
}
