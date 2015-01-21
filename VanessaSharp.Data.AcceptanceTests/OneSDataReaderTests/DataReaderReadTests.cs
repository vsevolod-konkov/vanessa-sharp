using System;
using System.Collections;
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

        /// <summary>
        /// Тестирование выполнения запроса с табличной частью.
        /// </summary>
        [Test]
        [Ignore("Пока не полной реализации VNS_SHRP-33")]
        public void TestQueryWithTablePart()
        {
            Test
                .Sql("ВЫБРАТЬ Наименование, Сумма, Состав ИЗ Справочник.СправочникСТабличнойЧастью")
                
                .Execute()

                .BeginDefineTypedFieldReaders
                    
                    [(r, i) => r.GetString(i)]
                    [(r, i) => r.GetDecimal(i)]
                    
                    .BeginTablePart
                        [(r, i) => r.GetString(i)]
                        [(r, i) => r.GetDecimal(i)]
                        [(r, i) => r.GetInt32(i)]
                    .EndTablePart

                .EndDefineTypedFieldReaders

                .BeginDefineExpectedDataFor<ExpectedWithTablePartDictionary>()
                    
                    .Field(d => d.Name) 
                    .Field(d => d.Summa)
                    
                    .BeginTablePartField(d => d.Composition)
                        .Field(d => d.Name)
                        .Field(d => d.Price)
                        .Field(d => d.Quantity)
                    .EndTablePartField

                    .AllRows
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
            private readonly List<IList<Func<OneSDataReader, int, object>>> _tablePartTypedReaders = new List<IList<Func<OneSDataReader, int, object>>>();

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

                    // TODO: Refactor
                    _typedReaders.Add(typedReader);
                    _tablePartTypedReaders.Add(null);

                    return this;
                }
            }

            /// <summary>
            /// Начало описания типизированных читателей полей табличной части.
            /// </summary>
            public DefiningTablePartFieldTypedReadersBuilderState BeginTablePart
            {
                get
                {
                    Contract.Ensures(Contract.Result<DefiningTablePartFieldTypedReadersBuilderState>() != null);
                    
                    _typedReaders.Add(null);
                    var tablePartTypedReaders = new List<Func<OneSDataReader, int, object>>();
                    _tablePartTypedReaders.Add(tablePartTypedReaders);

                    return new DefiningTablePartFieldTypedReadersBuilderState(this, tablePartTypedReaders);
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
                    Assert.AreEqual(fieldIndex, reader.GetOrdinal(ctx.ExpectedFieldName(fieldIndex)));

                    // TODO: Refactor
                    Assert.AreEqual(
                        ctx.IsFieldTablePart(fieldIndex) ? typeof(OneSDataReader) : ctx.ExpectedFieldType(fieldIndex),
                        reader.GetFieldType(fieldIndex));
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
                        // TODO Refactor
                        if (ctx.IsFieldTablePart(fieldIndex))
                        {
                            // Тестирование сырых значений
                            Assert.IsInstanceOf<OneSDataReader>(values[fieldIndex]);
                            Assert.IsInstanceOf<OneSDataReader>(reader[fieldIndex]);
                            Assert.IsInstanceOf<OneSDataReader>(reader.GetValue(fieldIndex));
                            Assert.IsInstanceOf<OneSDataReader>(reader[ctx.ExpectedFieldName(fieldIndex)]);

                            var tablePartExpectedData = ctx.ExpectedTablePart(recordCounter, fieldIndex);
                            var tablePartValues = new object[tablePartExpectedData.Fields.Count];
                            var tablePartTypedReaders = _tablePartTypedReaders[fieldIndex];

                            using (var tablePartReader = reader.GetDataReader(fieldIndex))
                            {
                                // Тестирование читателя табличного поля

                                // Тестирование атрибутов читателя
                                Assert.AreEqual(1, tablePartReader.Depth);

                                Assert.AreEqual(tablePartExpectedData.Rows.Count > 0, tablePartReader.HasRows);
                                Assert.IsFalse(tablePartReader.IsClosed);
                                Assert.AreEqual(-1, tablePartReader.RecordsAffected);

                                // Тестирование описания полей читателя
                                Assert.AreEqual(tablePartExpectedData.Fields.Count, tablePartReader.FieldCount);
                                Assert.AreEqual(tablePartExpectedData.Fields.Count, tablePartReader.VisibleFieldCount);

                                for (var tablePartFieldIndex = 0; tablePartFieldIndex < tablePartExpectedData.Fields.Count; tablePartFieldIndex++)
                                {
                                    Assert.AreEqual(tablePartExpectedData.Fields[tablePartFieldIndex].Name, tablePartReader.GetName(tablePartFieldIndex));
                                    Assert.AreEqual(tablePartFieldIndex, tablePartReader.GetOrdinal(ctx.ExpectedFieldName(tablePartFieldIndex)));

                                    // TODO: Refactor
                                    Assert.AreEqual(
                                       tablePartExpectedData.Fields[tablePartFieldIndex].Type,
                                       tablePartReader.GetFieldType(tablePartFieldIndex));
                                }

                                var tablePartRecordCounter = 0;

                                // TODO: Refactor
                                while (tablePartReader.Read())
                                {
                                    Assert.Less(tablePartRecordCounter, tablePartExpectedData.Rows.Count);
                                    Assert.AreEqual(tablePartExpectedData.Fields.Count, tablePartReader.GetValues(tablePartValues));

                                    for (var tablePartFieldIndex = 0; tablePartFieldIndex < tablePartExpectedData.Fields.Count; tablePartFieldIndex++)
                                    {
                                        var expectedFieldValue = tablePartExpectedData.Rows[recordCounter][tablePartFieldIndex];
                                        var rawExpectedFieldValue = QueryResultMockFactory.GetOneSRawValue(expectedFieldValue);

                                        // Тестирование сырых значений
                                        Assert.AreEqual(rawExpectedFieldValue, tablePartValues[tablePartFieldIndex]);
                                        Assert.AreEqual(rawExpectedFieldValue, tablePartReader[tablePartFieldIndex]);
                                        Assert.AreEqual(rawExpectedFieldValue, tablePartReader.GetValue(tablePartFieldIndex));
                                        Assert.AreEqual(rawExpectedFieldValue, tablePartReader[tablePartExpectedData.Fields[tablePartFieldIndex].Name]);

                                        // Тестирование типизированного значения
                                        Assert.AreEqual(expectedFieldValue, tablePartTypedReaders[tablePartFieldIndex](tablePartReader, tablePartFieldIndex));    
                                    }

                                    tablePartRecordCounter++;
                                }

                                Assert.AreEqual(tablePartExpectedData.Rows.Count, tablePartRecordCounter);
                            }
                        }
                        else
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
                    }

                    ++recordCounter;
                }

                Assert.AreEqual(ctx.ExpectedRowsCount, recordCounter);
            }
        }

        /// <summary>
        /// Состояние описания типизированных читателей полей табличной части.
        /// </summary>
        private sealed class DefiningTablePartFieldTypedReadersBuilderState
        {
            private readonly DefiningFieldTypedReadersBuilderState _prevState;
            private readonly IList<Func<OneSDataReader, int, object>> _typedReaders;

            public DefiningTablePartFieldTypedReadersBuilderState(DefiningFieldTypedReadersBuilderState prevState, IList<Func<OneSDataReader, int, object>> typedReaders)
            {
                Contract.Requires<ArgumentNullException>(prevState != null);

                _prevState = prevState;
                _typedReaders = typedReaders;
            }
            
            /// <summary>
            /// Добавление типизированного читателя поля.
            /// </summary>
            /// <param name="typedReader">Типизированный читатель поля.</param>
            public DefiningTablePartFieldTypedReadersBuilderState this[Func<OneSDataReader, int, object> typedReader]
            {
                get
                {
                    Contract.Requires<ArgumentNullException>(typedReader != null);
                    Contract.Ensures(Contract.Result<DefiningTablePartFieldTypedReadersBuilderState>() != null);

                    _typedReaders.Add(typedReader);

                    return this;
                }
            }

            /// <summary>
            /// Завершение описания читателей полей табличной части.
            /// </summary>
            public DefiningFieldTypedReadersBuilderState EndTablePart
            {
                get
                {
                    Contract.Ensures(Contract.Result<DefiningFieldTypedReadersBuilderState>() != null);

                    return _prevState;
                }
            }
        }

        #endregion
    }
}
