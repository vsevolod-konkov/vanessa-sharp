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
    [Ignore]
    public sealed class OneSCommandTests
    {
        /// <summary>Тестирование простого запроса.</summary>
        [Test]
        public void TestSimpleExecuteReader()
        {
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

                    while (reader.Read())
                    {
                        Assert.AreEqual(6, reader.GetValues(values));
                        //var code = reader.GetString(0);
                        //var name = reader.GetString(1);
                        //var account = reader.GetString(2);
                        //var city = reader.GetString(3);
                        //var address = reader.GetString(4);
                        //var phones = reader.GetString(5);

                        //Trace.WriteLine(string.Format(FORMAT, code, name, account, city, address, phones));   
                        Trace.WriteLine(string.Format(FORMAT, values));   
                    }
                }
            }
        }
    }
}
