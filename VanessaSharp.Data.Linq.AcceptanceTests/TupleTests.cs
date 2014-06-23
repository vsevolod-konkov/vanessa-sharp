using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тестирование получения данных.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public sealed class TupleTests : ReadDataTestBase
    {
        public TupleTests(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {}

        // TODO Копипаст
        private const string LONG_TEXT =
            @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

        // TODO Копипаст GettingDataRecordsTests.TestSelectSimpleQuery
        /// <summary>Тестирование выполнения простого запроса.</summary>
        [Test]
        public void TestSimpleQuery()
        {
            BeginDefineData();

            Field<string>("СтроковоеПоле");
            Field<double>("ЦелочисленноеПоле");
            Field<double>("ЧисловоеПоле");
            Field<bool>("БулевоПоле");
            Field<DateTime>("ДатаПоле");
            Field<DateTime>("ДатаВремяПоле");
            Field<DateTime>("ВремяПоле");
            Field<string>("НеограниченноеСтроковоеПоле");
            Field<string>("СимвольноеПоле");

            Row
            (
                "Тестирование", 234, 546.323, true,
                new DateTime(2014, 01, 15), new DateTime(2014, 01, 08, 4, 33, 43),
                new DateTime(100, 1, 1, 23, 43, 43), LONG_TEXT, "А"
            );

            Row
            (
                "", 0, 0, false,
                new DateTime(100, 1, 1), new DateTime(100, 1, 1),
                new DateTime(100, 1, 1),
                "", " "
            );

            EndDefineData();

            Assert.AreEqual(9, ExpectedFieldsCount);

            using (var dataContext = new OneSDataContext(Connection))
            {
                var entries = from e in dataContext.Get<TestDictionary>()
                              select e;

                var recordCounter = 0;

                foreach (var entry in entries)
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    SetCurrentExpectedRow(recordCounter);

                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.StringField);
                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.IntegerField);
                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.RealField);
                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.BooleanField);
                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.DateField);
                    Assert.AreEqual(ExpectedFieldValue("ДатаВремяПоле"), entry.DateTimeField);
                    Assert.AreEqual(ExpectedFieldValue("ВремяПоле"), entry.TimeField);
                    Assert.AreEqual(ExpectedFieldValue("НеограниченноеСтроковоеПоле"), entry.UnboundedStringField);
                    Assert.AreEqual(ExpectedFieldValue("СимвольноеПоле"), entry.CharField.ToString());

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);

                AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник");
                AssertSqlParameters(new Dictionary<string, object>());
            }
        }
    }

    /// <summary>Класс тестового справочника.</summary>
    [OneSDataSource("Справочник.ТестовыйСправочник")]
    public sealed class TestDictionary
    {
        [OneSDataColumn("СтроковоеПоле")]
        public string StringField;

        [OneSDataColumn("ЦелочисленноеПоле")]
        public int IntegerField;
            
        [OneSDataColumn("ЧисловоеПоле")]
        public double RealField;

        [OneSDataColumn("БулевоПоле")]
        public bool BooleanField;

        [OneSDataColumn("ДатаПоле")] 
        public DateTime DateField;

        [OneSDataColumn("ДатаВремяПоле")]
        public DateTime DateTimeField;

        [OneSDataColumn("ВремяПоле")]
        public DateTime TimeField;

        [OneSDataColumn("НеограниченноеСтроковоеПоле")] 
        public string UnboundedStringField;

        [OneSDataColumn("СимвольноеПоле")]
        public char CharField;
    }
}
