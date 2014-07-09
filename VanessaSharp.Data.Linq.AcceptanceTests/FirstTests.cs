using System;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
#if PROTOTYPE
    [TestFixture]
    public sealed class FirstTests
    {
        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        private static string TestConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder
                {
                    Catalog = Constants.TestCatalog,
                    User = Constants.TEST_USER
                };

                return builder.ConnectionString;
            }
        }
        
        [Test]
        [Ignore]
        public void Test()
        {
            var mapperBuilder = new OneSTypeMapperBuilder();
            mapperBuilder
                .AddSource<TestingDictionaryRecord>("Справочник.ТестовыйСправочник")
                    .AddColumn(e => e.String, "СтроковоеПоле");

            
            using (var connection = new OneSConnection(TestConnectionString))
            using (var dataContext = new OneSDataContext(connection))
            {
                var query = from e in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                            select new
                                {
                                    String = e.GetString("СтроковоеПоле"),
                                    Integer = e.GetInt32("ЦелочисленноеПоле"),
                                    Number = e.GetDouble("ЧисловоеПоле"),
                                    Boolean = e.GetBoolean("БулевоПоле"),
                                    Date = e.GetDateTime("ДатаПоле"),
                                    DateTime = e.GetDateTime("ДатаВремяПоле"),
                                    Time = e.GetDateTime("ВремяПоле"),
                                    UnboundString = e.GetString("НеограниченноеСтроковоеПоле"),
                                    Char = e.GetChar("СимвольноеПоле")
                                };

                var query2 = from e in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                             select new
                             {
                                String = (string)e["СтроковоеПоле"],
                                Integer = (int)e["ЦелочисленноеПоле"],
                                Number = (double)e["ЧисловоеПоле"],
                                Boolean = (bool)e["БулевоПоле"],
                                Date = (DateTime)e["ДатаПоле"],
                                DateTime = (DateTime)e["ДатаВремяПоле"],
                                Time = (DateTime)e["ВремяПоле"],
                                UnboundString = (string)e["НеограниченноеСтроковоеПоле"],
                                Char = (char)e["СимвольноеПоле"]
                             };

                var query5 = from e in dataContext.Get<TestingDictionaryRecord>()
                             select e;

                var query6 = from e in dataContext.Catalogs.GetRecords("ТестовыйСправочник")
                             select e;

                var query7 = from e in dataContext.Catalogs.Get<TestingRecord>()
                             select e;

            }
        }

        [OneSDataSource("Справочник.ТестовыйСправочник")]
        public sealed class TestingDictionaryRecord
        {
            [OneSDataColumn("СтроковоеПоле")]
            public string String;

            [OneSDataColumn("ЦелочисленноеПоле")]
            public int Int32;

            [OneSDataColumn("ЧисловоеПоле")]
            public double Number;

            [OneSDataColumn("БулевоПоле")]
            public bool Boolean;

            [OneSDataColumn("ДатаПоле")]
            public DateTime Date;

            [OneSDataColumn("ДатаВремяПоле")]
            public DateTime DateTime;

            [OneSDataColumn("ВремяПоле")]
            public DateTime Time;

            [OneSDataColumn("НеограниченноеСтроковоеПоле")]
            public string UnboundString;

            [OneSDataColumn("СимвольноеПоле")]
            public char Char;
        }

        public class CatalogRecordBase
        {
             
        }

        [OneSDataSource("ТестовыйСправочник")]
        public sealed class TestingRecord : CatalogRecordBase
        {
            [OneSDataColumn("СтроковоеПоле")]
            public string String;

            [OneSDataColumn("ЦелочисленноеПоле")]
            public int Int32;

            [OneSDataColumn("ЧисловоеПоле")]
            public double Number;

            [OneSDataColumn("БулевоПоле")]
            public bool Boolean;

            [OneSDataColumn("ДатаПоле")]
            public DateTime Date;

            [OneSDataColumn("ДатаВремяПоле")]
            public DateTime DateTime;

            [OneSDataColumn("ВремяПоле")]
            public DateTime Time;

            [OneSDataColumn("НеограниченноеСтроковоеПоле")]
            public string UnboundString;

            [OneSDataColumn("СимвольноеПоле")]
            public char Char;
        }
    }
#endif
}
