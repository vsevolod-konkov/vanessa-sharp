using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тесты на linq-запросы.
    /// </summary>
    [TestFixture]
    public sealed class LinqReadDataTests : LinqReadDataTestBase
    {
        public LinqReadDataTests()
            : base(TestMode.Real, true)
        {
        }

        /// <summary>
        /// Тестирование linq-запроса записей <see cref="OneSDataRecord"/>
        /// с последующей выборкой.
        /// </summary>
        [Test]
        public void TestSelectDataRecordsQuery()
        {
            Test
                .Query(dataContext => 
                              
                              from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                              select new
                              {
                                  String = r.GetString("СтроковоеПоле"),
                                  Integer = r.GetInt32("ЦелочисленноеПоле"),
                                  Number = r.GetDouble("ЧисловоеПоле"),
                                  Boolean = r.GetBoolean("БулевоПоле"),
                                  Date = r.GetDateTime("ДатаПоле"),
                                  DateTime = r.GetDateTime("ДатаВремяПоле"),
                                  Time = r.GetDateTime("ВремяПоле"),
                                  UnboundString = r.GetString("НеограниченноеСтроковоеПоле"),
                                  Char = r.GetChar("СимвольноеПоле")
                              })

                .AssertSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.String);
                        Assert.AreEqual(expected.IntField, actual.Integer);
                        Assert.AreEqual(expected.NumberField, actual.Number);
                        Assert.AreEqual(expected.BooleanField, actual.Boolean);
                        Assert.AreEqual(expected.DateField, actual.Date);
                        Assert.AreEqual(expected.DateTimeField, actual.DateTime);
                        Assert.AreEqual(expected.TimeField, actual.Time);
                        Assert.AreEqual(expected.UndoundStringField, actual.UnboundString);
                        Assert.AreEqual(expected.CharField, actual.Char);
                    })

                .BeginDefineExpectedData

                    .AllRows

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>Тестирование запроса с выборкой и фильтрацией записей <see cref="OneSDataRecord"/>.</summary>
        [Test]
        public void TestSelectAndWhereDataRecordsQuery()
        {
            Test
                .Query(dataContext => 
                              from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")

                              where r.GetString("СтроковоеПоле") == "Тестирование"
                              
                              select new
                              {
                                  String = r.GetString("СтроковоеПоле"),
                                  Integer = r.GetInt32("ЦелочисленноеПоле"),
                                  Number = r.GetDouble("ЧисловоеПоле"),
                                  Boolean = r.GetBoolean("БулевоПоле"),
                                  Date = r.GetDateTime("ДатаПоле"),
                                  DateTime = r.GetDateTime("ДатаВремяПоле"),
                                  Time = r.GetDateTime("ВремяПоле"),
                                  UnboundString = r.GetString("НеограниченноеСтроковоеПоле"),
                                  Char = r.GetChar("СимвольноеПоле")
                              }
                )

                .AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")

                .AssertSqlParameter("p1", "Тестирование")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.String);
                        Assert.AreEqual(expected.IntField, actual.Integer);
                        Assert.AreEqual(expected.NumberField, actual.Number);
                        Assert.AreEqual(expected.BooleanField, actual.Boolean);
                        Assert.AreEqual(expected.DateField, actual.Date);
                        Assert.AreEqual(expected.DateTimeField, actual.DateTime);
                        Assert.AreEqual(expected.TimeField, actual.Time);
                        Assert.AreEqual(expected.UndoundStringField, actual.UnboundString);
                        Assert.AreEqual(expected.CharField, actual.Char);
                    })

                .BeginDefineExpectedData

                .Rows(0)

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>
        /// Тестирование запроса с выборкой и сортировкой записей <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestSelectAndOrderByDataRecordsQuery()
        {
            Test
                .Query(dataContext =>
                       from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")

                       orderby r.GetInt32("ЦелочисленноеПоле"), r.GetString("ДатаПоле") descending,
                               r.GetString("СтроковоеПоле")

                       select new
                           {
                               String = r.GetString("СтроковоеПоле"),
                               Integer = r.GetInt32("ЦелочисленноеПоле"),
                               Number = r.GetDouble("ЧисловоеПоле"),
                               Boolean = r.GetBoolean("БулевоПоле"),
                               Date = r.GetDateTime("ДатаПоле"),
                               DateTime = r.GetDateTime("ДатаВремяПоле"),
                               Time = r.GetDateTime("ВремяПоле"),
                               UnboundString = r.GetString("НеограниченноеСтроковоеПоле"),
                               Char = r.GetChar("СимвольноеПоле")
                           })
                .AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле"
                    + " FROM Справочник.ТестовыйСправочник ORDER BY ЦелочисленноеПоле, ДатаПоле DESC, СтроковоеПоле")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.String);
                        Assert.AreEqual(expected.IntField, actual.Integer);
                        Assert.AreEqual(expected.NumberField, actual.Number);
                        Assert.AreEqual(expected.BooleanField, actual.Boolean);
                        Assert.AreEqual(expected.DateField, actual.Date);
                        Assert.AreEqual(expected.DateTimeField, actual.DateTime);
                        Assert.AreEqual(expected.TimeField, actual.Time);
                        Assert.AreEqual(expected.UndoundStringField, actual.UnboundString);
                        Assert.AreEqual(expected.CharField, actual.Char);
                    })

                .BeginDefineExpectedData

                .Rows(1, 0)

                .EndDefineExpectedData

             .Run();
        }

        /// <summary>Тестирование выполнения запроса получения типизированных записей.</summary>
        [Test]
        public void TestGetTypedRecordsQuery()
        {
            Test
                .Query(dataContext =>
                    from e in dataContext.Get<TestDictionary>()
                    select e
                )

                .AssertSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.StringField);
                        Assert.AreEqual(expected.IntField, actual.IntegerField);
                        Assert.AreEqual(expected.NumberField, actual.RealField);
                        Assert.AreEqual(expected.BooleanField, actual.BooleanField);
                        Assert.AreEqual(expected.DateField, actual.DateField);
                        Assert.AreEqual(expected.DateTimeField, actual.DateTimeField);
                        Assert.AreEqual(expected.TimeField, actual.TimeField);
                        Assert.AreEqual(expected.UndoundStringField, actual.UnboundedStringField);
                        Assert.AreEqual(expected.CharField, actual.CharField);
                    })

                .BeginDefineExpectedData

                    .AllRows

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>Тестирование запроса с фильтрацией типизированных записей.</summary>
        [Test]
        public void TestFilteringAndSelectTypedRecordsQuery()
        {
            Test
                .Query(dataContext => 
                        from e in dataContext.Get<TestDictionary>()
                        
                        where e.StringField == "Тестирование"
                              
                        select new
                              {
                                  String = e.StringField,
                                  Integer = e.IntegerField,
                                  Real = e.RealField,
                                  Boolean = e.BooleanField,
                                  Date = e.DateField
                              }
                )

                .AssertSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")

                .AssertSqlParameter("p1", "Тестирование")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.String);
                        Assert.AreEqual(expected.IntField, actual.Integer);
                        Assert.AreEqual(expected.NumberField, actual.Real);
                        Assert.AreEqual(expected.BooleanField, actual.Boolean);
                        Assert.AreEqual(expected.DateField, actual.Date);
                    })

                .BeginDefineExpectedData
                    .Rows(0)
                .EndDefineExpectedData

            .Run();
        }

        /// <summary>Тестирование выполнения запроса сортировки и выборки типизированных записей.</summary>
        [Test]
        public void TestSortingAndSelectQuery()
        {
            Test
                .Query(dataContext => 
                                        from e in dataContext.Get<TestDictionary>()
                                        
                                        orderby e.IntegerField, e.DateField descending
                                        
                                        select new { e.StringField, e.IntegerField, e.BooleanField, e.DateField })

                .AssertSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник ORDER BY ЦелочисленноеПоле, ДатаПоле DESC")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.StringField);
                        Assert.AreEqual(expected.IntField, actual.IntegerField);
                        Assert.AreEqual(expected.BooleanField, actual.BooleanField);
                        Assert.AreEqual(expected.DateField, actual.DateField);
                    })

                .BeginDefineExpectedData

                    .Rows(1, 0)

                .EndDefineExpectedData

            .Run();
        }
    }
}
