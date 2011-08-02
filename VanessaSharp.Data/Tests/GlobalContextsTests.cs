using System;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестирование класса <see cref="Proxies.GlobalContext"/>.</summary>
    [TestFixture]
    public sealed class GlobalContextsTests
    {
        /// <summary>Тестирование получения глобального контекста.</summary>
        [Test(Description = "Тестирование получения глобального контекста")]
        public void TestGetGlobalContext()
        {
            var parameters = new Proxies.ConnectionParameters();
            parameters.ConnectionString = string.Format("File={0};Usr=Иванов", Properties.Settings.Default.TestCatalog);
            parameters.PoolTimeout = 2000;
            parameters.PoolCapacity = 10;

            using (var globalContext = Proxies.GlobalContext.Connect(parameters))
            {
                Assert.AreEqual(2000, globalContext.PoolTimeout);
                Assert.AreEqual(10, globalContext.PoolCapacity);
            }
        }

        /// <summary>Тестирование свойства <see cref="Proxies.GlobalContext.IsExclusiveMode"/>.</summary>
        [Test(Description="Тестирование монопольного доступа")]
        public void TestIsExclusiveMode()
        {
            const string connectionStringFormat = "File={0};Usr={1}";

            var parameters = new Proxies.ConnectionParameters();
            parameters.ConnectionString = string.Format(connectionStringFormat, 
                                Properties.Settings.Default.TestCatalog, "Иванов");

            using (var globalContext = Proxies.GlobalContext.Connect(parameters))
            {
                Assert.IsFalse(globalContext.IsExclusiveMode);
                globalContext.IsExclusiveMode = true;
                Assert.IsTrue(globalContext.IsExclusiveMode);

                parameters.ConnectionString = string.Format(connectionStringFormat,
                                Properties.Settings.Default.TestCatalog, "Петров");
                ChecksHelper.AssertException<InvalidOperationException>(() => 
                {
                    var globalContext2 = Proxies.GlobalContext.Connect(parameters);
                    globalContext2.Dispose();
                });

                globalContext.IsExclusiveMode = false;
                Assert.IsFalse(globalContext.IsExclusiveMode);
                
                var globalContext3 = Proxies.GlobalContext.Connect(parameters);
                globalContext3.Dispose();
            }
        }

        /// <summary>Тестирование создания запроса.</summary>
        [Test(Description = "Тестирование создания запроса")]
        public void TestCreateQuery()
        {
            var parameters = new Proxies.ConnectionParameters();
            parameters.ConnectionString = string.Format("File={0};Usr=Иванов", Properties.Settings.Default.TestCatalog);
            using (var globalContext = Proxies.GlobalContext.Connect(parameters))
            {
                using (var query = globalContext.CreateQuery())
                {
                    Assert.IsNotNull(query);
                    var sql = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";
                    query.SetProperty("Text", sql);
                    using (var result = new Proxies.OneSObjectProxy(query.InvokeMethod("Execute")))
                    using (var selection = new Proxies.OneSObjectProxy(result.InvokeMethod("Choose")))
                    {
                        while ((bool)selection.InvokeMethod("Next"))
                        {
                            const string format = "Код \"{0}\"; Наименование \"{1}\"";
                            object code = selection.InvokeMethod("Get", 0);
                            object name = selection.InvokeMethod("Get", 1);
                            System.Diagnostics.Trace.WriteLine(string.Format(format, code, name));
                        }
                    }
                }
            }
        }
    }
}
