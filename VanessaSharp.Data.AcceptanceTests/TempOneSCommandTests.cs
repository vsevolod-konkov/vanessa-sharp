using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    // TODO: Требуется написать нормальные приемочные тесты
    /// <summary>Приемочные тесты на <see cref="OneSCommand"/>.</summary>
    [TestFixture]
    public sealed class TempOneSCommandTests
    {
        /// <summary>Тестирование простого запроса.</summary>
        [Test]
        public void TestSimpleExecuteReader()
        {
            const int FIELD_COUNT = 6;
            
            var expectedRecords = new[]
                {
                    new[]
                        {
                            "852500102", "АКБ \"АВТ-БАНК\"", "10805202301222054654", "Г.МОСКВА",
                            "101511 Г.МОСКВА УЛ.ЛУГОВАЯ,41", "112-90-08"
                        },
                    new[]
                        {
                            "852304412", "АКБ \"ТОРГБАНК\"", "40712300045600065502", "Г.МОСКВА",
                            "109094 Г.МОСКВА ГЛУБИНЫЙ ПЕР.,2", "223-322"
                        }
                };

            var fieldNames = new[]
                {
                    "Код",
                    "Наименование",
                    "КоррСчет",
                    "Город",
                    "Адрес",
                    "Телефоны"
                };

            Assert.AreEqual(FIELD_COUNT, fieldNames.Length);

            var queryStringBuilder = new StringBuilder("Выбрать ");
            for (var fieldIndex = 0; fieldIndex < fieldNames.Length - 1; fieldIndex++)
            {
                queryStringBuilder.Append(fieldNames[fieldIndex]);
                queryStringBuilder.Append(", ");
            }
            queryStringBuilder.Append(fieldNames[fieldNames.Length - 1]);
            queryStringBuilder.Append(" Из Справочник.Банки");

            var queryText = queryStringBuilder.ToString();
            Console.WriteLine(queryText);

            var connectionBuilder = new OneSConnectionStringBuilder
                {
                    Catalog = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Db"),
                    User = "Абдулов (директор)"
                };

            using (var connection = new OneSConnection(connectionBuilder.ConnectionString))
            {
                var command = new OneSCommand
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = queryText
                    };

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                {
                    Assert.AreEqual(0, reader.Depth);
                    Assert.IsTrue(reader.HasRows);
                    Assert.IsFalse(reader.IsClosed);
                    Assert.AreEqual(-1, reader.RecordsAffected);
                    
                    const string FORMAT = "Код \"{0}\"; Наименование \"{1}\"; Корр. Счет \"{2}\"; Город \"{3}\"; Адрес \"{4}\"; Телефоны \"{5}\"";

                    Assert.AreEqual(FIELD_COUNT, reader.FieldCount);
                    Assert.AreEqual(FIELD_COUNT, reader.VisibleFieldCount);

                    for (var fieldIndex = 0; fieldIndex < FIELD_COUNT; fieldIndex++)
                        Assert.AreEqual(fieldNames[fieldIndex], reader.GetName(fieldIndex));

                    Assert.AreEqual(typeof(string), reader.GetFieldType(0));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(1));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(2));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(3));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(4));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(5));

                    // Тестирование GetOrdinal
                    for (var fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                        Assert.AreEqual(fieldIndex, reader.GetOrdinal(fieldNames[fieldIndex]));

                    var values = new object[FIELD_COUNT];

                    var recordCounter = 0;

                    while (reader.Read())
                    {
                        Assert.Less(recordCounter, expectedRecords.Length);

                        Assert.AreEqual(FIELD_COUNT, reader.GetValues(values));

                        Trace.WriteLine(string.Format(FORMAT, values));

                        var expectedRecord = expectedRecords[recordCounter];

                        for (var fieldIndex = 0; fieldIndex < expectedRecord.Length; fieldIndex++)
                        {
                            Assert.AreEqual(expectedRecord[fieldIndex], values[fieldIndex]);
                            Assert.AreEqual(expectedRecord[fieldIndex], reader[fieldIndex]);
                            Assert.AreEqual(expectedRecord[fieldIndex], reader.GetValue(fieldIndex));
                            Assert.AreEqual(expectedRecord[fieldIndex], reader[fieldNames[fieldIndex]]);
                        }

                        ++recordCounter;
                    }
                }
            }
        }

        /// <summary>Тестирование простого запроса.</summary>
        [Test]
        public void TestSimpleExecuteReaderV2()
        {
            const int FIELD_COUNT = 8;

            var fieldNames = new[]
                {
                    "СтроковоеПоле",
                    "ЦелочисленноеПоле",
                    "ЧисловоеПоле",
                    "БулевоПоле",
                    "ДатаПоле",
                    "ДатаВремяПоле",
                    "ВремяПоле",
                    "НеограниченноеСтроковоеПоле"
                };

            Assert.AreEqual(FIELD_COUNT, fieldNames.Length);

            var expectedRecords = new[]
                {
                    new object[]
                        {
                            "Тестирование", 234, 546.323, true,
                            new DateTime(2014, 01, 15), new DateTime(2014, 01, 08, 4, 33, 43),
                            new DateTime(100, 1, 1, 23, 43, 43),
                            @"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
                        },
                    new object[]
                        {
                            "", 0, 0, false,
                            new DateTime(100, 1, 1), new DateTime(100, 1, 1),
                            new DateTime(100, 1, 1),
                            ""
                        },
                };


            var queryStringBuilder = new StringBuilder("Выбрать ");
            for (var fieldIndex = 0; fieldIndex < fieldNames.Length - 1; fieldIndex++)
            {
                queryStringBuilder.Append(fieldNames[fieldIndex]);
                queryStringBuilder.Append(", ");
            }
            queryStringBuilder.Append(fieldNames[fieldNames.Length - 1]);
            queryStringBuilder.Append(" Из Справочник.ТестовыйСправочник");

            var queryText = queryStringBuilder.ToString();
            Console.WriteLine(queryText);

            var connectionBuilder = new OneSConnectionStringBuilder
            {
                Catalog = Constants.TestCatalog,
                User = Constants.TEST_USER
            };

            var formatBuilder = new StringBuilder();
            for (var fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
            {
                if (fieldIndex > 0)
                    formatBuilder.Append(", ");
                formatBuilder.Append(fieldNames[fieldIndex]);
                formatBuilder.Append(" = {");
                formatBuilder.Append(fieldIndex);
                formatBuilder.Append("}");
            }
            var format = formatBuilder.ToString();

            using (var connection = new OneSConnection(connectionBuilder.ConnectionString))
            {
                var command = new OneSCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText = queryText
                };

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                {
                    Assert.AreEqual(0, reader.Depth);
                    Assert.IsTrue(reader.HasRows);
                    Assert.IsFalse(reader.IsClosed);
                    Assert.AreEqual(-1, reader.RecordsAffected);


                    Assert.AreEqual(FIELD_COUNT, reader.FieldCount);
                    Assert.AreEqual(FIELD_COUNT, reader.VisibleFieldCount);

                    for (var fieldIndex = 0; fieldIndex < FIELD_COUNT; fieldIndex++)
                        Assert.AreEqual(fieldNames[fieldIndex], reader.GetName(fieldIndex));

                    Assert.AreEqual(typeof(string), reader.GetFieldType(0));
                    Assert.AreEqual(typeof(double), reader.GetFieldType(1));
                    Assert.AreEqual(typeof(double), reader.GetFieldType(2));
                    Assert.AreEqual(typeof(bool), reader.GetFieldType(3));
                    Assert.AreEqual(typeof(DateTime), reader.GetFieldType(4));
                    Assert.AreEqual(typeof(DateTime), reader.GetFieldType(5));
                    Assert.AreEqual(typeof(DateTime), reader.GetFieldType(6));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(7));

                    // Тестирование GetOrdinal
                    for (var fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                        Assert.AreEqual(fieldIndex, reader.GetOrdinal(fieldNames[fieldIndex]));

                    var values = new object[FIELD_COUNT];

                    var recordCounter = 0;

                    while (reader.Read())
                    {
                        Assert.Less(recordCounter, expectedRecords.Length);

                        Assert.AreEqual(FIELD_COUNT, reader.GetValues(values));

                        Trace.WriteLine(string.Format(format, values));

                        var expectedRecord = expectedRecords[recordCounter];

                        for (var fieldIndex = 0; fieldIndex < expectedRecord.Length; fieldIndex++)
                        {
                            Assert.AreEqual(expectedRecord[fieldIndex], values[fieldIndex]);
                            Assert.AreEqual(expectedRecord[fieldIndex], reader[fieldIndex]);
                            Assert.AreEqual(expectedRecord[fieldIndex], reader.GetValue(fieldIndex));
                            Assert.AreEqual(expectedRecord[fieldIndex], reader[fieldNames[fieldIndex]]);
                        }

                        Assert.AreEqual(expectedRecord[0], reader.GetString(0));
                        Assert.AreEqual(expectedRecord[1], reader.GetInt32(1));
                        Assert.AreEqual(expectedRecord[2], reader.GetDouble(2));
                        Assert.AreEqual(expectedRecord[3], reader.GetBoolean(3));
                        Assert.AreEqual(expectedRecord[4], reader.GetDateTime(4));
                        Assert.AreEqual(expectedRecord[5], reader.GetDateTime(5));
                        Assert.AreEqual(expectedRecord[6], reader.GetDateTime(6));


                        ++recordCounter;
                    }
                }
            }
        }
    }
}
