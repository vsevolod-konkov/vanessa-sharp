using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.Mocks;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для тестирования работы с <see cref="OneSDataReader"/>.
    /// </summary>
    public abstract class DataReaderTestsBase : QueryTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        protected DataReaderTestsBase(TestMode testMode)
            : base(testMode, true)
        { }

        /// <summary>Начало описания теста.</summary>
        protected new InitBuilderState Test
        {
            get
            {
                Contract.Ensures(Contract.Result<InitBuilderState>() != null);

                return new InitBuilderState(base.Test);
            }
        }

        #region Объекты DSL

        /// <summary>
        /// Контекст тестирования.
        /// </summary>
        protected new sealed class TestingContext
        {
            /// <summary>Внутренний контекст.</summary>
            private readonly QueryTestsBase.TestingContext _innerContext;

            /// <summary>Конструктор.</summary>
            /// <param name="innerContext">Внутренний контекст.</param>
            /// <param name="testedReader">Тестируемый читатель данных.</param>
            public TestingContext(QueryTestsBase.TestingContext innerContext, OneSDataReader testedReader)
            {
                Contract.Requires<ArgumentNullException>(innerContext != null);
                Contract.Requires<ArgumentNullException>(testedReader != null);

                _innerContext = innerContext;
                _testedReader = testedReader;
            }

            /// <summary>
            /// Тестируемый читатель данных.
            /// </summary>
            public OneSDataReader TestedReader
            {
                get
                {
                    Contract.Ensures(Contract.Result<OneSDataReader>() != null);

                    return _testedReader;
                }
            }
            private readonly OneSDataReader _testedReader;

            /// <summary>Ожидаемое количество полей.</summary>
            public int ExpectedFieldsCount
            {
                get { return _innerContext.ExpectedFieldsCount; }
            }

            /// <summary>Ожидаемое имя поля.</summary>
            /// <param name="fieldIndex">Индекс поля.</param>
            public string ExpectedFieldName(int fieldIndex)
            {
                return _innerContext.ExpectedFieldName(fieldIndex);
            }

            /// <summary>Ожидаемый тип поля.</summary>
            /// <param name="fieldIndex">Тип поля.</param>
            public Type ExpectedFieldType(int fieldIndex)
            {
                return _innerContext.ExpectedFieldType(fieldIndex);
            }

            /// <summary>Является ли поле табличной частью.</summary>
            /// <param name="fieldIndex">Индекс поля.</param>
            public bool IsFieldTablePart(int fieldIndex)
            {
                return _innerContext.IsFieldTablePart(fieldIndex);
            }

            /// <summary>Ожидаемое количество строк.</summary>
            public int ExpectedRowsCount
            {
                get { return _innerContext.ExpectedRowsCount; }
            }

            /// <summary>
            /// Ожидаемое значение.
            /// </summary>
            /// <param name="rowIndex">Индекс строки.</param>
            /// <param name="fieldIndex">Индекс поля.</param>
            public object ExpectedValue(int rowIndex, int fieldIndex)
            {
                return _innerContext.ExpectedValue(rowIndex, fieldIndex);
            }

            /// <summary>
            /// Ожидаемые данные табличной части.
            /// </summary>
            /// <param name="rowIndex">Индекс строки.</param>
            /// <param name="fieldIndex">Индекс поля.</param>
            public TableData ExpectedTablePart(int rowIndex, int fieldIndex)
            {
                return _innerContext.ExpectedTablePart(rowIndex, fieldIndex);
            }

            /// <summary>
            /// Проверка вызовов выполнения методов объекта запроса 1С.
            /// </summary>
            /// <param name="command">
            /// Команда, через которую производился вызов 1С.
            /// </param>
            public void VerifyQueryExecute(OneSCommand command)
            {
                Contract.Requires<ArgumentNullException>(command != null);
                
                _innerContext.VerifyQueryExecute(command);
            }
        }

        /// <summary>
        /// Начальное состояние описания теста.
        /// </summary>
        protected new sealed class InitBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public InitBuilderState(QueryTestsBase.InitBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                
                _innerState = innerState;
            }

            /// <summary>Описание SQL-запроса, выполняемого в тесте.</summary>
            /// <param name="sql">SQL-запрос.</param>
            public ExplicitSqlDefinedBuilderState Sql(string sql)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sql));
                Contract.Ensures(Contract.Result<ExplicitSqlDefinedBuilderState>() != null);

                return new ExplicitSqlDefinedBuilderState(_innerState, sql);
            }

            /// <summary>
            /// Описание источника данных 1С, к которому будет производиться запрос с выборкой всех
            /// полей описанных ожидаемыми данными.
            /// </summary>
            /// <param name="source">Имя источника данных.</param>
            public OnlySourceDefinedBuilderState Source(string source)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
                Contract.Ensures(Contract.Result<OnlySourceDefinedBuilderState>() != null);
                
                return new OnlySourceDefinedBuilderState(_innerState, source);
            }
        }

        /// <summary>
        /// Состояние явного описания SQL-запроса.
        /// </summary>
        protected sealed class ExplicitSqlDefinedBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly OneSCommand _command;

            public ExplicitSqlDefinedBuilderState(QueryTestsBase.InitBuilderState innerState, string sql)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sql));

                _innerState = innerState;
                _command = new OneSCommand { CommandText = sql };
            }

            /// <summary>Описание параметра SQL-запроса.</summary>
            /// <param name="name">Имя параметра.</param>
            /// <param name="value">Значение параметра.</param>
            public ExplicitSqlDefinedBuilderState Parameter(string name, object value)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
                Contract.Ensures(Contract.Result<ExplicitSqlDefinedBuilderState>() != null);

                _command.Parameters.Add(name, value);

                return this;
            }

            /// <summary>
            /// Описание выполнения запроса.
            /// </summary>
            public ActionDefiningBuilderState Execute(CommandBehavior commandBehavior = CommandBehavior.Default)
            {
                Contract.Ensures(Contract.Result<ActionDefiningBuilderState>() != null);

                return new ActionDefiningBuilderState(_innerState, ctx => _command, commandBehavior);
            }
        }

        /// <summary>
        /// Состояние описания источника данных 1С для запроса.
        /// </summary>
        protected sealed class OnlySourceDefinedBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly string _source;

            public OnlySourceDefinedBuilderState(QueryTestsBase.InitBuilderState innerState, string source)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));

                _innerState = innerState;
                _source = source;
            }

            /// <summary>
            /// Описание выполнения запроса.
            /// </summary>
            public ActionDefiningBuilderState Execute(CommandBehavior commandBehavior = CommandBehavior.Default)
            {
                Contract.Ensures(Contract.Result<ActionDefiningBuilderState>() != null);
                
                return new ActionDefiningBuilderState(_innerState, GetCommand, commandBehavior);
            }

            private OneSCommand GetCommand(QueryTestsBase.TestingContext testingContext0)
            {
                return new OneSCommand
                {
                    CommandText = GetSql(testingContext0)
                };
            }

            private string GetSql(QueryTestsBase.TestingContext testingContext0)
            {
                var fieldNames = Enumerable
                    .Range(0, testingContext0.ExpectedFieldsCount)
                    .Select(testingContext0.ExpectedFieldName)
                    .ToArray();

                var queryStringBuilder = new StringBuilder("ВЫБРАТЬ ");
                queryStringBuilder.Append(fieldNames.First());
                foreach (var field in fieldNames.Skip(1))
                {
                    queryStringBuilder.Append(", ");
                    queryStringBuilder.Append(field);
                }
                queryStringBuilder.Append(" ИЗ ");
                queryStringBuilder.Append(_source);

                return queryStringBuilder.ToString();
            }
        }

        /// <summary>Состояние описания тестирующего действия.</summary>
        protected sealed class ActionDefiningBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<QueryTestsBase.TestingContext, OneSCommand> _commandCreator;
            private readonly CommandBehavior _commandBehavior;

            public ActionDefiningBuilderState(
                QueryTestsBase.InitBuilderState innerState,
                Func<QueryTestsBase.TestingContext, OneSCommand> commandCreator,
                CommandBehavior commandBehavior)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                Contract.Requires<ArgumentNullException>(commandCreator != null);

                _innerState = innerState;
                _commandCreator = commandCreator;
                _commandBehavior = commandBehavior;
            }

            /// <summary>Описание тестирующего действия.</summary>
            /// <param name="testingAction">Тестирующее действие.</param>
            /// <returns></returns>
            public ActionDefinedBuilderState Action(Action<TestingContext> testingAction)
            {
                Contract.Requires<ArgumentNullException>(testingAction != null);
                Contract.Ensures(Contract.Result<ActionDefinedBuilderState>() != null);

                return _innerState.Action(ctx => TestingAction(ctx, testingAction));
            }

            /// <summary>Тестирующее действие.</summary>
            /// <param name="innerContext">Внутренний контекст.</param>
            /// <param name="testingActionBody">Тело тестирующего действия.</param>
            private void TestingAction(QueryTestsBase.TestingContext innerContext, Action<TestingContext> testingActionBody)
            {
                using (var testingReader = ExecuteReader(innerContext))
                {
                    var outerContext = new TestingContext(innerContext, testingReader);
                    testingActionBody(outerContext);
                }
            }

            private OneSDataReader ExecuteReader(QueryTestsBase.TestingContext innerContext)
            {
                var command = _commandCreator(innerContext);
                command.Connection = innerContext.Connection;

                command.Prepare();

                var reader = command.ExecuteReader(_commandBehavior);
                try
                {
                    innerContext.VerifyQueryExecute(command);
                }
                catch
                {
                    reader.Dispose();
                    throw;
                }

                return reader;
            }
        }

        #endregion
    }
}
