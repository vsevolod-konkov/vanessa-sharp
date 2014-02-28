using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
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
                .AddSource<TestingDictionaryEntry>("Справочник.ТестовыйСправочник")
                    .AddColumn(e => e.String, "СтроковоеПоле");

            
            using (var connection = new OneSConnection(TestConnectionString))
            using (var dataContext = new OneSDataContext(connection))
            {
                var query = from e in dataContext.GetEntries("Справочник.ТестовыйСправочник")
                            select new
                                {
                                    String = e.GetString("СтроковоеПоле"),
                                    Integer = e.GetInt32("ЦелочисленноеПоле"),
                                    Number = e.GetDouble("ЧисловоеПоле"),
                                    Boolean = e.GetBoolean("БулевоПоле"),
                                    Date = e.GetDateTime("ДатаПоле"),
                                    DateTime = e.GetDateTime("ДатаВремяПоле"),
                                    Time = e.GetDateTime("ВремяПоле"),
                                    UnboundStringt = e.GetString("НеограниченноеСтроковоеПоле"),
                                    Char = e.GetChar("СимвольноеПоле")
                                };

                var query2 = from e in dataContext.GetEntries("Справочник.ТестовыйСправочник")
                             select new
                             {
                                String = (string)e["СтроковоеПоле"],
                                Integer = (int)e["ЦелочисленноеПоле"],
                                Number = (double)e["ЧисловоеПоле"],
                                Boolean = (bool)e["БулевоПоле"],
                                Date = (DateTime)e["ДатаПоле"],
                                DateTime = (DateTime)e["ДатаВремяПоле"],
                                Time = (DateTime)e["ВремяПоле"],
                                UnboundStringt = (string)e["НеограниченноеСтроковоеПоле"],
                                Char = (char)e["СимвольноеПоле"]
                             };

                //var query3 = from e in dataContext.GetEntries("Справочник.ТестовыйСправочник")
                //             let d = e.AsDynamic()
                //             select new
                //             {
                //                 String = (string)d.СтроковоеПоле,
                //                 Integer = (int)d.ЦелочисленноеПоле,
                //                 Number = (double)d.ЧисловоеПоле,
                //                 Boolean = (bool)d.БулевоПоле,
                //                 Date = (DateTime)d.ДатаПоле,
                //                 DateTime = (DateTime)d.ДатаВремяПоле,
                //                 Time = (DateTime)d.ВремяПоле,
                //                 UnboundStringt = (string)d.НеограниченноеСтроковоеПоле,
                //                 Char = (char)d.СимвольноеПоле
                //             };

                //var query4 = from d in dataContext.GetDynamicEntries("Справочник.ТестовыйСправочник")
                //             select new
                //             {
                //                 String = (string)d.СтроковоеПоле,
                //                 Integer = (int)d.ЦелочисленноеПоле,
                //                 Number = (double)d.ЧисловоеПоле,
                //                 Boolean = (bool)d.БулевоПоле,
                //                 Date = (DateTime)d.ДатаПоле,
                //                 DateTime = (DateTime)d.ДатаВремяПоле,
                //                 Time = (DateTime)d.ВремяПоле,
                //                 UnboundStringt = (string)d.НеограниченноеСтроковоеПоле,
                //                 Char = (char)d.СимвольноеПоле
                //             };

                var query5 = from e in dataContext.Get<TestingDictionaryEntry>()
                             select e;

                var query6 = from e in dataContext.Catalogs.GetEntries("ТестовыйСправочник")
                             select e;

                var query7 = from e in dataContext.Catalogs.Get<TestingEntry>()
                             select e;

            }
        }

        [OneSDataSource("Справочник.ТестовыйСправочник")]
        public sealed class TestingDictionaryEntry
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

        public class CatalogEntryBase
        {
             
        }

        [OneSDataSource("ТестовыйСправочник")]
        public sealed class TestingEntry : CatalogEntryBase
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

    
}
