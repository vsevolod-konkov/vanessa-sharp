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

        protected new BuilderState0 Test
        {
            get
            {
                return new BuilderState0(base.Test);
            }
        }

        protected sealed class BuilderState0
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public BuilderState0(InitBuilderState innerState)
            {
                _innerState = innerState;
            }

            public BuilderState1<T> Query<T>(Func<OneSDataContext, IQueryable<T>> queryAction)
            {
                return new BuilderState1<T>(_innerState, queryAction);
            }
        }

        protected sealed class BuilderState1<T>
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<T>> _queryAction;

            public BuilderState1(InitBuilderState innerState, Func<OneSDataContext, IQueryable<T>> queryAction)
            {
                _innerState = innerState;
                _queryAction = queryAction;
            }

            public BuilderState2<T> AssertSql(string sql)
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

            public BuilderState2(InitBuilderState innerState, Func<OneSDataContext, IQueryable<T>> queryAction, string expectedSql)
            {
                _innerState = innerState;
                _queryAction = queryAction;
                _expectedSql = expectedSql;
            }

            public BuilderState2<T> AssertSqlParameter(string name, object value)
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
    }
}
