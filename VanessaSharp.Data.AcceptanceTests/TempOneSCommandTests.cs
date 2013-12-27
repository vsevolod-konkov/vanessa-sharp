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

                    const string FORMAT = "Код \"{0}\"; Наименование \"{1}\"; Корр. Счет \"{2}\"; Город \"{3}\"; Адрес \"{4}\"; Телефоны \"{5}\"";

                    Assert.AreEqual(FIELD_COUNT, reader.FieldCount);

                    for (var fieldIndex = 0; fieldIndex < FIELD_COUNT; fieldIndex++)
                        Assert.AreEqual(fieldNames[fieldIndex], reader.GetName(fieldIndex));

                    Assert.AreEqual(typeof(string), reader.GetFieldType(0));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(1));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(2));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(3));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(4));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(5));

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
                            Assert.AreEqual(expectedRecord[fieldIndex], reader[fieldNames[fieldIndex]]);
                        }

                        ++recordCounter;
                    }
                }
            }
        }
    }
}
