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

        private static void TestDataQuery<TExpectedRecord, TActualRecord>(
            TestingContext testingContext, 
            Func<OneSDataContext, IQueryable<TActualRecord>> queryAction,
            Action<TExpectedRecord, TActualRecord> testingRecordAction,
            string expectedSql,
            IDictionary<string, object> expectedSqlParameters)
        {
            using (var dataContext = new OneSDataContext(testingContext.Connection))
            {
                var query = queryAction(dataContext);

                var recordCounter = 0;

                var expectedData = ExpectedDataHelper.GetExpectedData<TExpectedRecord>();

                foreach (var actualRecord in query)
                {
                    Assert.Less(recordCounter, testingContext.ExpectedRowsCount);

                    var expectedRow = expectedData[testingContext.ExpectedRowIndexes[recordCounter]];

                    testingRecordAction(expectedRow, actualRecord);

                    ++recordCounter;
                }

                Assert.AreEqual(testingContext.ExpectedRowsCount, recordCounter);

                testingContext.AssertSql(expectedSql);
                testingContext.AssertSqlParameters(expectedSqlParameters);
            }
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

            /// <summary>
            /// Описание ожидаемого sql-запроса полученного из linq-запроса, отправляемого в 1С.
            /// </summary>
            /// <param name="sql">Ожидаемый sql-запрос.</param>
            /// <returns></returns>
            public ActionDefiningBuilderState<T> ExpectedSql(string sql)
            {
                return new ActionDefiningBuilderState<T>(_innerState, _queryAction, sql);
            }
        }

        /// <summary>Состояние определения тестирующего действия.</summary>
        /// <typeparam name="T">
        /// Тип элементов последовательности возвращаемых тестовым запросом.
        /// </typeparam>
        protected sealed class ActionDefiningBuilderState<T>
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<T>> _queryAction;
            private readonly string _expectedSql;
            private readonly Dictionary<string, object> _expectedSqlParameters = new Dictionary<string, object>();

            public ActionDefiningBuilderState(QueryTestsBase.InitBuilderState innerState, Func<OneSDataContext, IQueryable<T>> queryAction, string expectedSql)
            {
                _innerState = innerState;
                _queryAction = queryAction;
                _expectedSql = expectedSql;
            }

            /// <summary>
            /// Описание ожидаемого парметра sql-запроса отправляемого в 1С.
            /// </summary>
            /// <param name="name">Имя параметра.</param>
            /// <param name="value">Значение параметра.</param>
            public ActionDefiningBuilderState<T> ExpectedSqlParameter(string name, object value)
            {
                _expectedSqlParameters.Add(name, value);

                return this;
            }

            /// <summary>
            /// Описание действия
            /// проверяющего элементы последовательности
            /// возвращенных тестируемым запросом.
            /// </summary>
            /// <typeparam name="TExpectedData">Тип ожидаемых данных.</typeparam>
            /// <param name="testingAction">Действие проверки.</param>
            public ActionDefinedBuilderState<TExpectedData> AssertItem<TExpectedData>(Action<TExpectedData, T> testingAction)
            {
                return new ActionDefinedBuilderState<TExpectedData>(
                    _innerState.Action(
                        ctx => TestingAction(ctx, testingAction)));
            }

            private void TestingAction<TExpectedData>(TestingContext testingContext, Action<TExpectedData, T> testingAction)
            {
                TestDataQuery(testingContext, _queryAction, testingAction, _expectedSql, _expectedSqlParameters);
            }
        }

        /// <summary>Состояние определения тестового действия.</summary>
        /// <typeparam name="TExpectedData">
        /// Тип ожидаемых данных.
        /// </typeparam>
        protected sealed class ActionDefinedBuilderState<TExpectedData>
        {
            private readonly ActionDefinedBuilderState _innerState;

            public ActionDefinedBuilderState(ActionDefinedBuilderState innerState)
            {
                _innerState = innerState;
            }

            /// <summary>
            /// Начало опеределения ожидаемых данных.
            /// </summary>
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
