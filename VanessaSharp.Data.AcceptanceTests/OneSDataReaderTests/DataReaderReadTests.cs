using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public void TestQueryWithTablePart()
        {
            Test
                .Sql("ВЫБРАТЬ Наименование, Сумма, Состав ИЗ Справочник.СправочникСТабличнойЧастью")
                
                .Execute()

                .BeginDefineTypedFieldReaders
                    
                    [(r, i) => r.GetString(i)]
                    [(r, i) => r.GetDecimal(i)]
                    
                    .BeginTablePart
                        
                        .Any
                        .Any
                        [(r, i) => r.GetString(i)]
                        [(r, i) => r.GetDecimal(i)]
                        [(r, i) => r.GetInt32(i)]

                    .EndTablePart

                .EndDefineTypedFieldReaders

                .BeginDefineExpectedDataFor<ExpectedWithTablePartDictionary>()
                    
                    .Field(d => d.Name) 
                    .Field(d => d.Summa)

                    .BeginTablePartField(d => d.Composition)
                        
                        .AnyField("Ссылка")
                        .AnyField("НомерСтроки")

                        .Field(d => d.Name)
                        .Field(d => d.Price)
                        .Field(d => d.Quantity)

                    .EndTablePartField

                    .AllRows
                .EndDefineExpectedData

            .Run();
        }

        #region Методы тестирования

        /// <summary>Тестирующее действие.</summary>
        /// <param name="ctx">Контекст тестирования.</param>
        private static void TestingAction(TestingContext ctx)
        {
            TestingAction(ctx, ctx.TestedReader);
        }

        private static void TestingAction(ITestingContext ctx, OneSDataReader reader)
        {
            // Тестирование атрибутов читателя
            TestReaderAttributes(reader, ctx.HasData, ctx.IsTablePart);

            // Тестирование описания полей читателя
            TestFields(ctx, reader);

            // буфер для чтения данных строк
            var values = new object[ctx.ExpectedFieldsCount];

            var recordCounter = 0;

            while (reader.Read())
            {
                Assert.Less(recordCounter, ctx.ExpectedRecordsCount);
                Assert.AreEqual(0, reader.Level);
                Assert.AreEqual(ctx.IsTablePart ? 1 : 0, reader.Depth);

                Assert.AreEqual(ctx.ExpectedFieldsCount, reader.GetValues(values));

                var recordCtx = ctx.GetRecordContext(recordCounter);

                for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                {
                    if (!recordCtx.IsAnyField(fieldIndex))
                    {
                        var valueCtx = recordCtx.GetValueContext(fieldIndex);

                        if (valueCtx.ExpectedFieldKind == FieldKind.TablePart)
                        {
                            Assert.IsInstanceOf<OneSDataReader>(values[fieldIndex]);
                            Assert.IsInstanceOf<OneSDataReader>(reader[fieldIndex]);
                            Assert.IsInstanceOf<OneSDataReader>(reader.GetValue(fieldIndex));
                            Assert.IsInstanceOf<OneSDataReader>(reader[valueCtx.ExpectedFieldName]);

                            using (var tablePartReader = reader.GetDataReader(fieldIndex))
                            {
                                TestingAction((ITestingTablePartContext)valueCtx, tablePartReader);
                            }
                        }
                        else
                        {
                            TestFieldValue((ITestingScalarValueContext)valueCtx, values, reader);
                        }    
                    }
                }

                ++recordCounter;
            }

            Assert.AreEqual(ctx.ExpectedRecordsCount, recordCounter);
        }

        /// <summary>Тестирование атрибутов читателя.</summary>
        private static void TestReaderAttributes(OneSDataReader testedReader, bool hasData, bool isTablePart)
        {
            Assert.AreEqual(hasData, testedReader.HasRows);
            Assert.IsFalse(testedReader.IsClosed);
            Assert.AreEqual(-1, testedReader.RecordsAffected);
            Assert.AreEqual(isTablePart, testedReader.IsTablePart);
        }

        /// <summary>Тестирование полей читателя.</summary>
        private static void TestFields(ITestingContext ctx, OneSDataReader testedReader)
        {
            Assert.AreEqual(ctx.ExpectedFieldsCount, testedReader.FieldCount);
            Assert.AreEqual(ctx.ExpectedFieldsCount, testedReader.VisibleFieldCount);

            for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
            {
                TestField(ctx.GetFieldContext(fieldIndex), testedReader);
            }
        }

        /// <summary>Тестирование поля читателя.</summary>
        private static void TestField(ITestingFieldContext ctx, OneSDataReader testedReader)
        {
            Assert.AreEqual(ctx.ExpectedFieldName, testedReader.GetName(ctx.FieldIndex));
            Assert.AreEqual(ctx.FieldIndex, testedReader.GetOrdinal(ctx.ExpectedFieldName));

            if (ctx.ExpectedFieldKind != FieldKind.Any)
            {
                var expectedFieldType = (ctx.ExpectedFieldKind == FieldKind.TablePart)
                                            ? typeof(OneSDataReader)
                                            : ((ITestingScalarFieldContext)ctx).ExpectedFieldType;

                Assert.AreEqual(
                    expectedFieldType,
                    testedReader.GetFieldType(ctx.FieldIndex));
            }
        }

        /// <summary>Тестирование значения поля записи.</summary>
        private static void TestFieldValue(ITestingScalarValueContext ctx, object[] values, OneSDataReader testedReader)
        {
            if (ctx.ExpectedValue is AnyType)
                return;
            
            var rawExpectedFieldValue = QueryResultMockFactory.GetOneSRawValue(ctx.ExpectedValue);

            // Тестирование сырых значений
            Assert.AreEqual(rawExpectedFieldValue, values[ctx.FieldIndex]);
            Assert.AreEqual(rawExpectedFieldValue, testedReader[ctx.FieldIndex]);
            Assert.AreEqual(rawExpectedFieldValue, testedReader.GetValue(ctx.FieldIndex));
            Assert.AreEqual(rawExpectedFieldValue, testedReader[ctx.ExpectedFieldName]);

            // Тестирование типизированного значения
            Assert.AreEqual(ctx.ExpectedValue, ctx.TypedReader(testedReader));    
        }
        
        private interface ITestingContext
        {
            int ExpectedFieldsCount { get; }

            int ExpectedRecordsCount { get; }

            ITestingFieldContext GetFieldContext(int fieldIndex);

            ITestingRecordContext GetRecordContext(int recordIndex);

            bool IsTablePart { get; }

            bool HasData { get; }
        }

        private interface ITestingFieldContext
        {
            int FieldIndex { get; }

            string ExpectedFieldName { get; }
            FieldKind ExpectedFieldKind { get; }
        }

        private interface ITestingScalarFieldContext : ITestingFieldContext
        {
            Type ExpectedFieldType { get; }
        }

        private interface ITestingRecordContext
        {
            bool IsAnyField(int fieldIndex);
            
            ITestingValueContext GetValueContext(int fieldIndex);
        }

        private interface ITestingValueContext
        {
            int FieldIndex { get; }
            string ExpectedFieldName { get; }
            FieldKind ExpectedFieldKind { get; }
        }

        private interface ITestingScalarValueContext : ITestingValueContext
        {
            object ExpectedValue { get; }
            Func<OneSDataReader, object> TypedReader { get; }
        }

        private interface ITestingTablePartContext 
            : ITestingValueContext, ITestingContext
        {}

        #endregion

        #region Объекты для внутреннего DSL

        private abstract class TypedFieldValueReader
        {
            public abstract FieldKind Kind { get; }

            public static TypedFieldValueReader Any
            {
                get { return _any; }
            }
            private static readonly TypedFieldValueReader _any = new AnyFieldValueReader();
        }

        private sealed class AnyFieldValueReader : TypedFieldValueReader
        {
            public override FieldKind Kind
            {
                get { return FieldKind.Any; }
            }
        }

        private sealed class TypedScalarFieldValueReader : TypedFieldValueReader
        {
            public TypedScalarFieldValueReader(Func<OneSDataReader, int, object> scalarReader)
            {
                Contract.Requires<ArgumentNullException>(scalarReader != null);
                
                _scalarReader = scalarReader;
            }
            
            public override FieldKind Kind
            {
                get { return FieldKind.Scalar; }
            }

            public Func<OneSDataReader, int, object> ScalarReader
            {
                get { return _scalarReader; }
            }
            private readonly Func<OneSDataReader, int, object> _scalarReader;
        }

        private sealed class TypedTablePartFieldReader : TypedFieldValueReader
        {
            public TypedTablePartFieldReader(IList<TypedFieldValueReader> readers)
            {
                Contract.Requires<ArgumentNullException>(readers != null);

                _readers = readers;
            }

            public override FieldKind Kind
            {
                get { return FieldKind.TablePart; }
            }

            public IList<TypedFieldValueReader> Readers
            {
                get { return _readers; }
            }
            private readonly IList<TypedFieldValueReader> _readers;
        }

        private new sealed class TestingContext : DataReaderTestsBase.TestingContext, ITestingContext
        {
            private readonly IList<TypedFieldValueReader> _typedReaders;
            
            public TestingContext(DataReaderTestsBase.TestingContext innerContext,
                IList<TypedFieldValueReader> typedReaders)
                : base(innerContext)
            {
                Contract.Requires<ArgumentNullException>(innerContext != null);
                Contract.Requires<ArgumentNullException>(typedReaders != null);

                _typedReaders = typedReaders;
            }

            public int ExpectedRecordsCount 
            { 
                get { return ExpectedData.Rows.Count; } 
            }

            public ITestingFieldContext GetFieldContext(int fieldIndex)
            {
                return TestingFieldContext.Create(
                    fieldIndex,
                    ExpectedData.Fields[fieldIndex]);
            }

            public ITestingRecordContext GetRecordContext(int recordIndex)
            {
                var rowIndex = ExpectedRowIndexes[recordIndex];

                return new TestingRecordContext(
                    ExpectedData.Fields,
                    _typedReaders,
                    ExpectedData.Rows[rowIndex]
                    );
            }

            public bool IsTablePart { get { return false; } }

            public bool HasData { get { return ExpectedRowsCount > 0; } }
        }

        private class TestingFieldContext : ITestingFieldContext
        {
            private readonly FieldDescription _fieldDescription;

            public static TestingFieldContext Create(int fieldIndex, FieldDescription fieldDescription)
            {
                return (fieldDescription.Kind == FieldKind.Scalar)
                           ? new TestingScalarFieldContext(fieldIndex, (ScalarFieldDescription)fieldDescription)
                           : new TestingFieldContext(fieldIndex, fieldDescription);
            }

            protected TestingFieldContext(int fieldIndex, FieldDescription fieldDescription)
            {
                Contract.Requires<ArgumentNullException>(fieldDescription != null);
                
                _fieldIndex = fieldIndex;
                _fieldDescription = fieldDescription;
            }

            public int FieldIndex
            {
                get { return _fieldIndex; }
            }
            private readonly int _fieldIndex;

            public FieldKind ExpectedFieldKind
            {
                get { return _fieldDescription.Kind; }
            }

            public string ExpectedFieldName
            {
                get { return _fieldDescription.Name; }
            }
        }

        private sealed class TestingScalarFieldContext : TestingFieldContext, ITestingScalarFieldContext
        {
            private readonly ScalarFieldDescription _fieldDescription;
            
            public TestingScalarFieldContext(int fieldIndex, ScalarFieldDescription fieldDescription) 
                : base(fieldIndex, fieldDescription)
            {
                Contract.Requires<ArgumentNullException>(fieldDescription != null);

                _fieldDescription = fieldDescription;
            }

            public Type ExpectedFieldType
            {
                get { return _fieldDescription.Type; }
            }
        }

        private sealed class TestingRecordContext : ITestingRecordContext
        {
            private readonly ReadOnlyCollection<object> _expectedRow;
            private readonly ReadOnlyCollection<FieldDescription> _expectedFields;
            private readonly IList<TypedFieldValueReader> _typedReaders;

            public TestingRecordContext(
                ReadOnlyCollection<FieldDescription> expectedFields,
                IList<TypedFieldValueReader> typedReaders,
                ReadOnlyCollection<object> expectedRow)
            {
                Contract.Requires<ArgumentNullException>(expectedFields != null);
                Contract.Requires<ArgumentNullException>(typedReaders != null);
                Contract.Requires<ArgumentNullException>(expectedRow != null);
                
                _expectedFields = expectedFields;
                _typedReaders = typedReaders;
                _expectedRow = expectedRow;
            }

            public bool IsAnyField(int fieldIndex)
            {
                return _expectedFields[fieldIndex].Kind == FieldKind.Any;
            }

            public ITestingValueContext GetValueContext(int fieldIndex)
            {
                var expectedField = _expectedFields[fieldIndex];
                var expectedFieldName = expectedField.Name;
                var expectedValue = _expectedRow[fieldIndex];

                var fieldKind = _expectedFields[fieldIndex].Kind;
                var reader = _typedReaders[fieldIndex];
                Contract.Assert(reader.Kind == fieldKind);

                switch (fieldKind)
                {
                    case FieldKind.Scalar:
                        return new TestingScalarValueContext(
                                fieldIndex,
                                expectedFieldName,
                                expectedValue,
                                ((TypedScalarFieldValueReader)reader).ScalarReader
                            );

                    case FieldKind.TablePart:
                        return new TestingTablePartContext(
                                fieldIndex,
                                expectedFieldName,
                                (TableData)expectedValue,
                                ((TypedTablePartFieldReader)reader).Readers
                            );

                    default:
                        throw new NotSupportedException(string.Format(
                            "Field Kind \"{0}\" is not supported.",
                            fieldKind));
                }
            }
        }

        private abstract class TestingValueContextBase : ITestingValueContext
        {
            protected TestingValueContextBase(int fieldIndex, string expectedFieldName)
            {
                _fieldIndex = fieldIndex;
                _expectedFieldName = expectedFieldName;
            }

            public int FieldIndex { get { return _fieldIndex; } }
            private readonly int _fieldIndex;

            public string ExpectedFieldName { get { return _expectedFieldName; } }
            private readonly string _expectedFieldName;

            public abstract FieldKind ExpectedFieldKind { get; }
        }

        private sealed class TestingScalarValueContext : TestingValueContextBase, ITestingScalarValueContext
        {
            public TestingScalarValueContext(int fieldIndex, string expectedFieldName, 
                object expectedValue, Func<OneSDataReader, int, object> typedReader)
                : base(fieldIndex, expectedFieldName)
            {
                Contract.Requires<ArgumentNullException>(typedReader != null);
                
                _expectedValue = expectedValue;
                _typedReader = typedReader;
            }
            
            public object ExpectedValue { get { return _expectedValue; } }
            private readonly object _expectedValue;

            public override FieldKind ExpectedFieldKind
            {
                get { return FieldKind.Scalar; }
            }

            public Func<OneSDataReader, object> TypedReader
            {
                get
                {
                    return r => _typedReader(r, FieldIndex);
                }
            }
            private readonly Func<OneSDataReader, int, object> _typedReader;
        }

        private sealed class TestingTablePartContext : TestingValueContextBase, ITestingTablePartContext
        {
            private readonly TableData _expectedData;
            private readonly IList<TypedFieldValueReader> _typedReaders; 
            
            public TestingTablePartContext(
                int fieldIndex,
                string expectedFieldName,
                TableData expectedData,
                IList<TypedFieldValueReader> typedReaders)
                : base(fieldIndex, expectedFieldName)
            {
                Contract.Requires<ArgumentNullException>(expectedData != null);
                Contract.Requires<ArgumentNullException>(typedReaders != null);

                _expectedData = expectedData;
                _typedReaders = typedReaders;
            }

            public override FieldKind ExpectedFieldKind
            {
                get { return FieldKind.TablePart; }
            }

            public bool HasData
            {
                get
                {
                    return _expectedData.Rows.Count > 0;
                }
            }

            public int ExpectedFieldsCount
            {
                get { return _expectedData.Fields.Count; }
            }

            public int ExpectedRecordsCount
            {
                get { return _expectedData.Rows.Count; }
            }

            public ITestingFieldContext GetFieldContext(int fieldIndex)
            {
                return TestingFieldContext.Create(fieldIndex, _expectedData.Fields[fieldIndex]);
            }

            public ITestingRecordContext GetRecordContext(int recordIndex)
            {
                return new TestingRecordContext(
                    _expectedData.Fields, _typedReaders, 
                    _expectedData.Rows[recordIndex]
                    );
            }

            bool ITestingContext.IsTablePart
            {
                get { return true; }
            }
        }

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
            private readonly List<TypedFieldValueReader> _typedReaders = new List<TypedFieldValueReader>();

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

                    _typedReaders.Add(new TypedScalarFieldValueReader(typedReader));

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

                    var tablePartTypedReaders = new List<TypedFieldValueReader>();

                    _typedReaders.Add(new TypedTablePartFieldReader(tablePartTypedReaders));
                    
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
            private void TestingAction(DataReaderTestsBase.TestingContext ctx)
            {
                var newCtx = new TestingContext(ctx, _typedReaders);

                DataReaderReadTests.TestingAction(newCtx);
            }
        }

        /// <summary>
        /// Состояние описания типизированных читателей полей табличной части.
        /// </summary>
        private sealed class DefiningTablePartFieldTypedReadersBuilderState
        {
            private readonly DefiningFieldTypedReadersBuilderState _prevState;
            private readonly IList<TypedFieldValueReader> _typedReaders;

            public DefiningTablePartFieldTypedReadersBuilderState(DefiningFieldTypedReadersBuilderState prevState, IList<TypedFieldValueReader> typedReaders)
            {
                Contract.Requires<ArgumentNullException>(prevState != null);

                _prevState = prevState;
                _typedReaders = typedReaders;
            }

            public DefiningTablePartFieldTypedReadersBuilderState Any
            {
                get
                {
                    _typedReaders.Add(TypedFieldValueReader.Any);

                    return this;
                }
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

                    _typedReaders.Add(new TypedScalarFieldValueReader(typedReader));

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
