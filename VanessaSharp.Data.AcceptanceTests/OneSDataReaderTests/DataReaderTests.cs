using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;
using VanessaSharp.AcceptanceTests.Utility.Mocks;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Базовый класс приемочных тестов на <see cref="OneSDataReader"/>.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, Description = "Тестирование для реального режима")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, Description = "Тестирование для изолированного режима")]
    #endif
    public sealed class DataReaderTests : DataReaderTestsBase
    {
        public DataReaderTests(TestMode testMode)
            : base(testMode)
        {}

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetSchemaTable"/>.
        /// </summary>
        [Test]
        public void TestGetSchema()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx => 
                    {
                        var schemaTable = ctx.TestedReader.GetSchemaTable();

                        Assert.AreEqual(4, schemaTable.Columns.Count);

                        Assert.AreEqual("ColumnOrdinal", schemaTable.Columns[0].ColumnName);
                        Assert.AreEqual("ColumnName", schemaTable.Columns[1].ColumnName);
                        Assert.AreEqual("DataTypeName", schemaTable.Columns[2].ColumnName);
                        Assert.AreEqual("AllowDBNull", schemaTable.Columns[3].ColumnName);

                        Assert.AreEqual(typeof(int), schemaTable.Columns[0].DataType);
                        Assert.AreEqual(typeof(string), schemaTable.Columns[1].DataType);
                        Assert.AreEqual(typeof(string), schemaTable.Columns[2].DataType);
                        Assert.AreEqual(typeof(bool), schemaTable.Columns[3].DataType);

                        Assert.AreEqual(ctx.ExpectedFieldsCount, schemaTable.Rows.Count);

                        for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                        {
                            var row = schemaTable.Rows[fieldIndex];

                            Assert.AreEqual(fieldIndex, row["ColumnOrdinal"]);
                            Assert.AreEqual(ctx.ExpectedFieldName(fieldIndex), row["ColumnName"]);
                            Assert.AreEqual(ctx.ExpectedFieldDataTypeName(fieldIndex), row["DataTypeName"]);
                            Assert.AreEqual(
                                ctx.ExpectedFieldDataTypeName(fieldIndex).Contains("Null"),
                                row["AllowDBNull"]
                                );
                        }
                    })

                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()

                .Field(d => d.StringField)
                .Field(d => d.IntField)

                .Rows()

            .EndDefineExpectedData
            
            .Run();
        }

        /// <summary>
        /// Тестирование 
        /// методов потокового чтения:
        /// <see cref="OneSDataReader.GetBytes"/>
        /// и <see cref="OneSDataReader.GetChars"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestStreamReading()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx =>
                    {
                        Assert.IsTrue(ctx.TestedReader.Read());

                        const int BUFFER_SIZE = 1024;

                        var binaryBuffer = new byte[BUFFER_SIZE];
                        Assert.Throws<NotImplementedException>(
                            () => ctx.TestedReader.GetBytes(0, 0, binaryBuffer, 0, BUFFER_SIZE));

                        var charBuffer = new char[BUFFER_SIZE];
                        Assert.Throws<NotImplementedException>(
                            () => ctx.TestedReader.GetChars(0, 0, charBuffer, 0, BUFFER_SIZE));
                    })

                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()

                .Field(d => d.UndoundStringField)
                .Rows(0)

           .EndDefineExpectedData

           .Run();
        }

        /// <summary>
        /// Тестирование значений курсора.
        /// </summary>
        /// <param name="ctx">Контекст тестирования.</param>
        /// <param name="rowIndex">Индекс текущей строки.</param>
        /// <param name="values">Проверямые значения.</param>
        private static void TestValues(TestingContext ctx, int rowIndex, IList<object> values)
        {
            Assert.AreEqual(ctx.ExpectedFieldsCount, values.Count);
            for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
            {
                var rawExpectedFieldValue = QueryResultMockFactory.GetOneSRawValue(
                    ctx.ExpectedValue(rowIndex, fieldIndex));

                Assert.AreEqual(
                    rawExpectedFieldValue,
                    values[fieldIndex]
                    );
            }
        }

        /// <summary>
        /// Тестирование реализации <see cref="System.Collections.IEnumerable"/>.
        /// </summary>
        [Test]
        public void TestEnumerable()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx =>
                    {
                        var rowCounter = 0;
                        foreach (IList<object> values in ctx.TestedReader)
                        {
                            Assert.Less(rowCounter, ctx.ExpectedRowsCount);
                            TestValues(ctx, rowCounter, values);

                            rowCounter++;
                        }

                        Assert.AreEqual(rowCounter, ctx.ExpectedRowsCount);
                    })
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
        /// Тестирование значений перечислителя.
        /// </summary>
        /// <param name="ctx">Контекст тестирования.</param>
        /// <param name="enumerator">Тестируемый перечислитель.</param>
        private static void TestEnumerator(TestingContext ctx, IEnumerator enumerator)
        {
            var rowCounter = 0;

            while (enumerator.MoveNext())
            {
                Assert.Less(rowCounter, ctx.ExpectedRowsCount);
                TestValues(ctx, rowCounter, (IList<object>)enumerator.Current);

                rowCounter++;
            }

            Assert.AreEqual(rowCounter, ctx.ExpectedRowsCount);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetEnumerator"/>.
        /// </summary>
        [Test]
        public void TestGetEnumerator()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx =>
                {
                    var enumerator = ctx.TestedReader.GetEnumerator();
                    
                    var disposable = enumerator as IDisposable;
                    using (disposable)
                    {
                        TestEnumerator(ctx, enumerator);
                        enumerator.Reset();
                        TestEnumerator(ctx, enumerator);
                    }
                })
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
    }
}
