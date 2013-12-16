using System;
using System.Data;
using System.Diagnostics;
using System.IO;
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
                        CommandText = "Выбрать Код, Наименование, КоррСчет, Город, Адрес, Телефоны Из Справочник.Банки"
                    };

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult))
                {
                    

                    const string FORMAT = "Код \"{0}\"; Наименование \"{1}\"; Корр. Счет \"{2}\"; Город \"{3}\"; Адрес \"{4}\"; Телефоны \"{5}\"";

                    Assert.AreEqual(6, reader.FieldCount);

                    Assert.AreEqual("Код", reader.GetName(0));
                    Assert.AreEqual("Наименование", reader.GetName(1));
                    Assert.AreEqual("КоррСчет", reader.GetName(2));
                    Assert.AreEqual("Город", reader.GetName(3));
                    Assert.AreEqual("Адрес", reader.GetName(4));
                    Assert.AreEqual("Телефоны", reader.GetName(5));

                    Assert.AreEqual(typeof(string), reader.GetFieldType(0));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(1));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(2));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(3));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(4));
                    Assert.AreEqual(typeof(string), reader.GetFieldType(5));

                    var values = new object[6];

                    var recordCounter = 0;

                    while (reader.Read())
                    {
                        Assert.Less(recordCounter, expectedRecords.Length);
                        
                        Assert.AreEqual(6, reader.GetValues(values));

                        Trace.WriteLine(string.Format(FORMAT, values));

                        var expectedRecord = expectedRecords[recordCounter];

                        for (var fieldIndex = 0; fieldIndex < expectedRecord.Length; fieldIndex++)
                            Assert.AreEqual(expectedRecord[fieldIndex], values[fieldIndex]);

                        ++recordCounter;
                    }
                }
            }
        }
    }
}
