﻿using System;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тесты на linq-запросы.
    /// </summary>
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
    public sealed class ReadDataTests : ReadDataTestBase
    {
        public ReadDataTests(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
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

                .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник")

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

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField)

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

                .ExpectedSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")

                .ExpectedSqlParameter("p1", "Тестирование")

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

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField)

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

                .ExpectedSql(
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

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField)

                    .Rows(1, 0)

                .EndDefineExpectedData

             .Run();
        }

        /// <summary>
        /// Тестирование linq-запроса записей <see cref="OneSDataRecord"/>
        /// с последующей выборкой используя метод <see cref="OneSDataRecord.GetValue(string)"/>.
        /// </summary>
        [Test]
        public void TestSelectGetValueDataRecordsQuery()
        {
            Test
                .Query(dataContext =>

                              from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                              select new
                              {
                                  StringField = r.GetValue("СтроковоеПоле"),
                                  IntegerField = r.GetValue("ЦелочисленноеПоле"),
                                  NumberField = r.GetValue("ЧисловоеПоле"),
                                  BooleanField = r.GetValue("БулевоПоле"),
                                  DateField = r.GetValue("ДатаПоле"),
                                  CharField = r.GetValue("СимвольноеПоле")
                              })

                .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                {
                    Assert.AreEqual(expected.StringField, (string)actual.StringField);
                    Assert.AreEqual(expected.IntField, (int)actual.IntegerField);
                    Assert.AreEqual(expected.NumberField, (double)actual.NumberField);
                    Assert.AreEqual(expected.BooleanField, (bool)actual.BooleanField);
                    Assert.AreEqual(expected.DateField, (DateTime)actual.DateField);
                    Assert.AreEqual(expected.CharField, (char)actual.CharField);
                })

                .BeginDefineExpectedData

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.CharField)

                    .AllRows

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

                .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник")

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

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField)

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

                .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")

                .ExpectedSqlParameter("p1", "Тестирование")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.String);
                        Assert.AreEqual(expected.IntField, actual.Integer);
                        Assert.AreEqual(expected.NumberField, actual.Real);
                        Assert.AreEqual(expected.BooleanField, actual.Boolean);
                        Assert.AreEqual(expected.DateField, actual.Date);
                    })

                .BeginDefineExpectedData

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)

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

                .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник ORDER BY ЦелочисленноеПоле, ДатаПоле DESC")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.AreEqual(expected.StringField, actual.StringField);
                        Assert.AreEqual(expected.IntField, actual.IntegerField);
                        Assert.AreEqual(expected.BooleanField, actual.BooleanField);
                        Assert.AreEqual(expected.DateField, actual.DateField);
                    })

                .BeginDefineExpectedData

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)

                    .Rows(1, 0)

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>
        /// Тестовая типизированная запись с полями имеющие слаботипизированные типы,
        /// такие как <see cref="object"/> и <see cref="OneSValue"/>.
        /// </summary>
        [OneSDataSource("Справочник.ТестовыйСправочник")]
        public sealed class TestDictionaryWithWeakTyping
        {
            [OneSDataColumn("ДатаПоле")]
            public object DateField;

            [OneSDataColumn("ДатаВремяПоле")]
            public OneSValue DateTimeField;

            [OneSDataColumn("ВремяПоле")]
            public DateTime TimeField;
        }

        /// <summary>
        /// Тестирование получения типизированных записей
        /// с полями имеющие слаботипизированные типы.
        /// </summary>
        [Test]
        public void TestGetTypedRecordsWithWeakTyping()
        {
            Test
                .Query(dataContext =>
                       
                        from d in dataContext.Get<TestDictionaryWithWeakTyping>() 
                        select d)

                .ExpectedSql("SELECT ДатаПоле, ДатаВремяПоле, ВремяПоле FROM Справочник.ТестовыйСправочник")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                    {
                        Assert.IsInstanceOf<OneSValue>(actual.DateField);
                        Assert.AreEqual(expected.DateField, (DateTime)(OneSValue)actual.DateField);
                        
                        Assert.AreEqual(expected.DateTimeField, (DateTime)actual.DateTimeField);

                        Assert.AreEqual(expected.TimeField, actual.TimeField);
                    })

                .BeginDefineExpectedData

                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)

                    .AllRows

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>
        /// Тестирование выборки типизированных записей
        /// с полями имеющие слаботипизированные типы.
        /// </summary>
        [Test]
        public void TestSelectTypedRecordsWithWeakTyping()
        {
            Test
                .Query(dataContext =>

                        from d in dataContext.Get<TestDictionaryWithWeakTyping>()
                        select new { d.DateField, d.DateTimeField} )

                .ExpectedSql("SELECT ДатаПоле, ДатаВремяПоле FROM Справочник.ТестовыйСправочник")

                .AssertItem<ExpectedTestDictionary>((expected, actual) =>
                {
                    Assert.IsInstanceOf<OneSValue>(actual.DateField);
                    Assert.AreEqual(expected.DateField, (DateTime)(OneSValue)actual.DateField);

                    Assert.AreEqual(expected.DateTimeField, (DateTime)actual.DateTimeField);
                })

                .BeginDefineExpectedData

                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)

                    .AllRows

                .EndDefineExpectedData

            .Run();
        }
    }
}