using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Приемочные тесты на <see cref="OneSDataReader"/> в реальном режиме.</summary>
    #if REAL_MODE
    [TestFixture(Description = "Тестирование для реального режима.")]
    #endif
    public sealed class OnlyRealModeDataReaderTests : DataReaderTestsBase
    {
        public OnlyRealModeDataReaderTests() : base(TestMode.Real)
        {}

        /// <summary>Тестирование получения значения ссылки.</summary>
        [Test]
        public void TestGetValueForReference()
        {
            Test
                .Sql("ВЫБРАТЬ Ссылка ИЗ Справочник.ТестовыйСправочник")
                .Execute()
                
                .Action(ctx =>
                    {
                          Assert.IsTrue(ctx.TestedReader.Read());

                          dynamic obj = ctx.TestedReader.GetValue(0);
                          string strValue = obj.СтроковоеПоле;

                          Assert.AreEqual(ctx.ExpectedValue(0, 0), strValue);
                    })
                
                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                    .Field(d => d.StringField)
                    .AllRows
                .EndDefineExpectedData

                .Run();
        }
    }
}
