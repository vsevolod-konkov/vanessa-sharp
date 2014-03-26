using System;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тестирование невозможности выполнения запроса экземпляром класса <see cref="OneSCommand"/>.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, Description = "Тестирование команды запроса в реальном режиме")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, Description = "Тестирование команды запроса в изолированном режиме")]
    #endif
    public sealed class FailedExecuteTests : CommandTestsBase
    {
        public FailedExecuteTests(TestMode testMode) : base(testMode, true)
        {}

        /// <summary>
        /// Тестирование того, что метод <see cref="OneSCommand.ExecuteNonQuery"/>
        /// выбрасывает исключение <see cref="NotSupportedException"/>,
        /// так как 1С не поддерживает запросы изменяющие исходные данные.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestExecuteNonQuery()
        {
            TestedCommand.CommandText = "ВЫБРАТЬ * ИЗ Справочник.ТестовыйСправочник";
            TestedCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Тестирование <see cref="OneSCommand.ExecuteScalar"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplementedAttribute]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestExecuteScalar()
        {
            TestedCommand.CommandText = "ВЫБРАТЬ КОЛИЧЕСТВО(*) ИЗ Справочник.ТестовыйСправочник";
            TestedCommand.ExecuteScalar();
        }
    }
}
