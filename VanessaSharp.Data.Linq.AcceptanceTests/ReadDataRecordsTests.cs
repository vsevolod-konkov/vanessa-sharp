using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;
using VanessaSharp.AcceptanceTests.Utility.Mocks;
using VanessaSharp.Data.Linq.PredefinedData;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>
    /// Тестирование LINQ-запросов для вычитки записей <see cref="OneSDataRecord"/>.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public sealed class ReadDataRecordsTests : QueryTestsBase
    {
        public ReadDataRecordsTests(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        {}

        /// <summary>Получение типизированного читателя значения поля по индексу.</summary>
        /// <param name="typedFieldValueGetters">Массив типизированных читателей полей по индексу.</param>
        private static Func<int, object> GetTypedFieldValueGetterById(params Func<int, object>[] typedFieldValueGetters)
        {
            return index =>
            {
                var getter = typedFieldValueGetters[index];
                if (getter == null)
                    throw new ArgumentOutOfRangeException("index");

                return getter(index);
            };
        }

        /// <summary>Получение типизированного читателя значения поля по наименованию поля.</summary>
        /// <param name="ctx">Контекст тестирования.</param>
        /// <param name="typedFieldValueGetters">Массив типизированных читателей полей по наименованию поля.</param>
        private static Func<int, object> GetTypedFieldValueGetterByName(TestingContext ctx, params Func<string, object>[] typedFieldValueGetters)
        {
            return index =>
            {
                var getter = typedFieldValueGetters[index];
                if (getter == null)
                    throw new ArgumentOutOfRangeException("index");

                return getter(ctx.ExpectedFieldName(index));
            };
        }

        /// <summary>Получение конвертера занчений по индекс поля.</summary>
        /// <param name="converters">Конвертеры</param>
        private static Func<int, OneSValue, object> GetConvertedValue(params Func<OneSValue, object>[] converters)
        {
            return (index, value) =>
                {
                    var conveter = converters[index];
                    if (conveter == null)
                        throw new ArgumentOutOfRangeException("index");

                    return conveter(value);
                };

        }

        /// <summary>Начало описания теста.</summary>
        private new InitBuilderState Test
        {
           get { return new InitBuilderState(base.Test); }
        }

        /// <summary>Тестирование чтения записей.</summary>
        /// <param name="ctx">Контекст тестирования.</param>
        /// <param name="queryAction">Действия получения тестового linq-запроса.</param>
        /// <param name="expectedSql">Ожидаемый SQL-запрос передаваемый в 1С.</param>
        /// <param name="expectedSqlParameters">Ожидаемые параметры SQL-запроса передаваемые в 1С.</param>
        private static void TestReadRecords(TestingContext ctx,
                                            Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction,
                                            string expectedSql, Dictionary<string, object> expectedSqlParameters)
        {
            using (var dataContext = new OneSDataContext(ctx.Connection))
            {
                var records = queryAction(dataContext);

                var recordCounter = 0;

                foreach (var record in records)
                {
                    Assert.Less(recordCounter, ctx.ExpectedRowsCount);
                    
                    Assert.AreEqual(ctx.ExpectedFieldsCount, record.Fields.Count);

                    // Чтение сырых значений
                    var rawValues = new object[ctx.ExpectedFieldsCount];
                    Assert.AreEqual(ctx.ExpectedFieldsCount, record.GetValues(rawValues));

                    // Чтение обернутых значений
                    var oneSValues = new OneSValue[ctx.ExpectedFieldsCount];
                    Assert.AreEqual(ctx.ExpectedFieldsCount, record.GetValues(oneSValues));

                    // Тестирование чтения полей по отдельности
                    Func<int, object> getStringValueById = index => record.GetString(index);
                    Func<int, object> getDateTimeValueById = index => record.GetDateTime(index);

                    Func<string, object> getStringValueByName = name => record.GetString(name);
                    Func<string, object> getDateTimeValueByName = name => record.GetDateTime(name);

                    var getTypedValueById = GetTypedFieldValueGetterById(
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        getStringValueById,
                        i => record.GetInt32(i),
                        i => record.GetDouble(i),
                        i => record.GetBoolean(i),
                        getDateTimeValueById,
                        getDateTimeValueById,
                        getDateTimeValueById,
                        getStringValueById,
                        i => record.GetChar(i)
                        );

                    var getTypedValueByName = GetTypedFieldValueGetterByName(ctx,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        getStringValueByName,
                        s => record.GetInt32(s),
                        s => record.GetDouble(s),
                        s => record.GetBoolean(s),
                        getDateTimeValueByName,
                        getDateTimeValueByName,
                        getDateTimeValueByName,
                        getStringValueByName,
                        s => record.GetChar(s)
                        );

                    var converter = GetConvertedValue(
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        v => (string)v,
                        v => (int)v,
                        v => (double)v,
                        v => (bool)v,
                        v => (DateTime)v,
                        v => (DateTime)v,
                        v => (DateTime)v,
                        v => (string)v,
                        v => (char)v
                        );


                    for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                    {
                        if (ctx.ExpectedFieldKind(fieldIndex) == FieldKind.Any)
                            continue;

                        var expectedFieldValue = ctx.ExpectedValue(recordCounter, fieldIndex);
                        var expectedRawFieldValue = QueryResultMockFactory.GetOneSRawValue(expectedFieldValue);
                        var expectedFieldName = ctx.ExpectedFieldName(fieldIndex);

                        // Сравнение сырых значений
                        Assert.AreEqual(expectedRawFieldValue, rawValues[fieldIndex]);
                        Assert.AreEqual(expectedRawFieldValue, oneSValues[fieldIndex].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(fieldIndex).RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record[fieldIndex].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(expectedFieldName).RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record[expectedFieldName].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(fieldIndex).RawValue);
                        
                        // Сравнение значений приведенных к требумому типу
                        Assert.AreEqual(expectedFieldValue, getTypedValueById(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, getTypedValueByName(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, converter(fieldIndex, record.GetValue(fieldIndex)));
                    }

                    ++recordCounter;
                }

                Assert.AreEqual(ctx.ExpectedRowsCount, recordCounter);

                ctx.AssertSql(expectedSql);
                ctx.AssertSqlParameters(expectedSqlParameters);
            }
        }

        /// <summary>Тестирование запроса получения записей <see cref="OneSDataRecord"/>.</summary>
        [Test]
        public void TestGetDataRecordsQuery()
        {
            Test
                .Query(dataContext =>
                    
                    from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                    select r)

                .ExpectedSql("SELECT * FROM Справочник.ТестовыйСправочник")
                
                .ExpectAllRows

            .Run();
        }

        /// <summary>
        /// Тестирование запроса фильтрации записей <see cref="OneSDataRecord"/>.
        /// </summary>
        [Test]
        public void TestFilteringDataRecordsQuery()
        {
            Test
                .Query(dataContext =>

                    from r in dataContext.GetRecords("Справочник.ТестовыйСправочник")
                    where r.GetString("СтроковоеПоле") == "Тестирование"
                    select r
                    
                    )

                .ExpectedSql("SELECT * FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = \"Тестирование\"")

                .ExpectedRows(0)

            .Run();
        }

        #region Типы для внутреннего DSL описания тестов

        /// <summary>Начальное состояние описания теста.</summary>
        private new sealed class InitBuilderState
        {
            /// <summary>Внутреннее состояние.</summary>
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public InitBuilderState(QueryTestsBase.InitBuilderState innerState)
            {
                _innerState = innerState;
            }

            /// <summary>Описание тестируемого linq-запроса.</summary>
            /// <param name="queryAction">
            /// Действие создания запроса.
            /// </param>
            public QueryDefinedBuilderState Query(Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction)
            {
                return new QueryDefinedBuilderState(_innerState, queryAction);
            }
        }

        /// <summary>
        /// Состояние после описания тестируемого запроса.
        /// </summary>
        private sealed class QueryDefinedBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<OneSDataRecord>> _queryAction;

            public QueryDefinedBuilderState(QueryTestsBase.InitBuilderState innerState, Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction)
            {
                _innerState = innerState;
                _queryAction = queryAction;
            }

            /// <summary>Описание ожидаемого sql-запроса отправляемого в 1С.</summary>
            /// <param name="sql">Ожидаемый sql-запрос.</param>
            public DefiningActionBuilderState ExpectedSql(string sql)
            {
                return new DefiningActionBuilderState(_innerState, _queryAction, sql);
            }
        }

        /// <summary>
        /// Состояние описания тестируемого действия.
        /// </summary>
        private sealed class DefiningActionBuilderState
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<OneSDataRecord>> _queryAction;
            private readonly string _expectedSql;
            private readonly Dictionary<string, object> _expectedSqlParameters = new Dictionary<string, object>();

            public DefiningActionBuilderState(QueryTestsBase.InitBuilderState innerState, Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction, string expectedSql)
            {
                _innerState = innerState;
                _queryAction = queryAction;
                _expectedSql = expectedSql;
            }

            /// <summary>Описания ожидаемого параметра sql-запроса отправляемого в 1С.</summary>
            /// <param name="name">Имя параметра.</param>
            /// <param name="value">Значения параметра.</param>
            public DefiningActionBuilderState ExpectedSqlParameter(string name, object value)
            {
                _expectedSqlParameters.Add(name, value);

                return this;
            }

            private DefiningExpectedDataBuilderState<ExpectedTestDictionary> DefineActionAndExpectedFields()
            {
                return _innerState

                    .Action(
                        ctx => TestReadRecords(ctx, _queryAction, _expectedSql, _expectedSqlParameters))

                    .BeginDefineExpectedDataFor<ExpectedTestDictionary>()

                    .AnyField(Fields.Catalog.Ref.GetLocalizedName())
                    .AnyField(Fields.Catalog.Code.GetLocalizedName())
                    .AnyField(Fields.Catalog.Description.GetLocalizedName())
                    .AnyField(Fields.Catalog.DeletionMark.GetLocalizedName())
                    .AnyField(Fields.Catalog.Presentation.GetLocalizedName())
                    .AnyField(Fields.Catalog.Predefined.GetLocalizedName())

                    .Field(d => d.StringField)
                    .Field(d => d.IntField)
                    .Field(d => d.NumberField)
                    .Field(d => d.BooleanField)
                    .Field(d => d.DateField)
                    .Field(d => d.DateTimeField)
                    .Field(d => d.TimeField)
                    .Field(d => d.UndoundStringField)
                    .Field(d => d.CharField);
            }

            /// <summary>
            /// Описание того, какие строки ожидаеются в результате запроса, и в каком порядке.
            /// </summary>
            /// <param name="rowIndexes">
            /// Индексы строки.
            /// </param>
            public FinalBuilderState ExpectedRows(params int[] rowIndexes)
            {
                return DefineActionAndExpectedFields()
                    .Rows(rowIndexes)
                    .EndDefineExpectedData;
            }

            /// <summary>
            /// Указывает, что следует ожидать все строки ожидаемых данных,
            /// в исходном порядке.
            /// </summary>
            public FinalBuilderState ExpectAllRows
            {
                get
                {
                    return DefineActionAndExpectedFields()
                        .AllRows
                        .EndDefineExpectedData;
                }
            }
        }

        #endregion
    }
}
