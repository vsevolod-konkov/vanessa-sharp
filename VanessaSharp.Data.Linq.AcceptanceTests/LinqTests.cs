//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using NUnit.Framework;
//using VanessaSharp.AcceptanceTests.Utility;

//namespace VanessaSharp.Data.Linq.AcceptanceTests
//{
    //#if REAL_MODE
    ////[TestFixture(TestMode.Real, false)]
    ////[TestFixture(TestMode.Real, true)]
    //#endif
    //#if ISOLATED_MODE
    ////[TestFixture(TestMode.Isolated, false)]
    //[TestFixture(TestMode.Isolated, true)]
    //#endif
    //internal sealed class LinqTests : LinqReadDataTestBase
    //{
    //    public LinqTests(TestMode testMode, bool shouldBeOpen)
    //        : base(testMode, shouldBeOpen)
    //    {}

    //    private readonly List<Func<ExpectedTestDictionary, object>> _fieldAccessors = new List<Func<ExpectedTestDictionary, object>>();

    //    private void Field<TValue>(Expression<Func<ExpectedTestDictionary, TValue>> fieldAccessor)
    //    {
    //        // TODO Safe

    //        var fieldInfo = (FieldInfo)((MemberExpression)fieldAccessor.Body).Member;
    //        var fieldAttr = (FieldAttribute)fieldInfo.GetCustomAttributes(typeof(FieldAttribute), false)[0];

    //        var fieldType = fieldAttr.FieldType ?? fieldInfo.FieldType;

    //        Field(fieldAttr.FieldName, fieldType);

    //        var fieldAccessorFunc = Expression.Lambda<Func<ExpectedTestDictionary, object>>(
    //            Expression.Convert(fieldAccessor.Body, typeof(object)),
    //            fieldAccessor.Parameters)
    //            .Compile();

    //        _fieldAccessors.Add(fieldAccessorFunc);
    //    }

    //    private void Rows(params int[] indexes)
    //    {
    //        foreach (var index in indexes)
    //        {
    //            var testDictionary = ExpectedTestDictionary.ExpectedData[index];
    //            var rowData = _fieldAccessors.Select(fa => fa(testDictionary)).ToArray();
    //            Row(rowData);
    //        }
    //    }

    //    private void TestQuery<T>(
    //        Func<OneSDataContext, IQueryable<T>> query, Action<T> assertEntry, 
    //        string expectedSql,
    //        Dictionary<string, object> expectedSqlParameters = null)
    //    {
    //        using (var dataContext = new OneSDataContext(Connection))
    //        {
    //            var entries = query(dataContext);

    //            var recordCounter = 0;

    //            foreach (var entry in entries)
    //            {
    //                Assert.Less(recordCounter, ExpectedRowsCount);

    //                SetCurrentExpectedRow(recordCounter);

    //                assertEntry(entry);

    //                ++recordCounter;
    //            }

    //            Assert.AreEqual(ExpectedRowsCount, recordCounter);

    //            AssertSql(expectedSql);
    //            AssertSqlParameters(expectedSqlParameters ?? new Dictionary<string, object>());
    //        }
    //    }

    //    [Test]
    //    public void Test()
    //    {
    //        BeginDefineData();

    //        Field(t => t.StringField);
    //        Field(t => t.IntField);
    //        Field(t => t.NumberField);
    //        Field(t => t.BooleanField);
    //        Field(t => t.DateField);

    //        Rows(0);

    //        EndDefineData();

    //        Assert.AreEqual(5, ExpectedFieldsCount);

    //        TestQuery(
    //            dataContext => from e in dataContext.Get<TestDictionary>()
    //                           where e.StringField == "Тестирование"
    //                           select new
    //                           {
    //                              String = e.StringField,
    //                              Integer = e.IntegerField,
    //                              Real = e.RealField,
    //                              Boolean = e.BooleanField,
    //                              Date = e.DateField
    //                           },

    //            entry =>
    //                {
    //                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.String);
    //                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.Integer);
    //                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.Real);
    //                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.Boolean);
    //                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.Date);
    //                },
    //                "SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1",
    //                new Dictionary<string, object> { { "p1", "Тестирование" } }
    //            );
    //    }

    //    [Test]
    //    public void Test2()
    //    {
            
    //            LinqTestBuilder.BeginDefineDataFor<ExpectedTestDictionary>()
                    
    //                .Field(t => t.StringField)
    //                .Field(t => t.IntField)
    //                .Field(t => t.NumberField)
    //                .Field(t => t.BooleanField)
    //                .Field(t => t.DateField)
                    
    //                .RowIndexes(0)

    //            .EndDefineData


    //            .Query(dataContext =>
    //                           from e in dataContext.Get<TestDictionary>()
    //                           where e.StringField == "Тестирование"
    //                           select new
    //                           {
    //                              String = e.StringField,
    //                              Integer = e.IntegerField,
    //                              Real = e.RealField,
    //                              Boolean = e.BooleanField,
    //                              Date = e.DateField
    //                           }
                    
    //            )
                
    //            .ExpectedSql("SELECT СтроковоеПоле, ЦелочисленноеПоле, ЧисловоеПоле, БулевоПоле, ДатаПоле FROM Справочник.ТестовыйСправочник WHERE СтроковоеПоле = &p1")
    //            .ExpectedSqlParameter("p1", "Тестирование")
                
    //            .AssertEntry(entry =>
    //                {
    //                    Assert.AreEqual(ExpectedFieldValue("СтроковоеПоле"), entry.String);
    //                    Assert.AreEqual(ExpectedFieldValue("ЦелочисленноеПоле"), entry.Integer);
    //                    Assert.AreEqual(ExpectedFieldValue("ЧисловоеПоле"), entry.Real);
    //                    Assert.AreEqual(ExpectedFieldValue("БулевоПоле"), entry.Boolean);
    //                    Assert.AreEqual(ExpectedFieldValue("ДатаПоле"), entry.Date);
    //                })

    //            .Run();
    //    }

    //    protected internal static class LinqTestBuilder
    //    {
    //        public static DefineFieldsState<T> BeginDefineDataFor<T>()
    //        {
    //            return new DefineFieldsState<T>();
    //        }

    //        public sealed class DefineFieldsState<T>
    //        {
    //            public DefineFieldsState<T> Field<TValue>(Expression<Func<T, TValue>> fieldAccessor)
    //            {

    //                return this;
    //            }

    //            public DefineRowsState RowIndexes(params int[] rowIndexes)
    //            {
    //                return new DefineRowsState();
    //            }
    //        }

    //        public sealed class DefineRowsState
    //        {
    //            public EndDefineDataState EndDefineData
    //            {
    //                get { return new EndDefineDataState(); }
    //            }
    //        }

    //        public sealed class EndDefineDataState
    //        {
    //            public DefineQueryState<T> Query<T>(Func<OneSDataContext, IQueryable<T>> query)
    //            {
    //                return new DefineQueryState<T>();
    //            }
    //        }

    //        public sealed class DefineQueryState<T>
    //        {
    //            public DefineExpectedSqlState<T> ExpectedSql(string sql)
    //            {
    //                return new DefineExpectedSqlState<T>();
    //            }
    //        }

    //        public sealed class DefineExpectedSqlState<T>
    //        {
    //            public DefineExpectedSqlState<T> ExpectedSqlParameter(string parameterName, object value)
    //            {
    //                return this;
    //            }

    //            public DefineAssertEntryState<T> AssertEntry(Action<T> assertAction)
    //            {
    //                return new DefineAssertEntryState<T>();
    //            }
    //        }

    //        public sealed class DefineAssertEntryState<T>
    //        {
    //            public void Run()
    //            {

    //            }
    //        }
    //    }
//    }
//}
