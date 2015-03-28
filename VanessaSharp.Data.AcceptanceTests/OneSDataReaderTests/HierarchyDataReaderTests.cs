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
        public void TestDefault(
            [Values(
                QueryResultIteration.Default,
                QueryResultIteration.Linear,
                QueryResultIteration.ByGroups,
                QueryResultIteration.ByGroupsWithHierarchy
            )]
            QueryResultIteration queryResultIteration)
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
                .Execute(queryResultIteration: queryResultIteration)
                .Action(ctx => PrintRecords(queryResultIteration, ctx.TestedReader))
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()
                .AllRows
                .EndDefineExpectedData
           .Run();
        }

        private static void PrintRecords(QueryResultIteration queryResultIteration, OneSDataReader testedReader)
        {
            Console.WriteLine("QueryResultIteration = {0}", queryResultIteration);

            PrintRecords(queryResultIteration, testedReader, 1);
        }

        private static void PrintRecords(QueryResultIteration queryResultIteration, OneSDataReader testedReader, int recursion)
        {
            Console.WriteLine("Recursion = {0}", recursion);

            while (testedReader.Read())
            {
                Console.WriteLine("Наименование = {0}, Количество = {1}, Цена = {2}, Уровень = {3}, Группировка = {4}, Тип записи = {5}",
                    testedReader.GetString(0),
                    testedReader.IsDBNull(1) ? "" : (object)testedReader.GetInt32(1),
                    testedReader.IsDBNull(2) ? "" : (object)testedReader.GetDecimal(2),
                    testedReader.Level,
                    testedReader.GroupName,
                    testedReader.RecordType);

                //using (var descendantsReader = testedReader.GetDescendantsReader(queryResultIteration))
                //    PrintRecords(queryResultIteration, descendantsReader, recursion + 1);
            }
        }

        private static void PrintRecord(OneSDataReader testedReader)
        {
            Console.WriteLine("Наименование = {0}, Количество = {1}, Цена = {2}, Уровень = {3}, Группировка = {4}, Тип записи = {5}",
                                testedReader.GetString(0),
                                testedReader.IsDBNull(1) ? "" : (object)testedReader.GetInt32(1),
                                testedReader.IsDBNull(2) ? "" : (object)testedReader.GetDecimal(2),
                                testedReader.Level,
                                testedReader.GroupName,
                                testedReader.RecordType);
        }

        [Test]
        public void TestLinear()
        {
            Test
                .Sql(
                    @"
                      ВЫБРАТЬ Наименование, Количество, Цена
                      ИЗ Справочник.ИерархическийСправочник
                      ИТОГИ
                      СУММА(Количество),
                      СРЕДНЕЕ(Цена)
                      ПО ОБЩИЕ, Ссылка ТОЛЬКО ИЕРАРХИЯ Как ТоварИерархия
                    "
                )
                .Execute(queryResultIteration: QueryResultIteration.Linear)
                .Action(ctx =>
                {
                    
                    
                    while (ctx.TestedReader.Read())
                    {
                        PrintRecord(ctx.TestedReader);

                        
                    }
                })
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()

                .Field(d => d.Name)
                .Field(d => d.Price)
                .Field(d => d.Quantity)

                .AllRows
                .EndDefineExpectedData
           .Run();
        }

        [Test]
        public void Test3()
        {
            Test
                .Sql(
                    @"
                      ВЫБРАТЬ Наименование, Количество, Цена
                      ИЗ Справочник.ИерархическийСправочник
                      ИТОГИ
                      СУММА(Количество),
                      СРЕДНЕЕ(Цена)
                      ПО Ссылка ТОЛЬКО ИЕРАРХИЯ Как ТоварИерархия
                    "
                )
                .Execute(queryResultIteration: QueryResultIteration.ByGroups)
                .Action(ctx =>
                {
                    while (ctx.TestedReader.Read())
                        PrintRecord(ctx.TestedReader);
                })
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()
                .AllRows
                .EndDefineExpectedData
           .Run();
        }

        [Test]
        public void Test4()
        {
            Test
                .Sql(
                    @"
                      ВЫБРАТЬ Наименование, Количество, Цена
                      ИЗ Справочник.ИерархическийСправочник
                      УПОРЯДОЧИТЬ ПО Ссылка ИЕРАРХИЯ
                    "
                )
                .Execute(queryResultIteration: QueryResultIteration.ByGroupsWithHierarchy)
                .Action(ctx =>
                {
                    while (ctx.TestedReader.Read())
                    {
                        PrintRecord(ctx.TestedReader);

                        using (var descendantsReader = ctx.TestedReader.GetDescendantsReader(QueryResultIteration.ByGroupsWithHierarchy))
                        {
                            while (descendantsReader.Read())
                            {
                                PrintRecord(descendantsReader);

                                using (var descendantsReader2 = descendantsReader.GetDescendantsReader(QueryResultIteration.ByGroupsWithHierarchy))
                                {
                                    while (descendantsReader2.Read())
                                    {
                                        PrintRecord(descendantsReader2);
                                    }
                                }
                            }
                        }
                    }
                })
                .BeginDefineExpectedDataFor<ExpectedHierarchicalTestDictionary>()
                .AllRows
                .EndDefineExpectedData
           .Run();
        }
    }
}
