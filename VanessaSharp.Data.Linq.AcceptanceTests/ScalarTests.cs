using System;
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

        #region Объекты DSL

        private new InitState Test
        {
            get { return new InitState(base.Test); }
        }

        private sealed class InitState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public InitState(InitBuilderState innerState)
            {
                _innerState = innerState;
            }

            public DefinedQueryState<T> Linq<T>(Func<OneSDataContext, T> query)
            {
                return new DefinedQueryState<T>(_innerState, query);
            }
        }

        private sealed class DefinedQueryState<TResult>
        {
            private readonly InitBuilderState _innerState;
            private readonly Func<OneSDataContext, TResult> _query;

            public DefinedQueryState(InitBuilderState innerState, Func<OneSDataContext, TResult> query)
            {
                _innerState = innerState;
                _query = query;
            }

            public DefinedExpectedSqlState<TResult> ExpectSql(string sql)
            {
                return new DefinedExpectedSqlState<TResult>(_innerState, _query, sql);
            }
        }

        private sealed class DefinedExpectedSqlState<TResult>
        {
            private readonly InitBuilderState _innerState;
            private readonly Func<OneSDataContext, TResult> _query;
            private readonly string _sql;

            public DefinedExpectedSqlState(InitBuilderState innerState, Func<OneSDataContext, TResult> query, string sql)
            {
                _innerState = innerState;
                _query = query;
                _sql = sql;
            }

            public DefiningExpectedValueState<TExpectedData> Define<TExpectedData>()
            {
                var newInnerState = _innerState
                    .Action(Action)
                    .BeginDefineExpectedDataFor<TExpectedData>();

                return new DefiningExpectedValueState<TExpectedData>(newInnerState);
            }
 
            private void Action(TestingContext ctx)
            {
                 // Act
                TResult actualResult;
                using (var dataCtx = new OneSDataContext(ctx.Connection))
                    actualResult = _query(dataCtx);

               // Assert
                var expectedResult = ctx.ExpectedValue(0, 0);
                Assert.AreEqual(expectedResult, actualResult);

                ctx.AssertSql(_sql);
                ctx.AssertSqlParameters(new Dictionary<string, object>());
            }
        }

        private sealed class DefiningExpectedValueState<TExpectedData>
        {
            private readonly DefiningExpectedDataBuilderState<TExpectedData> _innerState;

            public DefiningExpectedValueState(DefiningExpectedDataBuilderState<TExpectedData> innerState)
            {
                _innerState = innerState;
            }

            public FinalState ExpectAggregateValue<T>(Func<IEnumerable<TExpectedData>, T> aggregator)
            {
                var dataType =
                    (new[] {typeof (int), typeof (long), typeof (float), typeof (decimal)}.Contains(typeof (T)))
                        ? typeof (double)
                        : typeof (T);

                
                return new FinalState(
                    _innerState.Aggregate(aggregator, "c1", "Число", dataType));
            }
        }

        private sealed class FinalState
        {
            private readonly DefinedExpectedDataBuilderState _innerState;

            public FinalState(DefinedExpectedDataBuilderState innerState)
            {
                _innerState = innerState;
            }
            public void Run()
            {
                _innerState
                    .EndDefineExpectedData
                    .Run();
            }
        }

        #endregion

        /// <summary>
        /// Тестирование запроса получения суммы значений.
        /// </summary>
        [Test]
        public void TestSum()
        {
            Test
                .Linq
                (
                        dataCtx => dataCtx.Get<TestDictionary>()
                                          .Select(d => d.IntegerField)
                                          .Sum()
                )

                .ExpectSql("SELECT SUM(ЦелочисленноеПоле) FROM Справочник.ТестовыйСправочник")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Sum(d => d.IntField))

                .Run();
        }

        /// <summary>
        /// Тестирование запроса получения количества всех строк <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestCountForDataRecords()
        {
            Test
                .Linq
                (
                        dataCtx => dataCtx.GetRecords("Справочник.ТестовыйСправочник")
                                          .Count()
                )

                .ExpectSql("SELECT COUNT(*) FROM Справочник.ТестовыйСправочник")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Count())

                .Run();
        }

        /// <summary>
        /// Тестирование запроса получения количества всех строк типизированных записей.
        /// </summary>
        [Test]
        public void TestCountForTypedRecords()
        {
            Test
                .Linq
                (
                    dataCtx => dataCtx.Get<TestDictionary>().Count()
                )

                .ExpectSql("SELECT COUNT(*) FROM Справочник.ТестовыйСправочник")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Count())

                .Run();
        }

        /// <summary>
        /// Тестирование запроса получения количества всех строк у которых выбранное поле не NULL.
        /// </summary>
        [Test]
        public void TestCountField()
        {
            Test
                .Linq
                (
                    dataCtx => dataCtx.Get<TestDictionary>()
                                      .Select(d => d.IntegerField)
                                      .Count()
                )

                .ExpectSql("SELECT COUNT(ЦелочисленноеПоле) FROM Справочник.ТестовыйСправочник")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Count())

                .Run();
        }

        /// <summary>
        /// Тестирование запроса получения количества всех разлиных значений выбранного поля.
        /// </summary>
        [Test]
        public void TestCountDistinctField()
        {
            Test
                .Linq
                (
                    dataCtx => dataCtx.Get<TestDictionary>()
                                      .Select(d => d.IntegerField)
                                      .Distinct()
                                      .Count()
                )

                .ExpectSql("SELECT COUNT(DISTINCT ЦелочисленноеПоле) FROM Справочник.ТестовыйСправочник")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Count())

                .Run();
        }

        /// <summary>
        /// Тестирование запроса с методом <see cref="Queryable.LongCount{TSource}(System.Linq.IQueryable{TSource})"/>.
        /// </summary>
        [Test]
        public void TestLongCount()
        {
            Test
                .Linq
                (
                    dataCtx => dataCtx.Get<TestDictionary>()
                                      .Where(d => d.IntegerField > 0)
                                      .LongCount()
                )

                .ExpectSql("SELECT COUNT(*) FROM Справочник.ТестовыйСправочник WHERE ЦелочисленноеПоле > 0")

                .Define<ExpectedTestDictionary>()

                .ExpectAggregateValue(s => s.Count(d => d.IntField > 0))

                .Run();
        }
    }
}
