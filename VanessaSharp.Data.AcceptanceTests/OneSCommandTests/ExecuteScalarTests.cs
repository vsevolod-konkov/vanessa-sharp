using System.Collections.Generic;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>
    /// Тесты на <see cref="OneSCommand.ExecuteScalar"/>.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, Description = "Тестирование для реального режима")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, Description = "Тестирование для изолированного режима")]
    #endif
    public sealed class ExecuteScalarTests : QueryTestsBase
    {
        public ExecuteScalarTests(TestMode testMode) : base(testMode, true)
        {}

        /// <summary>
        /// Основной тест.
        /// </summary>
        [Test]
        public void TestExecuteScalar()
        {
            Test
                .Action(ctx =>
                    {
                        // Arrange
                        const string TEST_COMMAND = "ВЫБРАТЬ КОЛИЧЕСТВО(*) ИЗ Справочник.ТестовыйСправочник";
                        var testedCommand = new OneSCommand(ctx.Connection, TEST_COMMAND);
                        
                        // Act
                        var actualResult = testedCommand.ExecuteScalar();

                        // Assert
                        Assert.AreEqual(
                            ExpectedTestDictionary.ExpectedData.Count,
                            actualResult);

                        ctx.AssertSql(TEST_COMMAND);
                        ctx.AssertSqlParameters(new Dictionary<string, object>());
                    })
                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                    .Count
                .EndDefineExpectedData
                .Run();
        }
    }
}
