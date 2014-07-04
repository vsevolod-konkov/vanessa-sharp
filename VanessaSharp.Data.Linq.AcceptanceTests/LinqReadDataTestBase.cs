using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Базовый класс для тестов на linq-запросы.
    /// </summary>
    public abstract class LinqReadDataTestBase : QueryTestsBase
    {
        protected LinqReadDataTestBase(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        { }

        /// <summary>Начало описания теста.</summary>
        protected new InitBuilderState Test
        {
            get { return new InitBuilderState(base.Test); }
        }

        #region Типы для встроенного DSL языка описания тестов

        /// <summary>Начальное состояние описания теста.</summary>
        protected new sealed class InitBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public InitBuilderState(QueryTestsBase.InitBuilderState innerState)
            {
                _innerState = innerState;
            }

            /// <summary>Описание тестируемого запроса.</summary>
            /// <typeparam name="T">
            /// Тип элементов последовательности возвращаемых запросом.
            /// </typeparam>
            /// <param name="queryAction">Функция получения запроса.</param>
            public QueryDefinedBuilderState<T> Query<T>(Func<OneSDataContext, IQueryable<T>> queryAction)
            {
                return new QueryDefinedBuilderState<T>(_innerState, queryAction);
            }
        }

        /// <summary>Состояние после описания тестируемого запроса.</summary>
        /// <typeparam name="T">
        /// Тип элементов последовательности возвращаемых запросом.
        /// </typeparam>
        protected sealed class QueryDefinedBuilderState<T>
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<T>> _queryAction;

            public QueryDefinedBuilderState(QueryTestsBase.InitBuilderState innerState, Func<OneSDataContext, IQueryable<T>> queryAction)
            {
                _innerState = innerState;
                _queryAction = queryAction;
            }

            public BuilderState2<T> ExpectedSql(string sql)
            {
                return new BuilderState2<T>(_innerState, _queryAction, sql);
            }
        }

        protected sealed class BuilderState2<T>
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<T>> _queryAction;
            private readonly string _expectedSql;
            private readonly Dictionary<string, object> _expectedSqlParameters = new Dictionary<string, object>();

            public BuilderState2(QueryTestsBase.InitBuilderState innerState, Func<OneSDataContext, IQueryable<T>> queryAction, string expectedSql)
            {
                _innerState = innerState;
                _queryAction = queryAction;
                _expectedSql = expectedSql;
            }

            public BuilderState2<T> ExpectedSqlParameter(string name, object value)
            {
                _expectedSqlParameters.Add(name, value);

                return this;
            }

            public BuilderState3<TExpectedData> AssertItem<TExpectedData>(Action<TExpectedData, T> testingAction)
            {
                return new BuilderState3<TExpectedData>(
                    _innerState.Action(
                        ctx => TestingAction(ctx, testingAction)));
            }

            private void TestingAction<TExpectedData>(TestingContext testingContext, Action<TExpectedData, T> testingAction)
            {
                using (var dataContext = new OneSDataContext(testingContext.Connection))
                {
                    var query = _queryAction(dataContext);

                    var recordCounter = 0;

                    var expectedData = ExpectedDataHelper.GetExpectedData<TExpectedData>();

                    foreach (var entry in query)
                    {
                        Assert.Less(recordCounter, testingContext.ExpectedRowsCount);

                        var expectedRow = expectedData[testingContext.ExpectedRowIndexes[recordCounter]];

                        testingAction(expectedRow, entry);

                        ++recordCounter;
                    }

                    Assert.AreEqual(testingContext.ExpectedRowsCount, recordCounter);

                    testingContext.AssertSql(_expectedSql);
                    testingContext.AssertSqlParameters(_expectedSqlParameters);
                }
            }
        }

        protected sealed class BuilderState3<TExpectedData>
        {
            private readonly ActionDefinedBuilderState _innerState;

            public BuilderState3(ActionDefinedBuilderState innerState)
            {
                _innerState = innerState;
            }

            public DefiningExpectedDataBuilderState<TExpectedData> BeginDefineExpectedData
            {
                get
                {
                    return _innerState.BeginDefineExpectedDataFor<TExpectedData>();
                }
            }
        }

        #endregion
    }
}
