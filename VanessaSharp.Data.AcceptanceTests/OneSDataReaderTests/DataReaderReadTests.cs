using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;
using VanessaSharp.AcceptanceTests.Utility.Mocks;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    [TestFixture(TestMode.Real)]
    [TestFixture(TestMode.Isolated)]
    public sealed class DataReaderReadTests : DataReaderTestsBase
    {
        public DataReaderReadTests(TestMode testMode) 
            : base(testMode)
        {}

        /// <summary>Начало описания теста.</summary>
        private new InitBuilderState Test
        {
            get {  return new InitBuilderState(base.Test); }
        }

        /// <summary>Тестирование простого запроса.</summary>
        [Test]
        public void TestSimpleQuery()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                
                .Execute(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult)
                
                .BeginDefineTypedFieldReaders
                    [(r, i) => r.GetString(i)]
                    [(r, i) => r.GetInt32(i)]
                    [(r, i) => r.GetDouble(i)]
                    [(r, i) => r.GetBoolean(i)]
                    [(r, i) => r.GetDateTime(i)]
                    [(r, i) => r.GetDateTime(i)]
                    [(r, i) => r.GetDateTime(i)]
                    [(r, i) => r.GetString(i)]
                    [(r, i) => r.GetChar(i)]
                .EndDefineTypedFieldReaders


                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                
                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField)

                    .AllRows

                .EndDefineExpectedData
            
            .Run();
        }

        /// <summary>
        /// Тестирование выполнения параметрического запроса.
        /// </summary>
        [Test]
        public void TestSimpleParameterizedQuery()
        {
            Test
                .Sql("ВЫБРАТЬ СтроковоеПоле, ЦелочисленноеПоле ИЗ Справочник.ТестовыйСправочник ГДЕ СтроковоеПоле = &Фильтр")
                .Parameter("Фильтр", "Тестирование")

                .Execute(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult)
                
                .BeginDefineTypedFieldReaders
                    [(r, i) => r.GetString(i)]
                    [(r, i) => r.GetInt32(i)]
                .EndDefineTypedFieldReaders

                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                    .Field(d => d.StringField)
                    .Field(d => d.IntField)

                    .Rows(0)
                .EndDefineExpectedData
                
            .Run();
        }

        #region Объекты для внутреннего DSL

        /// <summary>
        /// Начальное состояние для описания теста.
        /// </summary>
        private new sealed class InitBuilderState
        {
            private readonly DataReaderTestsBase.InitBuilderState _innerState;

            /// <summary>Конструктор.</summary>
            /// <param name="innerState">Внутреннее состояние.</param>
            public InitBuilderState(DataReaderTestsBase.InitBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);

                _innerState = innerState;
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
                
                return new OnlySourceDefinedBuilderState(_innerState.Source(source));
            }

            /// <summary>Описание SQL-запроса, выполняемого в тесте.</summary>
            /// <param name="sql">SQL-запрос.</param>
            public ExplicitSqlDefinedBuilderState Sql(string sql)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sql));
                Contract.Ensures(Contract.Result<ExplicitSqlDefinedBuilderState>() != null);
                
                return new ExplicitSqlDefinedBuilderState(_innerState.Sql(sql));
            }
        }

        /// <summary>
        /// Состояние описания источника данных 1С для запроса.
        /// </summary>
        private new sealed class OnlySourceDefinedBuilderState
        {
            private readonly DataReaderTestsBase.OnlySourceDefinedBuilderState _innerState;

            public OnlySourceDefinedBuilderState(DataReaderTestsBase.OnlySourceDefinedBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                
                _innerState = innerState;
            }

            /// <summary>
            /// Описание выполнения запроса.
            /// </summary>
            public BeforeDefiningFieldTypedReadersBuilderState Execute(CommandBehavior commandBehavior = CommandBehavior.Default)
            {
                Contract.Ensures(Contract.Result<BeforeDefiningFieldTypedReadersBuilderState>() != null);

                return new BeforeDefiningFieldTypedReadersBuilderState(_innerState.Execute(commandBehavior));
            }
        }

        /// <summary>
        /// Состояние явного описания SQL-запроса.
        /// </summary>
        private new sealed class ExplicitSqlDefinedBuilderState
        {
            private DataReaderTestsBase.ExplicitSqlDefinedBuilderState _innerState;

            public ExplicitSqlDefinedBuilderState(DataReaderTestsBase.ExplicitSqlDefinedBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                
                _innerState = innerState;
            }

            /// <summary>Описание параметра SQL-запроса.</summary>
            /// <param name="name">Имя параметра.</param>
            /// <param name="value">Значение параметра.</param>
            public ExplicitSqlDefinedBuilderState Parameter(string name, object value)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
                Contract.Ensures(Contract.Result<ExplicitSqlDefinedBuilderState>() != null);
                
                _innerState = _innerState.Parameter(name, value);

                return this;
            }

            /// <summary>
            /// Описание выполнения запроса.
            /// </summary>
            public BeforeDefiningFieldTypedReadersBuilderState Execute(CommandBehavior commandBehavior = CommandBehavior.Default)
            {
                Contract.Ensures(Contract.Result<BeforeDefiningFieldTypedReadersBuilderState>() != null);
                
                return new BeforeDefiningFieldTypedReadersBuilderState(_innerState.Execute(commandBehavior));
            }
        }

        /// <summary>Состояние перед началом описания действия.</summary>
        private sealed class BeforeDefiningFieldTypedReadersBuilderState
        {
            private readonly ActionDefiningBuilderState _innerState;

            public BeforeDefiningFieldTypedReadersBuilderState(ActionDefiningBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                
                _innerState = innerState;
            }

            /// <summary>Начало описания типизированных читателей значения полей.</summary>
            public DefiningFieldTypedReadersBuilderState BeginDefineTypedFieldReaders
            {
                get
                {
                    Contract.Ensures(Contract.Result<DefiningFieldTypedReadersBuilderState>() != null);

                    return new DefiningFieldTypedReadersBuilderState(_innerState);
                }
            }
        }

        /// <summary>
        /// Состояние описания типизированных читателей полей.
        /// </summary>
        private sealed class DefiningFieldTypedReadersBuilderState
        {
            private readonly ActionDefiningBuilderState _innerState;
            private readonly List<Func<OneSDataReader, int, object>> _typedReaders = new List<Func<OneSDataReader, int, object>>();

            public DefiningFieldTypedReadersBuilderState(ActionDefiningBuilderState innerState)
            {
                Contract.Requires<ArgumentNullException>(innerState != null);
                
                _innerState = innerState;
            }

            /// <summary>
            /// Добавление типизированного читателя поля.
            /// </summary>
            /// <param name="typedReader">Типизированный читатель поля.</param>
            /// <returns></returns>
            public DefiningFieldTypedReadersBuilderState this[Func<OneSDataReader, int, object> typedReader]
            {
                get
                {
                    Contract.Requires<ArgumentNullException>(typedReader != null);
                    Contract.Ensures(Contract.Result<DefiningFieldTypedReadersBuilderState>() != null);

                    _typedReaders.Add(typedReader);

                    return this;
                }
            }

            /// <summary>Завершение описания типизированных читателей полей.</summary>
            public ActionDefinedBuilderState EndDefineTypedFieldReaders
            {
                get
                {
                    Contract.Ensures(Contract.Result<QueryTestsBase.ActionDefinedBuilderState>() != null);
                    
                    return _innerState.Action(TestingAction);
                }
            }

            /// <summary>Тестирующие действие.</summary>
            /// <param name="ctx">Контекст тестирования.</param>
            private void TestingAction(TestingContext ctx)
            {
                var reader = ctx.TestedReader;

                // Тестирование атрибутов читателя
                Assert.AreEqual(0, reader.Depth);
                Assert.IsTrue(reader.HasRows);
                Assert.IsFalse(reader.IsClosed);
                Assert.AreEqual(-1, reader.RecordsAffected);

                // Тестирование описания полей читателя
                Assert.AreEqual(ctx.ExpectedFieldsCount, reader.FieldCount);
                Assert.AreEqual(ctx.ExpectedFieldsCount, reader.VisibleFieldCount);

                for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                {
                    Assert.AreEqual(ctx.ExpectedFieldName(fieldIndex), reader.GetName(fieldIndex));
                    Assert.AreEqual(ctx.ExpectedFieldType(fieldIndex), reader.GetFieldType(fieldIndex));
                    Assert.AreEqual(fieldIndex, reader.GetOrdinal(ctx.ExpectedFieldName(fieldIndex)));
                }

                // буфер для чтения данных строк
                var values = new object[ctx.ExpectedFieldsCount];

                var recordCounter = 0;

                while (reader.Read())
                {
                    Assert.Less(recordCounter, ctx.ExpectedRowsCount);

                    Assert.AreEqual(ctx.ExpectedFieldsCount, reader.GetValues(values));

                    for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                    {
                        var expectedFieldValue = ctx.ExpectedValue(recordCounter, fieldIndex);
                        var rawExpectedFieldValue = QueryResultMockFactory.GetOneSRawValue(expectedFieldValue);

                        // Тестирование сырых значений
                        Assert.AreEqual(rawExpectedFieldValue, values[fieldIndex]);
                        Assert.AreEqual(rawExpectedFieldValue, reader[fieldIndex]);
                        Assert.AreEqual(rawExpectedFieldValue, reader.GetValue(fieldIndex));
                        Assert.AreEqual(rawExpectedFieldValue, reader[ctx.ExpectedFieldName(fieldIndex)]);

                        // Тестирование типизированного значения
                        Assert.AreEqual(expectedFieldValue, _typedReaders[fieldIndex](reader, fieldIndex));
                    }

                    ++recordCounter;
                }

                Assert.AreEqual(ctx.ExpectedRowsCount, recordCounter);
            }
        }

        #endregion
    }
}
