using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестмрование коннектора.</summary>
    [TestFixture]
    public sealed class ConnectorTests
    {
        private const string CATALOG = @"C:\Users\Мастер\Documents\1C\DemoTrd";
        
        /// <summary>
        /// Тестирование правильности построения строки соединения для метода 
        /// <see cref="V8.COMConnectorClass.Connect"/>.
        /// </summary>
        [Test(Description="Тестирование подключения к 1С по построенной строке")]
        public void TestValidBuildsConnectionString()
        {
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = CATALOG;
                builder.User = "Иванов";

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.User = "Иванов";
                builder.Catalog = CATALOG;

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.ConnectionString = string.Format("File={0};Usr=Иванов", CATALOG);
                builder.Password = string.Empty;

                CheckConnect(builder);
            }
        }

        /// <summary>Тестирование глобального контекста.</summary>
        [Test(Description="Тестирование глобального контекста")]
        public void TestGlobalContext()
        {
            var parameters = new Proxies.ConnectionParameters();
            parameters.ConnectionString = string.Format("File={0};Usr=Иванов", CATALOG);
            parameters.PoolTimeout = 2000;
            parameters.PoolCapacity = 10;

            using (var globalContext = Proxies.GlobalContext.Connect(parameters))
            {
                Assert.AreEqual(2000, globalContext.PoolTimeout);
                Assert.AreEqual(10, globalContext.PoolCapacity);
            }
        }

        /// <summary>Проверка соединения.</summary>
        /// <param name="builder">Построитель строки.</param>
        private static void CheckConnect(OneSConnectionStringBuilder builder)
        {
            var connector = new V8.COMConnectorClass();
            try
            {
                Assert.IsNotNull(connector);

                var root = connector.Connect(builder.ConnectionString);
                try
                {
                    Assert.IsNotNull(root);
                }
                finally
                {
                    Marshal.ReleaseComObject(root);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(connector);
            }
        }
    }
}
