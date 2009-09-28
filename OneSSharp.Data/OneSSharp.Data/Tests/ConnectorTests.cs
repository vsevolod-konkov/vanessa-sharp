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
        /// <summary>
        /// Тестирование правильности построения строки соединения для метода 
        /// <see cref="V8.COMConnectorClass.Connect"/>.
        /// </summary>
        [Test(Description="Тестирование подключения к 1С по построенной строке")]
        public void TestValidBuildsConnectionString()
        {
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Properties.Settings.Default.TestCatalog;
                builder.User = "Иванов";

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.User = "Иванов";
                builder.Catalog = Properties.Settings.Default.TestCatalog;

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.ConnectionString = string.Format("File={0};Usr=Иванов", Properties.Settings.Default.TestCatalog);
                builder.Password = string.Empty;

                CheckConnect(builder);
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

                Console.WriteLine("Строка подключения к 1С : {0}", builder.ConnectionString);

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
