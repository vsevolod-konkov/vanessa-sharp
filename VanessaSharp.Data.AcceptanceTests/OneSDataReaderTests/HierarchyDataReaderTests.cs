using System;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование чтения иерархических данных
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, Description = "Тестирование для реального режима")]
    #endif
    public sealed class HierarchyDataReaderTests : DataReaderTestsBase
    {
        public HierarchyDataReaderTests(TestMode testMode) : base(testMode)
        {}

        /// <summary>Тестирование поведения по умолчанию.</summary>
        [Test]
        public void TestDefault()
        {
            Test
                .Sql(
                    @"
                      ВЫБРАТЬ Наименование, Количество, Цена
                      ИЗ Справочник.ИерархическийСправочник
                      ИТОГИ
                      СУММА(Количество),
                      СРЕДНЕЕ(Цена)
                      ПО ОБЩИЕ, Ссылка ТОЛЬКО ИЕРАРХИЯ
                    "
                )
                .Execute(queryResultIteration: QueryResultIteration.ByGroups)
                .Action(ctx =>
                    {
                        while (ctx.TestedReader.Read())
                        {
                            Console.WriteLine("Наименование = {0}, Количество = {1}, Цена = {2}, Уровень = {3}, Группировка = {4}, Тип записи = {5}",
                                ctx.TestedReader.GetString(0),
                                ctx.TestedReader.IsDBNull(1) ? "" : (object)ctx.TestedReader.GetInt32(1),
                                ctx.TestedReader.IsDBNull(2) ? "" : (object)ctx.TestedReader.GetDecimal(2),
                                ctx.TestedReader.Level,
                                ctx.TestedReader.GroupName,
                                ctx.TestedReader.RecordType);
                        }
                    })
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()
                .AllRows
                .EndDefineExpectedData
           .Run();
        }
    }
}
