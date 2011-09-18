using System;
using System.IO;
using VanessaSharp.Proxy.Common;
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
            CopyProxyAssemblyFiles();

            var appDomain = AppDomain.CreateDomain("test", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                appDomain.DoCallBack(() =>
                {
                    var connector = OneSConnectorFactory.Default.Create("8.2");
                    connector.Dispose();
                });
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        /// <summary>Копирование файлов сборки.</summary>
        private static void CopyProxyAssemblyFiles()
        {
            const string sourcePath = @"..\..\..\VanessaSharp.Proxy.V82\bin";
            var assemblyFiles = new[] { 
                                        "VanessaSharp.Proxy.V82.dll",
                                        "Interop.V82.dll" 
                                      };
            var parentDirectory = new DirectoryInfo(@".").Name;
            
            foreach (var assemblyFile in assemblyFiles)
                File.Copy(Path.Combine(sourcePath, parentDirectory, assemblyFile), assemblyFile, true);
        }
    }
}
