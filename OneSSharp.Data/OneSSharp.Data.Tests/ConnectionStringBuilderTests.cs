using System;
using System.Data.Common;
using NUnit.Framework;

namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Тестирование класса <see cref="OneSConnectionStringBuilder"/>.</summary>
    [TestFixture]
    public sealed class ConnectionStringBuilderTests
    {
        /// <summary>Тестирование свойства <see cref="OneSConnectionStringBuilder.Catalog"/>.</summary>
        [Test(Description="Тестирование свойства Catalog")]
        public void TestCatalogProperty()
        {
            var builder = new OneSConnectionStringBuilder();
            builder.Catalog = "Catalog";

            Assert.AreEqual("Catalog", builder["File"]);
            Assert.AreEqual("File=Catalog", builder.ConnectionString);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnectionStringBuilder.User"/>.</summary>
        [Test(Description = "Тестирование свойства User")]
        public void TestUserProperty()
        {
            var builder = new OneSConnectionStringBuilder();
            builder.User = "Иванов";

            Assert.AreEqual("Иванов", builder["Usr"]);
            Assert.AreEqual("Usr=Иванов", builder.ConnectionString);
        }

        /// <summary>Тестирование свойства <see cref="OneSConnectionStringBuilder.Password"/>.</summary>
        [Test(Description = "Тестирование свойства Password")]
        public void TestPasswordProperty()
        {
            var builder = new OneSConnectionStringBuilder();
            builder.Password = "12345";

            Assert.AreEqual("12345", builder["Pwd"]);
            Assert.AreEqual("Pwd=12345", builder.ConnectionString);
        }

        /// <summary>Тестирование совместного использования свойств.</summary>
        [Test(Description = "Тестирование совместного использования свойств")]
        public void TestComplex()
        {
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = @"C:\1CData";
                builder.User = "Иванов";
                Assert.AreEqual(@"File=C:\1CData;Usr=Иванов", builder.ConnectionString);

                builder.Password = "12345";
                Assert.AreEqual(@"File=C:\1CData;Usr=Иванов;Pwd=12345", builder.ConnectionString);

                builder.Password = string.Empty;
                Assert.AreEqual(@"File=C:\1CData;Usr=Иванов;Pwd=", builder.ConnectionString);
            }

            {
                var builder = new OneSConnectionStringBuilder();
                builder.User = "Иванов";
                builder.Catalog = @"C:\1CData";
                Assert.AreEqual(@"Usr=Иванов;File=C:\1CData", builder.ConnectionString);
            }
        }

        /// <summary>Тестирование парсинга строки соединения.</summary>
        [Test(Description="Тестирование парсинга строки соединения")]
        public void TestParse()
        {
            var builder = new OneSConnectionStringBuilder();
            builder.ConnectionString = @"File=C:\1CData;Usr=Иванов;Pwd=12345";

            Assert.AreEqual(@"C:\1CData", builder["File"]);
            Assert.AreEqual(@"C:\1CData", builder.Catalog);

            Assert.AreEqual("Иванов", builder["Usr"]);
            Assert.AreEqual("Иванов", builder.User);

            Assert.AreEqual("12345", builder["Pwd"]);
            Assert.AreEqual("12345", builder.Password);
        }

        /// <summary>Тестирование совместного использования <see cref="DbConnectionStringBuilder.ConnectionString"/> и других свойств.</summary>
        [Test(Description="Тестирование совместного использования ConnectionString и других свойств")]
        public void TestUseConnectionString()
        {
            var builder = new OneSConnectionStringBuilder();
            builder.ConnectionString = @"File=C:\1CData;Usr=Иванов";

            builder.Password = "12345";
            Assert.AreEqual(@"file=C:\1CData;usr=Иванов;Pwd=12345", builder.ConnectionString);

            builder.User = "Петров";
            Assert.AreEqual(@"file=C:\1CData;usr=Петров;Pwd=12345", builder.ConnectionString);
        }

        /// <summary>Тестирование поведения построителя при задании некорректной строки соединения.</summary>
        [Test(Description="Тестирование поведения построителя при задании некорректной строки соединения")]
        public void TestInvalidConnectionString()
        {
            var builder = new OneSConnectionStringBuilder();
            ChecksHelper.AssertException<ArgumentException>(() =>
            {
                builder.ConnectionString = "белеберда";
            });
        }
    }
}
