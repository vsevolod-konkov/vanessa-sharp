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
    
    #if REAL_MODE
    [TestFixture(TestMode.Real, false)]
    [TestFixture(TestMode.Real, true)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, false)]
    [TestFixture(TestMode.Isolated, true)]
    #endif
    public class LinqReadDataRecordsTests : QueryTestsBase
    {
        public LinqReadDataRecordsTests(TestMode testMode, bool shouldBeOpen)
            : base(testMode, shouldBeOpen)
        {}

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

        private new BuilderState0 Test
        {
           get { return new BuilderState0(base.Test); }
        }

        protected sealed class BuilderState0
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;

            public BuilderState0(InitBuilderState innerState)
            {
                _innerState = innerState;
            }

            public BuilderState1 Query(Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction)
            {
                return new BuilderState1(_innerState, queryAction);
            }
        }

        protected sealed class BuilderState1
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<OneSDataRecord>> _queryAction;

            public BuilderState1(InitBuilderState innerState, Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction)
            {
                _innerState = innerState;
                _queryAction = queryAction;
            }

            public BuilderState2 ExpectedSql(string sql)
            {
                return new BuilderState2(_innerState, _queryAction, sql);
            }
        }

        protected sealed class BuilderState2
        {
            private readonly QueryTestsBase.InitBuilderState _innerState;
            private readonly Func<OneSDataContext, IQueryable<OneSDataRecord>> _queryAction;
            private readonly string _expectedSql;
            private readonly Dictionary<string, object> _expectedSqlParameters = new Dictionary<string, object>();

            public BuilderState2(InitBuilderState innerState, Func<OneSDataContext, IQueryable<OneSDataRecord>> queryAction, string expectedSql)
            {
                _innerState = innerState;
                _queryAction = queryAction;
                _expectedSql = expectedSql;
            }

            public BuilderState2 ExpectedSqlParameter(string name, object value)
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

            public FinalBuilderState ExpectedRows(params int[] rowIndexes)
            {
                return DefineActionAndExpectedFields()
                    .Rows(rowIndexes)
                    .EndDefineExpectedData;
            }

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

                    var rawValues = new object[ctx.ExpectedFieldsCount];
                    Assert.AreEqual(ctx.ExpectedFieldsCount, record.GetValues(rawValues));

                    var oneSValues = new OneSValue[ctx.ExpectedFieldsCount];
                    Assert.AreEqual(ctx.ExpectedFieldsCount, record.GetValues(oneSValues));

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


                    for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                    {
                        if (ctx.ExpectedFieldType(fieldIndex) == typeof(AnyType))
                            continue;

                        var expectedFieldValue = ctx.ExpectedValue(recordCounter, fieldIndex);
                        var expectedRawFieldValue = QueryResultMockFactory.GetOneSRawValue(expectedFieldValue);
                        var fieldName = ctx.ExpectedFieldName(fieldIndex);

                        Assert.AreEqual(expectedRawFieldValue, rawValues[fieldIndex]);
                        Assert.AreEqual(expectedRawFieldValue, oneSValues[fieldIndex].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(fieldIndex).RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record[fieldIndex].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(fieldName).RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record[fieldName].RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record.GetValue(fieldIndex).RawValue);
                        Assert.AreEqual(expectedRawFieldValue, record[ctx.ExpectedFieldName(fieldIndex)].RawValue);
                        
                        Assert.AreEqual(expectedFieldValue, getTypedValueById(fieldIndex));
                        Assert.AreEqual(expectedFieldValue, getTypedValueByName(fieldIndex));
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

                .ExpectedSql("SELECT * FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")

                .ExpectedSqlParameter("p1", "Тестирование")
                
                .ExpectedRows(0)

            .Run();
        }
    }
}
