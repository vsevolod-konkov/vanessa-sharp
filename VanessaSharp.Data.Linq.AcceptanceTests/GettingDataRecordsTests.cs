using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.Data.Linq.PredefinedData;

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
    public sealed class GettingDataRecordsTests : ReadDataTestBase
    {
        public GettingDataRecordsTests(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        {}

        private const string LONG_TEXT =
            @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

        private static Func<int, object> GetTypedFieldValueGetterById(params Func<int, object>[] typedFieldValueGetters)
        {
            return index =>
                {
                    var getter = typedFieldValueGetters[index];
                    if (getter == null)
                        throw new ArgumentOutOfRangeException("index");

                    return getter(index);
                };
        }

        private Func<int, object> GetTypedFieldValueGetterByName(params Func<string, object>[] typedFieldValueGetters)
        {
            return index =>
                {
                    var getter = typedFieldValueGetters[index];
                    if (getter == null)
                        throw new ArgumentOutOfRangeException("index");

                    return getter(ExpectedFieldName(index));
                };
        }

        private void PredefinedField(Fields.Catalog field)
        {
            Field<AnyType>(field.GetLocalizedName());
        }

        /// <summary>Тестирование запроса получение целой записи.</summary>
        [Test]
        public void TestFillRecordQuery()
        {
            BeginDefineData();

            PredefinedField(Fields.Catalog.Ref);
            PredefinedField(Fields.Catalog.Code);
            PredefinedField(Fields.Catalog.Description);
            PredefinedField(Fields.Catalog.DeletionMark);
            PredefinedField(Fields.Catalog.Presentation);
            PredefinedField(Fields.Catalog.Predefined);

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
                AnyType.Instance, AnyType.Instance, AnyType.Instance,
                AnyType.Instance, AnyType.Instance, AnyType.Instance,
                "Тестирование", 234, 546.323, true,
                new DateTime(2014, 01, 15), new DateTime(2014, 01, 08, 4, 33, 43),
                new DateTime(100, 1, 1, 23, 43, 43), LONG_TEXT, "А"
            );

            Row
            (
                AnyType.Instance, AnyType.Instance, AnyType.Instance,
                AnyType.Instance, AnyType.Instance, AnyType.Instance,
                "", 0, 0, false,
                new DateTime(100, 1, 1), new DateTime(100, 1, 1),
                new DateTime(100, 1, 1),
                "", " "
            );

            EndDefineData();

            Assert.AreEqual(15, ExpectedFieldsCount);

            using (var dataContext = new OneSDataContext(Connection))
            {
                var records = from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                              select r;

                var recordCounter = 0;
                
                foreach (var record in records)
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);
                    Assert.AreEqual(ExpectedFieldsCount, record.Fields.Count);

                    SetCurrentExpectedRow(recordCounter);

                    var values = new object[ExpectedFieldsCount];
                    Assert.AreEqual(ExpectedFieldsCount, record.GetValues(values));

                    var oneSValues = new OneSValue[ExpectedFieldsCount];
                    Assert.AreEqual(ExpectedFieldsCount, record.GetValues(oneSValues));

                    Func<int, object> getStringValueById = index => record.GetString(index);
                    Func<int, object> getDateTimeValueById = index => record.GetDateTime(index);

                    Func<string, object> getStringValueByName = name => record.GetString(name);
                    Func<string, object> getDateTimeValueByName = name => record.GetDateTime(name);

                    var getTypedValueById = GetTypedFieldValueGetterById(
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            getStringValueById,
                                            i => record.GetInt32(i),
                                            i => record.GetDouble(i),
                                            i => record.GetBoolean(i),
                                            getDateTimeValueById,
                                            getDateTimeValueById,
                                            getDateTimeValueById,
                                            getStringValueById,
                                            i => record.GetChar(i).ToString()
                                            );

                    var getTypedValueByName = GetTypedFieldValueGetterByName(
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            getStringValueByName,
                                            s => record.GetInt32(s),
                                            s => record.GetDouble(s),
                                            s => record.GetBoolean(s),
                                            getDateTimeValueByName,
                                            getDateTimeValueByName,
                                            getDateTimeValueByName,
                                            getStringValueByName,
                                            s => record.GetChar(s).ToString()
                                            );


                    for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                    {
                        if (ExpectedFieldType(fieldIndex) == typeof(AnyType))
                            continue;

                        var expectedFieldValue = ExpectedFieldValue(fieldIndex);
                        var fieldName = ExpectedFieldName(fieldIndex);

                        Assert.AreEqual(expectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, oneSValues[fieldIndex].RawValue);
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex).RawValue);
                        Assert.AreEqual(expectedFieldValue, record[fieldIndex].RawValue);
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldName).RawValue);
                        Assert.AreEqual(expectedFieldValue, record[fieldName].RawValue);
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex).RawValue);
                        Assert.AreEqual(expectedFieldValue, record[ExpectedFieldName(fieldIndex)].RawValue);
                        Assert.AreEqual(expectedFieldValue, getTypedValueById(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, getTypedValueByName(fieldIndex));
                    } 

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);

                AssertSql("SELECT * FROM Справочник.ТестовыйСправочник");
                AssertSqlParameters(new Dictionary<string, object>());
            }
        }

        /// <summary>Тестирование простого запроса с выборкой данных из записи.</summary>
        [Test]
        public void TestSelectSimpleQuery()
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
                var entries = from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
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
                              };

                var recordCounter = 0;

                foreach (var entry in entries)
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    SetCurrentExpectedRow(recordCounter);

                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.String);
                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.Integer);
                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.Number);
                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.Boolean);
                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.Date);
                    Assert.AreEqual(ExpectedFieldValue("ДатаВремяПоле"), entry.DateTime);
                    Assert.AreEqual(ExpectedFieldValue("ВремяПоле"), entry.Time);
                    Assert.AreEqual(ExpectedFieldValue("НеограниченноеСтроковоеПоле"), entry.UnboundString);
                    Assert.AreEqual(ExpectedFieldValue("СимвольноеПоле"), entry.Char.ToString());

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);

                AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник");
                AssertSqlParameters(new Dictionary<string, object>());
            }
        }

        /// <summary>Тестирование запроса с выборкой и фильтрацией данных из записи.</summary>
        [Test]
        public void TestSelectAndWhereQuery()
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

            EndDefineData();

            Assert.AreEqual(9, ExpectedFieldsCount);

            using (var dataContext = new OneSDataContext(Connection))
            {
                var entries = from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
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
                              };

                var recordCounter = 0;

                foreach (var entry in entries)
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    SetCurrentExpectedRow(recordCounter);

                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.String);
                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.Integer);
                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.Number);
                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.Boolean);
                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.Date);
                    Assert.AreEqual(ExpectedFieldValue("ДатаВремяПоле"), entry.DateTime);
                    Assert.AreEqual(ExpectedFieldValue("ВремяПоле"), entry.Time);
                    Assert.AreEqual(ExpectedFieldValue("НеограниченноеСтроковоеПоле"), entry.UnboundString);
                    Assert.AreEqual(ExpectedFieldValue("СимвольноеПоле"), entry.Char.ToString());

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);

                AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1");
                AssertSqlParameters(new Dictionary<string, object> { { "p1", "Тестирование" } });
            }
        }

        /// <summary>Тестирование запроса с выборкой и сортировкой данных.</summary>
        [Test]
        public void TestSelectAndOrderByQuery()
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
                "", 0, 0, false,
                new DateTime(100, 1, 1), new DateTime(100, 1, 1),
                new DateTime(100, 1, 1),
                "", " "
            );

            Row
            (
                "Тестирование", 234, 546.323, true,
                new DateTime(2014, 01, 15), new DateTime(2014, 01, 08, 4, 33, 43),
                new DateTime(100, 1, 1, 23, 43, 43), LONG_TEXT, "А"
            );

            EndDefineData();

            Assert.AreEqual(9, ExpectedFieldsCount);

            using (var dataContext = new OneSDataContext(Connection))
            {
                var entries = from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                              orderby r.GetInt32("ЦелочисленноеПоле"), r.GetString("ДатаПоле") descending, r.GetString("СтроковоеПоле")
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
                              };

                var recordCounter = 0;

                foreach (var entry in entries)
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    SetCurrentExpectedRow(recordCounter);

                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.String);
                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.Integer);
                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.Number);
                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.Boolean);
                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.Date);
                    Assert.AreEqual(ExpectedFieldValue("ДатаВремяПоле"), entry.DateTime);
                    Assert.AreEqual(ExpectedFieldValue("ВремяПоле"), entry.Time);
                    Assert.AreEqual(ExpectedFieldValue("НеограниченноеСтроковоеПоле"), entry.UnboundString);
                    Assert.AreEqual(ExpectedFieldValue("СимвольноеПоле"), entry.Char.ToString());

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);

                AssertSql(
                    "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле, ДатаВремяПоле, ВремяПоле, НеограниченноеСтроковоеПоле, СимвольноеПоле FROM Справочник.ТестовыйСправочник ORDER BY ЦелочисленноеПоле, ДатаПоле DESC, СтроковоеПоле");
                AssertSqlParameters(new Dictionary<string, object>());
            }
        }
    }
}
