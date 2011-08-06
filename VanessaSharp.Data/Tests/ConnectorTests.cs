using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace VanessaSharp.Data.Tests
{
    /// <summary>Тестмрование коннектора.</summary>
    [TestFixture]
    public sealed class ConnectorTests
    {
        /// <summary>
        /// Тестирование правильности построения строки соединения для метода 
        /// <see cref="V82.COMConnectorClass.Connect"/>.
        /// </summary>
        [Test(Description="Тестирование подключения к 1С по построенной строке")]
        public void TestValidBuildsConnectionString()
        {
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Constants.TestCatalog;
                builder.User = Constants.TestUser;

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.User = Constants.TestUser;
                builder.Catalog = Constants.TestCatalog;

                CheckConnect(builder);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.ConnectionString = string.Format("File={0};Usr={1}", Constants.TestCatalog, Constants.TestUser);
                builder.Password = string.Empty;

                CheckConnect(builder);
            }
        }

        

        /// <summary>Проверка соединения.</summary>
        /// <param name="builder">Построитель строки.</param>
        private static void CheckConnect(OneSConnectionStringBuilder builder)
        {
            var connector = new V82.COMConnectorClass();
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
