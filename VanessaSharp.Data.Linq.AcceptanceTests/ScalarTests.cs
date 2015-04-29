using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тесты на выполнение скалярных linq-запросов.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public sealed class ScalarTests : QueryTestsBase
    {
        public ScalarTests(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {}

        /// <summary>
        /// Тестирование запроса получения суммы значений.
        /// </summary>
        [Test]
        public void TestSum()
        {
            Test.Action(ctx =>
                {
                    // Act
                    int actualResult;
                    using (var dataCtx = new OneSDataContext(ctx.Connection))
                    {
                        actualResult = dataCtx.Get<TestDictionary>()
                                              .Select(d => d.IntegerField)
                                              .Sum();
                    }

                    // Assert
                    var expectedResult = Enumerable
                        .Range(0, ctx.ExpectedRowsCount)
                        .Select(i => ctx.ExpectedValue(i, 0))
                        .Cast<int>()
                        .Sum();

                    Assert.AreEqual(expectedResult, actualResult);

                    ctx.AssertSql("SELECT SUM(ЦелочисленноеПоле) FROM Справочник.ТестовыйСправочник");
                    ctx.AssertSqlParameters(new Dictionary<string, object>());
                })

                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                .Field(e => e.IntField)
                .AllRows
                .EndDefineExpectedData
                .Run();
        }
    }
}
