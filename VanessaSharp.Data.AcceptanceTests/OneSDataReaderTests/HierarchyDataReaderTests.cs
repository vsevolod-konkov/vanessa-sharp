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
        [Ignore("Пока не реализовано GetDescendantsReader")]
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
                .Action(ctx => PrintRecords(ctx.TestedReader))
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()
                .AllRows
                .EndDefineExpectedData
           .Run();
        }

        private static void PrintRecords(OneSDataReader testedReader)
        {
            while (testedReader.Read())
            {
                Console.WriteLine("Наименование = {0}, Количество = {1}, Цена = {2}, Уровень = {3}, Группировка = {4}, Тип записи = {5}",
                    testedReader.GetString(0),
                    testedReader.IsDBNull(1) ? "" : (object)testedReader.GetInt32(1),
                    testedReader.IsDBNull(2) ? "" : (object)testedReader.GetDecimal(2),
                    testedReader.Level,
                    testedReader.GroupName,
                    testedReader.RecordType);

                using (var descendantsReader = testedReader.GetDescendantsReader())
                    PrintRecords(descendantsReader);
            }
        }
    }
}
