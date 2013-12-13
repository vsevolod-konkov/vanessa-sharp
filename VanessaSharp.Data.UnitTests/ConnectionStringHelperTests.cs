using System;
using NUnit.Framework;
using VanessaSharp.Data.Utility;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>Тесты на методы <see cref="ConnectionStringHelper"/>.</summary>
    [TestFixture]
    public class ConnectionStringHelperTests
    {
        /// <summary>
        /// Тест на <see cref="ConnectionStringHelper.GetCatalogFromConnectionString"/>
        /// с валидной строкой подключения.
        /// </summary>
        [Test]
        public void TestGetCatalogFromValidConnectionString()
        {
            const string EXPECTED_CATALOG = @"C:\1C";
            const string CONNECTION_STRING = "File=" + EXPECTED_CATALOG + "; Usr=Иванов";

            Assert.AreEqual(EXPECTED_CATALOG, ConnectionStringHelper.GetCatalogFromConnectionString(CONNECTION_STRING));
        }

        /// <summary>
        /// Тест на <see cref="ConnectionStringHelper.GetCatalogFromConnectionString"/>
        /// с строкой подключения.
        /// </summary>
        [Test]
        [TestCase(null, Result = "", Description = "Тестирование null")]
        [TestCase("", Result = "", Description = "Тестирование пустой строки")]
        [TestCase("абракадабра", ExpectedException = typeof(InvalidOperationException), Description = "Тестирование невалидной строки")]
        public string TestGetCatalogFromConnectionString(string connectionString)
        {
            return ConnectionStringHelper.GetCatalogFromConnectionString(connectionString);
        }
    }
}
