using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Приемочные тесты на <see cref="OneSDataReader"/> в реальном режиме.</summary>
    #if REAL_MODE
    [TestFixture(Description = "Тестирование для реального режима.")]
    #endif
    public sealed class TestsInRealMode : TestsBase
    {
        public TestsInRealMode() : base(TestMode.Real)
        {}

        // TODO Подумать
        /// <summary>Тестирование получения значения ссылки.</summary>
        [Test]
        public void TestGetValueForReference()
        {
            using (var command = GetCommand("ВЫБРАТЬ Ссылка ИЗ Справочник.ТестовыйСправочник"))
            using (var reader = command.ExecuteReader())
            {
                Assert.IsTrue(reader.Read());

                dynamic obj = reader.GetValue(0);
                string strValue = obj.СтроковоеПоле;

                Assert.AreEqual("Тестирование", strValue);
            }
        }
    }
}
