using System;
using System.Data;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>
    /// Тесты на инициализацию экземпляра <see cref="OneSParameter"/>.
    /// </summary>
    [TestFixture]
    public sealed class InitTests
    {
        private static void AssertPropertiesAfterInit(OneSParameter testedInstance)
        {
            Assert.AreEqual(DbType.Object, testedInstance.DbType);
            Assert.AreEqual(ParameterDirection.Input, testedInstance.Direction);
            Assert.IsTrue(testedInstance.IsNullable);
            Assert.AreEqual(0, testedInstance.Size);
            Assert.IsNull(testedInstance.SourceColumn);
            Assert.IsTrue(testedInstance.SourceColumnNullMapping);
            Assert.AreEqual(DataRowVersion.Default, testedInstance.SourceVersion);
        }
        
        /// <summary>Тестирование <see cref="OneSParameter()"/>.</summary>
        [Test]
        public void TestConstructorWithoutArguments()
        {
            var testedInstance = new OneSParameter();

            AssertPropertiesAfterInit(testedInstance);
            Assert.IsNull(testedInstance.ParameterName);
            Assert.AreEqual(DBNull.Value, testedInstance.Value);
        }

        /// <summary>Тестирование <see cref="OneSParameter(string)"/>.</summary>
        [Test]
        public void TestConstructorWithParameterName()
        {
            const string PARAMETER_NAME = "Параметр1";

            var testedInstance = new OneSParameter(PARAMETER_NAME);

            AssertPropertiesAfterInit(testedInstance);
            Assert.AreEqual(PARAMETER_NAME, testedInstance.ParameterName);
            Assert.AreEqual(DBNull.Value, testedInstance.Value);
        }

        /// <summary>Тестирование <see cref="OneSParameter(string, object)"/>.</summary>
        [Test]
        public void TestConstructorWithParameterNameAndValue()
        {
            const string PARAMETER_NAME = "Параметр1";
            const int PARAMETER_VALUE = 123;

            var testedInstance = new OneSParameter(PARAMETER_NAME, PARAMETER_VALUE);

            AssertPropertiesAfterInit(testedInstance);
            Assert.AreEqual(PARAMETER_NAME, testedInstance.ParameterName);
            Assert.AreEqual(PARAMETER_VALUE, testedInstance.Value);
        }
    }
}