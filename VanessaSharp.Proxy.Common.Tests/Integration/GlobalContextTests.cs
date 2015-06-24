using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests.Integration
{
    /// <summary>Интеграционные тесты.</summary>
    [TestFixture]
    public sealed class GlobalContextTests
    {
        /// <summary>Соединитель с 1С.</summary>
        private IOneSConnector _connector;

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            _connector = OneSConnectorFactory.Default.Create(null);
        }

        /// <summary>Очистка ресурсов теста.</summary>
        [TearDown]
        public void TearDown()
        {
            _connector.Dispose();
        }

        /// <summary>
        /// Тестирование методов 
        /// <see cref="OneSGlobalContext.ExclusiveMode"/> 
        /// и <see cref="OneSGlobalContext.SetExclusiveMode"/>.
        /// </summary>
        [Test(Description = "Тестирование монопольного доступа")]
        [Ignore("На учебной БД нельзя протестировать многопользовательский режим")]
        public void TestIsExclusiveMode()
        {
            const string CONNECTION_STRING_FORMAT = "File=\"{0}\";Usr=\"{1}\"";

            var connectString = string.Format(CONNECTION_STRING_FORMAT, Constants.TestCatalog, Constants.TestUser);

            using (var globalContext = _connector.Connect(connectString))
            {
                Assert.IsFalse(globalContext.ExclusiveMode());
                globalContext.SetExclusiveMode(true);
                Assert.IsTrue(globalContext.ExclusiveMode());

                var alternativeConnectString = string.Format(CONNECTION_STRING_FORMAT, Constants.TestCatalog, Constants.AlternativeTestUser);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using(var globalContext2 = _connector.Connect(alternativeConnectString)) {}
                });

                globalContext.SetExclusiveMode(false);
                Assert.IsFalse(globalContext.ExclusiveMode());

                using (var globalContext3 = _connector.Connect(alternativeConnectString)) {}
            }
        }

        /// <summary>Тестирование создания запроса.</summary>
        [Test(Description = "Тестирование создания запроса")]
        public void TestCreateQuery()
        {
             var connectString = string.Format("File=\"{0}\";Usr=\"{1}\"", Constants.TestCatalog, Constants.TestUser);
            
            using (var globalContext = _connector.Connect(connectString))
            {
                using (var query = globalContext.NewObject("Query"))
                {
                    Assert.IsNotNull(query);
                    var sql = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";
                    query.Text = sql;
                    using (var result = query.Execute())
                    using (var selection = result.Choose())
                    {
                        while ((bool)selection.Next())
                        {
                            const string format = "Код \"{0}\"; Наименование \"{1}\"";
                            object code = selection.Get(0);
                            object name = selection.Get(1);
                            Trace.WriteLine(string.Format(format, code, name));
                        }
                    }
                }
            }
        }

        /// <summary>Тестирование создания запроса.</summary>
        [Test(Description = "Тестирование создания запроса")]
        public void TestCreateQuery2()
        {
            var connectString = string.Format("File=\"{0}\";Usr=\"{1}\"", Constants.TestCatalog, Constants.TestUser);

            using (var globalContextProxy = _connector.Connect(connectString))
            {
                IOneSProxy queryProxy = globalContextProxy.NewObject("Query");
                dynamic queryCom = queryProxy.Unwrap();

                Assert.IsNotNull(queryCom);
                const string SQL = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";
                queryCom.Text = SQL;
                dynamic result = queryCom.Execute();
                try
                {
                    dynamic globalContext = ((IOneSProxy)globalContextProxy).Unwrap();
                    Assert.IsNotNull(globalContext);

                    dynamic queryResultIteration = globalContext.QueryResultIteration;
                    try
                    {
                        dynamic byGroups = queryResultIteration.ByGroups;
                        try
                        {
                            dynamic selection = result.Choose(byGroups);
                            try
                            {
                                while ((bool)selection.Next())
                                {
                                    const string FORMAT = "Код \"{0}\"; Наименование \"{1}\"";

                                    object code = selection.Get(0);
                                    object name = selection.Get(1);
                                    Trace.WriteLine(string.Format(FORMAT, code, name));
                                }
                            }
                            finally
                            {
                                Marshal.FinalReleaseComObject(selection);
                            }
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(byGroups);
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(queryResultIteration);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(result);
                }
            }
        }
    }
}
