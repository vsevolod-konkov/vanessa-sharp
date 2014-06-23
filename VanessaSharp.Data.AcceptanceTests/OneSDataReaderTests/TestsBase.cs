using System;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Базовый класс приемочных тестов на <see cref="OneSDataReader"/>.</summary>
    public abstract class TestsBase : ReadDataTestBase
    {
        private const string LONG_TEXT =
            @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        

        protected TestsBase(TestMode testMode) : base(testMode)
        {}

        protected OneSCommand GetCommand(string sql)
        {
            return new OneSCommand
            {
                Connection = Connection,
                CommandType = CommandType.Text,
                CommandText = sql
            };
        }

        private OneSDataReader GetTestedReader(string sourceName, CommandBehavior behavior)
        {
            var command = GetCommand(GetSql(sourceName));

            command.Prepare();

            return command.ExecuteReader(behavior);
        }

        private string GetSql(string sourceName)
        {
            var fieldNames = from field in ExpectedData.Fields
                             select field.Name;

            var queryStringBuilder = new StringBuilder("Выбрать ");
            queryStringBuilder.Append(fieldNames.First());
            foreach (var field in fieldNames.Skip(1))
            {
                queryStringBuilder.Append(", ");
                queryStringBuilder.Append(field);
            }
            queryStringBuilder.Append(" ИЗ ");
            queryStringBuilder.Append(sourceName);

            return queryStringBuilder.ToString();
        }

        private static Func<int, object> GetTypedFieldValueGetter(params Func<int, object>[] typedFieldValueGetters)
        {
            return index => typedFieldValueGetters[index](index);
        }

        /// <summary>Обработка после выполнения команды.</summary>
        /// <param name="command">Команда</param>
        protected virtual void OnAfterExecute(OneSCommand command)
        {}

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

            using (var reader = GetTestedReader("Справочник.ТестовыйСправочник",
                CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
            {
                Assert.AreEqual(0, reader.Depth);
                Assert.IsTrue(reader.HasRows);
                Assert.IsFalse(reader.IsClosed);
                Assert.AreEqual(-1, reader.RecordsAffected);


                Assert.AreEqual(ExpectedFieldsCount, reader.FieldCount);
                Assert.AreEqual(ExpectedFieldsCount, reader.VisibleFieldCount);

                for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                {
                    Assert.AreEqual(ExpectedFieldName(fieldIndex), reader.GetName(fieldIndex));
                    Assert.AreEqual(ExpectedFieldType(fieldIndex), reader.GetFieldType(fieldIndex));
                    Assert.AreEqual(fieldIndex, reader.GetOrdinal(ExpectedFieldName(fieldIndex)));
                }

                var values = new object[ExpectedFieldsCount];

                var recordCounter = 0;

                Func<int, object> getStringValue = index => reader.GetString(index);
                Func<int, object> getDateTimeValue = index => reader.GetDateTime(index);

                var getTypedValue = GetTypedFieldValueGetter(
                                        getStringValue,    
                                        i => reader.GetInt32(i),
                                        i => reader.GetDouble(i),
                                        i => reader.GetBoolean(i),
                                        getDateTimeValue,
                                        getDateTimeValue,
                                        getDateTimeValue,
                                        getStringValue,
                                        i => reader.GetChar(i).ToString()
                                        );

                while (reader.Read())
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    Assert.AreEqual(ExpectedFieldsCount, reader.GetValues(values));

                    SetCurrentExpectedRow(recordCounter);

                    for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                    {
                        var expectedFieldValue = ExpectedFieldValue(fieldIndex);

                        Assert.AreEqual(expectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, reader[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, reader.GetValue(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, reader[ExpectedFieldName(fieldIndex)]);
                        Assert.AreEqual(expectedFieldValue, getTypedValue(fieldIndex));
                    }

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);
            }
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetSchemaTable"/>
        /// и <see cref="OneSDataReader.GetDataTypeName"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetSchema()
        {
            BeginDefineData();

            Field<string>("СтроковоеПоле");
            Field<double>("ЦелочисленноеПоле");
            
            EndDefineData();

            using (var reader = GetTestedReader("Справочник.ТестовыйСправочник", CommandBehavior.Default))
            {
                Assert.Throws<NotImplementedException>(() =>
                    {
                        var schemaTable = reader.GetSchemaTable();
                    });

                for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                {
                    Assert.Throws<NotImplementedException>(() =>
                    {
                        var typeName = reader.GetDataTypeName(fieldIndex);
                    });    
                }
            }
        }

        /// <summary>
        /// Тестирование 
        /// методов потокового чтения:
        /// <see cref="OneSDataReader.GetBytes"/>
        /// и <see cref="OneSDataReader.GetChars"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestStreamReading()
        {
            BeginDefineData();
            
            Field<string>("НеограниченноеСтроковоеПоле");
            Row(LONG_TEXT);

            EndDefineData();

            using (var reader = GetTestedReader("Справочник.ТестовыйСправочник", CommandBehavior.Default))
            {
                Assert.IsTrue(reader.Read());
                SetCurrentExpectedRow(0);

                const int BUFFER_SIZE = 1024;
                
                var binaryBuffer = new byte[BUFFER_SIZE];
                Assert.Throws<NotImplementedException>(() => reader.GetBytes(0, 0, binaryBuffer, 0, BUFFER_SIZE));

                var charBuffer = new char[BUFFER_SIZE];
                Assert.Throws<NotImplementedException>(() => reader.GetChars(0, 0, charBuffer, 0, BUFFER_SIZE));
            }
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetGuid"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetGuid()
        {
            BeginDefineData();

            Field<Guid>("UID");
            Row(Guid.Parse("8e12149f-5b71-4218-a1cd-429d3d1cfe68"));

            EndDefineData();

            using (var reader = GetTestedReader("Справочник.СправочникUID", CommandBehavior.Default))
            {
                Assert.IsTrue(reader.Read());
                SetCurrentExpectedRow(0);

                Assert.Throws<NotImplementedException>(() => { var value = reader.GetGuid(0); });
            }
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetEnumerator"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetEnumerator()
        {
            BeginDefineData();
            EndDefineData();
            
            using (var reader = GetCommand("ВЫБРАТЬ * ИЗ Справочник.ТестовыйСправочник").ExecuteReader())
            {
                Assert.Throws<NotImplementedException>(() => { var enumerator = reader.GetEnumerator(); });
            }
        }

        /// <summary>
        /// Тестирование выполнения параметрического запроса.
        /// </summary>
        [Test]
        public void TestSimpleParameterizedQuery()
        {
            BeginDefineData();

            Field<string>("СтроковоеПоле");
            Field<double>("ЦелочисленноеПоле");

            Row("Тестирование", 234);

            EndDefineData();

            var command =
                GetCommand(
                    "ВЫБРАТЬ СтроковоеПоле, ЦелочисленноеПоле ИЗ Справочник.ТестовыйСправочник ГДЕ СтроковоеПоле = &Фильтр");

            command.Parameters.Add("Фильтр", "Тестирование");

            command.Prepare();

            using (var reader = command.ExecuteReader())
            {
                Assert.AreEqual(ExpectedFieldsCount, reader.FieldCount);
                Assert.AreEqual(ExpectedFieldsCount, reader.VisibleFieldCount);

                for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                {
                    Assert.AreEqual(ExpectedFieldName(fieldIndex), reader.GetName(fieldIndex));
                    Assert.AreEqual(ExpectedFieldType(fieldIndex), reader.GetFieldType(fieldIndex));
                    Assert.AreEqual(fieldIndex, reader.GetOrdinal(ExpectedFieldName(fieldIndex)));
                }

                var values = new object[ExpectedFieldsCount];

                var recordCounter = 0;

                Func<int, object> getStringValue = index => reader.GetString(index);
                Func<int, object> getDateTimeValue = index => reader.GetDateTime(index);

                var getTypedValue = GetTypedFieldValueGetter(
                                        getStringValue,
                                        i => reader.GetInt32(i),
                                        i => reader.GetDouble(i),
                                        i => reader.GetBoolean(i),
                                        getDateTimeValue,
                                        getDateTimeValue,
                                        getDateTimeValue,
                                        getStringValue,
                                        i => reader.GetChar(i).ToString()
                                        );

                while (reader.Read())
                {
                    Assert.Less(recordCounter, ExpectedRowsCount);

                    Assert.AreEqual(ExpectedFieldsCount, reader.GetValues(values));

                    SetCurrentExpectedRow(recordCounter);

                    for (var fieldIndex = 0; fieldIndex < ExpectedFieldsCount; fieldIndex++)
                    {
                        var expectedFieldValue = ExpectedFieldValue(fieldIndex);

                        Assert.AreEqual(expectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, reader[fieldIndex]);
                        Assert.AreEqual(expectedFieldValue, reader.GetValue(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, reader[ExpectedFieldName(fieldIndex)]);
                        Assert.AreEqual(expectedFieldValue, getTypedValue(fieldIndex));
                    }

                    ++recordCounter;
                }

                Assert.AreEqual(ExpectedRowsCount, recordCounter);
            }
        }
    }
}
