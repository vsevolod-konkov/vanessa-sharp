using System;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.ExpectedData;

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
        /// Тестирование <see cref="OneSDataReader.GetSchemaTable"/>
        /// и <see cref="OneSDataReader.GetDataTypeName"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetSchema()
        {
            Test
                .Source("Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx =>
                    {
                        Assert.Throws<NotImplementedException>(() =>
                        {
                            var schemaTable = ctx.TestedReader.GetSchemaTable();
                        });

                        for (var fieldIndex = 0; fieldIndex < ctx.ExpectedFieldsCount; fieldIndex++)
                        {
                            Assert.Throws<NotImplementedException>(() =>
                            {
                                var typeName = ctx.TestedReader.GetDataTypeName(fieldIndex);
                            });
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
        /// Тестирование <see cref="OneSDataReader.GetGuid"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetGuid()
        {
            Test
                .Source("Справочник.СправочникUID")
                .Execute()
                .Action(ctx =>
                    {
                        Assert.IsTrue(ctx.TestedReader.Read());

                        Assert.Throws<NotImplementedException>(() => { var value = ctx.TestedReader.GetGuid(0); });
                    })

                .BeginDefineExpectedDataFor<ExpectedUidTestDictionary>()

                    .Field(d => d.GuidField)
                    .Rows(0)

                .EndDefineExpectedData

            .Run();
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetEnumerator"/>.
        /// </summary>
        [Test]
        [TestCheckNotImplemented]
        public void TestGetEnumerator()
        {
            Test
                .Sql("ВЫБРАТЬ * ИЗ Справочник.ТестовыйСправочник")
                .Execute()
                .Action(ctx =>

                     Assert.Throws<NotImplementedException>(() => { var enumerator = ctx.TestedReader.GetEnumerator(); }))

                .BeginDefineExpectedDataFor<ExpectedTestDictionary>()
                .Rows()
                .EndDefineExpectedData

            .Run();
        }
    }
}
