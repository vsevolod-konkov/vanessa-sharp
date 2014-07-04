using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;
using VanessaSharp.AcceptanceTests.Utility.Mocks;
using VanessaSharp.Data;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.AcceptanceTests.Utility
{
    /// <summary>
    /// Базовый класс для тестов проверяющих результаты запросов.
    /// </summary>
    public abstract class QueryTestsBase : ConnectedTestsBase
    {
        /// <summary>
        /// Стратегия режима тестирования.
        /// </summary>
        private readonly ITestModeStrategy _testModeStrategy;

        /// <summary>Конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        protected QueryTestsBase(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        {
            _testModeStrategy = CreateTestModeStrategy(testMode);
        }

        /// <summary>
        /// Фабричный метод создания объекта стратегии режима тестирования.
        /// </summary>
        /// <param name="testMode">Режим тестирования.</param>
        private static ITestModeStrategy CreateTestModeStrategy(TestMode testMode)
        {
            return (testMode == TestMode.Isolated)
                                    ? (ITestModeStrategy)new IsolatedModeStrategy()
                                    : new RealModeStrategy();
        }

        /// <summary>
        /// Обработчик запроса на создание экземпляра объекта 1С.
        /// </summary>
        protected sealed override void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {
            _testModeStrategy.OnNewOneSObjectAsking(args);
        }

        /// <summary>Начало описания теста.</summary>
        protected InitBuilderState Test
        {
            get { return new InitBuilderState(Connection, _testModeStrategy); }
        }

        #region Стратегии в зависимости от режима тестирования

        /// <summary>
        /// Интерфейс стратегии режима тестирования.
        /// </summary>
        protected interface ITestModeStrategy
        {
            void SetUp(TableData expectedData);

            void OnNewOneSObjectAsking(NewOneSObjectEventArgs args);

            void VerifyQueryExecute(OneSCommand command);

            void AssertSql(string expectedSql);

            void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters);
        }

        /// <summary>
        /// Стратегия реального режима тестирования.
        /// </summary>
        private sealed class RealModeStrategy : ITestModeStrategy
        {
            public void SetUp(TableData expectedData)
            { }

            public void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
            { }

            public void VerifyQueryExecute(OneSCommand command)
            { }

            public void AssertSql(string expectedSql)
            { }

            public void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
            { }
        }

        /// <summary>
        /// Стратегия изолированного режима тестирования.
        /// </summary>
        private sealed class IsolatedModeStrategy : ITestModeStrategy
        {
            private Mock<IQuery> _queryMock;
            private Dictionary<string, object> _parameters;

            public void SetUp(TableData expectedData)
            {
                _parameters = new Dictionary<string, object>();

                _queryMock = MockHelper.CreateDisposableMock<IQuery>();
                _queryMock
                    .SetupProperty(q => q.Text);
                _queryMock
                    .Setup(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback<string, object>((name, value) => _parameters[name] = value);
                _queryMock
                    .Setup(q => q.Execute())
                    .Returns(() => QueryResultMockFactory.Create(expectedData));
            }

            public void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
            {
                if (args.RequiredType == typeof(IQuery))
                    args.CreatedInstance = _queryMock.Object;
            }

            public void VerifyQueryExecute(OneSCommand command)
            {
                var sql = command.CommandText;
                _queryMock.VerifySet(q => q.Text = sql, Times.Once());

                var parameters = command.Parameters.AsEnumerable();
                if (parameters.Any())
                {
                    foreach (var p in parameters)
                    {
                        _queryMock.Verify(q => q.SetParameter(p.ParameterName, p.Value), Times.Once());
                    }
                }
                else
                {
                    _queryMock.Verify(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
                }
            }

            public void AssertSql(string expectedSql)
            {
                Assert.AreEqual(expectedSql, _queryMock.Object.Text);
            }

            public void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
            {
                CollectionAssert.AreEquivalent(expectedSqlParameters, _parameters);
            }
        }

        #endregion

        #region Объекты внутреннего DSL

        /// <summary>
        /// Контекст тестирования.
        /// </summary>
        protected sealed class TestingContext
        {
            /// <summary>Ожидаемые данные.</summary>
            private readonly TableData _expectedData;

            /// <summary>
            /// Стратегия режима тестирования.
            /// </summary>
            private readonly ITestModeStrategy _testModeStrategy;

            /// <summary>Конструктор.</summary>
            /// <param name="connection">Соединение.</param>
            /// <param name="expectedData">Стратегия режима тестирования.</param>
            /// <param name="expectedRowIndexes">Индексы строк из ожидаемых данных.</param>
            /// <param name="testModeStrategy">Стратегия режима тестирования</param>
            public TestingContext(
                OneSConnection connection, TableData expectedData,
                ReadOnlyCollection<int> expectedRowIndexes, ITestModeStrategy testModeStrategy)
            {
                Contract.Requires<ArgumentNullException>(connection != null);
                Contract.Requires<ArgumentNullException>(expectedData != null);
                Contract.Requires<ArgumentNullException>(expectedRowIndexes != null);
                Contract.Requires<ArgumentNullException>(testModeStrategy != null);

                _connection = connection;
                _expectedData = expectedData;
                _expectedRowIndexes = expectedRowIndexes;
                _testModeStrategy = testModeStrategy;
            }

            /// <summary>Соединение.</summary>
            public OneSConnection Connection
            {
                get
                {
                    Contract.Ensures(Contract.Result<OneSConnection>() != null);
                 
                    return _connection;
                }
            }
            private readonly OneSConnection _connection;

            /// <summary>Ожидаемое количество полей.</summary>
            public int ExpectedFieldsCount
            {
                get { return _expectedData.Fields.Count; }
            }

            /// <summary>Ожидаемое имя поля.</summary>
            /// <param name="fieldIndex">Индекс поля.</param>
            public string ExpectedFieldName(int fieldIndex)
            {
                return _expectedData.Fields[fieldIndex].Name;
            }

            /// <summary>Ожидаемый тип поля.</summary>
            /// <param name="fieldIndex">Тип поля.</param>
            public Type ExpectedFieldType(int fieldIndex)
            {
                return _expectedData.Fields[fieldIndex].Type;
            }

            /// <summary>Ожидаемое количество строк.</summary>
            public int ExpectedRowsCount
            {
                get { return _expectedData.Rows.Count; }
            }

            /// <summary>
            /// Ожидаемое значение.
            /// </summary>
            /// <param name="rowIndex">Индекс строки.</param>
            /// <param name="fieldIndex">Индекс поля.</param>
            public object ExpectedValue(int rowIndex, int fieldIndex)
            {
                return _expectedData.Rows[rowIndex][fieldIndex];
            }

            /// <summary>Индексы строк из ожидаемых данных.</summary>
            public ReadOnlyCollection<int> ExpectedRowIndexes
            {
                get
                {
                    Contract.Ensures(Contract.Result<ReadOnlyCollection<int>>() != null);

                    return _expectedRowIndexes;
                }
            }
            private readonly ReadOnlyCollection<int> _expectedRowIndexes;

            /// <summary>
            /// Проверка вызовов выполнения методов объекта запроса 1С.
            /// </summary>
            /// <param name="command">
            /// Команда, через которую производился вызов 1С.
            /// </param>
            public void VerifyQueryExecute(OneSCommand command)
            {
                Contract.Requires<ArgumentNullException>(command != null);
                
                _testModeStrategy.VerifyQueryExecute(command);
            }

            /// <summary>Проверка запроса SQL переданного в 1С.</summary>
            /// <param name="expectedSql">Ожидаемый SQL-запрос.</param>
            public void AssertSql(string expectedSql)
            {
                _testModeStrategy.AssertSql(expectedSql);
            }

            /// <summary>Проверка параметров SQL-запроса переданных в 1С.</summary>
            /// <param name="expectedSqlParameters">
            /// Ожидаемый словарь параметров.
            /// </param>
            public void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
            {
                _testModeStrategy.AssertSqlParameters(expectedSqlParameters);
            }
        }

        /// <summary>Начальное состояние построения теста.</summary>
        protected sealed class InitBuilderState
        {
            private readonly OneSConnection _connection;
            private readonly ITestModeStrategy _testModeStrategy;

            public InitBuilderState(OneSConnection connection, ITestModeStrategy testModeStrategy)
            {
                Contract.Requires<ArgumentNullException>(connection != null);
                Contract.Requires<ArgumentNullException>(testModeStrategy != null);

                _connection = connection;
                _testModeStrategy = testModeStrategy;
            }

            /// <summary>Описание тестирующего действия.</summary>
            /// <param name="testingAction">Тестирующее действие.</param>
            public ActionDefinedBuilderState Action(Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(testingAction != null);
                Contract.Ensures(Contract.Result<ActionDefinedBuilderState>() != null);

                return new ActionDefinedBuilderState(_connection, _testModeStrategy, testingAction);
            }
        }

        /// <summary>
        /// Состояние после описания действия.
        /// </summary>
        protected sealed class ActionDefinedBuilderState
        {
            private readonly OneSConnection _connection;
            private readonly ITestModeStrategy _testModeStrategy;
            private readonly Action<TestingContext> _testingAction;

            public ActionDefinedBuilderState(OneSConnection connection, ITestModeStrategy testModeStrategy, Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(connection != null);
                Contract.Requires<ArgumentNullException>(testModeStrategy != null);
                Contract.Requires<ArgumentNullException>(testingAction != null);

                _connection = connection;
                _testingAction = testingAction;
                _testModeStrategy = testModeStrategy;
            }

            /// <summary>
            /// Начало описания ожидаемых данных.
            /// </summary>
            /// <typeparam name="TExpectedData">Тип ожидаемых данных.</typeparam>
            public DefiningExpectedDataBuilderState<TExpectedData> BeginDefineExpectedDataFor<TExpectedData>()
            {
                Contract.Ensures(Contract.Result<DefiningExpectedDataBuilderState<TExpectedData>>() != null);

                return new DefiningExpectedDataBuilderState<TExpectedData>(_connection, _testModeStrategy, _testingAction);
            }
        }

        /// <summary>Состояние описания ожидаемых данных.</summary>
        /// <typeparam name="TExpectedData">
        /// Тип ожидаемых данных.
        /// </typeparam>
        protected sealed class DefiningExpectedDataBuilderState<TExpectedData>
        {
            private readonly OneSConnection _connection;
            private readonly ITestModeStrategy _testModeStrategy;
            private readonly Action<TestingContext> _testingAction;
            private readonly List<Func<TExpectedData, object>> _fieldAccessors = new List<Func<TExpectedData, object>>();
            private readonly TableDataBuilder _tableDataBuilder = new TableDataBuilder();

            public DefiningExpectedDataBuilderState(
                OneSConnection connection, ITestModeStrategy testModeStrategy, Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(connection != null);
                Contract.Requires<ArgumentNullException>(testingAction != null);

                _connection = connection;
                _testingAction = testingAction;
                _testModeStrategy = testModeStrategy;
            }

            /// <summary>
            /// Описание поля ожидаемых данных.
            /// </summary>
            /// <typeparam name="TValue">Тип поля.</typeparam>
            /// <param name="fieldAccessor">Выражение доступа.</param>
            public DefiningExpectedDataBuilderState<TExpectedData> Field<TValue>(Expression<Func<TExpectedData, TValue>> fieldAccessor)
            {
                Contract.Requires<ArgumentNullException>(fieldAccessor != null);
                Contract.Ensures(Contract.Result<DefiningExpectedDataBuilderState<TExpectedData>>() != null);

                var info = ExpectedDataHelper.ExtractFieldInfo(fieldAccessor);

                AddFieldDefinition(info.Name, info.Type, info.Accessor);

                return this;
            }

            /// <summary>
            /// Описания поля, чей тип и значения не проверяются.
            /// </summary>
            /// <param name="fieldName">Имя поля.</param>
            public DefiningExpectedDataBuilderState<TExpectedData> AnyField(string fieldName)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
                Contract.Ensures(Contract.Result<DefiningExpectedDataBuilderState<TExpectedData>>() != null);

                AddFieldDefinition(fieldName, typeof(AnyType), d => AnyType.Instance);

                return this;
            }

            private void AddFieldDefinition(string fieldName, Type fieldType, Func<TExpectedData, object> fieldAccessor)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
                Contract.Requires<ArgumentNullException>(fieldType != null);
                Contract.Requires<ArgumentNullException>(fieldAccessor != null);

                _tableDataBuilder.AddField(fieldName, fieldType);
                _fieldAccessors.Add(fieldAccessor);
            }

            private DefinedExpectedDataBuilderState GetNextState(
                IEnumerable<TExpectedData> rows, ReadOnlyCollection<int> expectedRowIndexes)
            {
                foreach (var row in rows)
                {
                    var rowData = _fieldAccessors.Select(fa => fa(row)).ToArray();
                    _tableDataBuilder.AddRow(rowData);
                }

                var tableData = _tableDataBuilder.Build();
                _testModeStrategy.SetUp(tableData);
                var testingContext = new TestingContext(_connection, tableData, expectedRowIndexes, _testModeStrategy);

                return new DefinedExpectedDataBuilderState(testingContext, _testingAction);
            }

            private static IList<TExpectedData> GetExpectedData()
            {
                return ExpectedDataHelper.GetExpectedData<TExpectedData>();
            }

            /// <summary>
            /// Указание, того какие строки должны войти в ожидаемые данные теста.
            /// </summary>
            /// <param name="rowIndexes">Индексы строк.</param>
            public DefinedExpectedDataBuilderState Rows(params int[] rowIndexes)
            {
                Contract.Ensures(Contract.Result<DefinedExpectedDataBuilderState>() != null);
                
                var expectedData = GetExpectedData();

                return GetNextState(
                    rowIndexes.Select(i => expectedData[i]),
                    new ReadOnlyCollection<int>(rowIndexes));
            }

            /// <summary>
            /// Указание того, что в тест должны войти все строки ожидаемых данных.
            /// </summary>
            public DefinedExpectedDataBuilderState AllRows
            {
                get
                {
                    Contract.Ensures(Contract.Result<DefinedExpectedDataBuilderState>() != null);

                    var expectedData = GetExpectedData();
                    var expectedRowIndexes = new ReadOnlyCollection<int>(
                        Enumerable.Range(0, expectedData.Count).ToArray()
                        );

                    return GetNextState(expectedData, expectedRowIndexes);
                }
            }
        }

        /// <summary>Завершающее состояние описание ожидаемых жанных теста.</summary>
        protected sealed class DefinedExpectedDataBuilderState
        {
            private readonly TestingContext _testingContext;
            private readonly Action<TestingContext> _testingAction;

            public DefinedExpectedDataBuilderState(TestingContext testingContext, Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(testingContext != null);
                Contract.Requires<ArgumentNullException>(testingAction != null);

                _testingContext = testingContext;
                _testingAction = testingAction;
            }

            /// <summary>
            /// Указание того, что описание ожидаемых данных теста завершено.
            /// </summary>
            public FinalBuilderState EndDefineExpectedData
            {
                get
                {
                    Contract.Ensures(Contract.Result<FinalBuilderState>() != null);

                    return new FinalBuilderState(_testingContext, _testingAction);
                }
            }
        }

        /// <summary>
        /// Финальное состояние описания теста.
        /// </summary>
        protected sealed class FinalBuilderState
        {
            private readonly TestingContext _testingContext;
            private readonly Action<TestingContext> _testingAction;

            public FinalBuilderState(TestingContext testingContext, Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(testingContext != null);
                Contract.Requires<ArgumentNullException>(testingAction != null);

                _testingContext = testingContext;
                _testingAction = testingAction;
            }

            /// <summary>
            /// Выполнение теста.
            /// </summary>
            public void Run()
            {
                _testingAction(_testingContext);
            }
        }

        #endregion
    }
}