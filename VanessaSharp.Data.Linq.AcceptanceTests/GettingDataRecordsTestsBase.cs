﻿using System;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Базовый класс для тестирования получения данных.
    /// </summary>
    public abstract class GettingDataRecordsTestsBase : ReadDataTestBase
    {
        protected GettingDataRecordsTestsBase(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        {}

        private const string LONG_TEXT =
            @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

        private static Func<int, object> GetTypedFieldValueGetterById(params Func<int, object>[] typedFieldValueGetters)
        {
            return index => typedFieldValueGetters[index](index);
        }

        private Func<int, object> GetTypedFieldValueGetterByName(params Func<string, object>[] typedFieldValueGetters)
        {
            return index => typedFieldValueGetters[index](ExpectedFieldName(index));
        }

        /// <summary>Тестирование простого запроса.</summary>
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
                var records = dataContext.GetRecords("Справочник.ТестовыйСправочник");

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
                        var expectedFieldValue = ExpectedFieldValue(fieldIndex);
                        var fieldName = ExpectedFieldName(fieldIndex);

                        Assert.AreEqual(expectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, oneSValues[fieldIndex].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex).ToObject());
                        Assert.AreEqual(expectedFieldValue, record[fieldIndex].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldName).ToObject());
                        Assert.AreEqual(expectedFieldValue, record[fieldName].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, record[ExpectedFieldName(fieldIndex)]);
                        Assert.AreEqual(expectedFieldValue, getTypedValueById(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, getTypedValueByName(fieldIndex));
                    } 


                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);
            }
        }
    }
}