using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.Data.Linq.PredefinedData;

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
            return index => typedFieldValueGetters[index - PredefinedFieldsCount](index);
        }

        private Func<int, object> GetTypedFieldValueGetterByName(params Func<string, object>[] typedFieldValueGetters)
        {
            return index => typedFieldValueGetters[index - PredefinedFieldsCount](ExpectedFieldName(index));
        }

        private static readonly IList<Fields.Catalog> PrefinedFields = new[]
            {
                Fields.Catalog.Ref,
                Fields.Catalog.Code,
                Fields.Catalog.Description,
                Fields.Catalog.DeletionMark,
                Fields.Catalog.Presentation,
                Fields.Catalog.Predefined
            };

        private static int PredefinedFieldsCount
        {
            get { return PrefinedFields.Count; }
        }

        private int ExpectedOwnedFieldsCount
        {
            get { return base.ExpectedFieldsCount; }
        }

        protected sealed override int ExpectedFieldsCount
        {
            get { return ExpectedOwnedFieldsCount + PredefinedFieldsCount; }
        }

        private int CorrectedIndex(int fieldIndex)
        {
            var newIndex = fieldIndex - PredefinedFieldsCount;
            if (newIndex < 0)
            {
                throw new InvalidOperationException(string.Format(
                    "Ожидаемое значение для предопределенной колонки \"{0}\" неизвестно.", newIndex));
            }

            return newIndex;
        }

        protected override object ExpectedFieldValue(int fieldIndex)
        {
            return base.ExpectedFieldValue(CorrectedIndex(fieldIndex));
        }

        protected override string ExpectedFieldName(int fieldIndex)
        {
            return base.ExpectedFieldName(CorrectedIndex(fieldIndex));
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

            Assert.AreEqual(9, ExpectedOwnedFieldsCount);

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


                    for (var fieldIndex = PredefinedFieldsCount; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                    {
                        var expectedFieldValue = ExpectedFieldValue(fieldIndex);
                        var fieldName = ExpectedFieldName(fieldIndex);

                        Assert.AreEqual(expectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, oneSValues[fieldIndex].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex).ToObject());
                        Assert.AreEqual(expectedFieldValue, record[fieldIndex].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldName).ToObject());
                        Assert.AreEqual(expectedFieldValue, record[fieldName].ToObject());
                        Assert.AreEqual(expectedFieldValue, record.GetValue(fieldIndex).ToObject());
                        Assert.AreEqual(expectedFieldValue, record[ExpectedFieldName(fieldIndex)].ToObject());
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
