using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility.Mocks;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.AcceptanceTests.Utility
{
    /// <summary>Базовый класс для тестов чтения данных.</summary>
    public abstract class ReadDataTestBase : ConnectedTestsBase
    {
        private TableDataBuilder _dataBuilder;
        private ReadOnlyCollection<object> _currentExpectedRowData;

        private readonly ITestModeStrategy _testModeStrategy;

        protected ReadDataTestBase(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {
            _testModeStrategy = CreateTestModeStrategy(testMode);
        }

        protected ReadDataTestBase(TestMode testMode) : base(testMode)
        {
            _testModeStrategy = CreateTestModeStrategy(testMode);
        }

        private static ITestModeStrategy CreateTestModeStrategy(TestMode testMode)
        {
            return (testMode == TestMode.Isolated)
                                    ? (ITestModeStrategy)new IsolatedModeStrategy()
                                    : new RealModeStrategy();
        }

        /// <summary>Инициализация фикстуры в зависимости от режима.</summary>
        [SetUp]
        public void ModeSetUp()
        {
            _testModeStrategy.SetUp(this);
        }

        /// <summary>Проверка SQL-запроса переданного в 1С.</summary>
        protected void AssertSql(string expectedSql)
        {
            _testModeStrategy.AssertSql(expectedSql);
        }

        /// <summary>Проверка параметров SQL-запроса переданного в 1С.</summary>
        protected void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
        {
            _testModeStrategy.AssertSqlParameters(expectedSqlParameters);
        }

        /// <summary>
        /// Обработчик запроса на создание экземпляра объекта 1С.
        /// </summary>
        protected override void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {
            _testModeStrategy.OnNewOneSObjectAsking(args);
        }

        /// <summary>Ожидаемые табличные данные.</summary>
        protected TableData ExpectedData { get; private set; }

        /// <summary>Начало определения данных для теста.</summary>
        protected void BeginDefineData()
        {
            _dataBuilder = new TableDataBuilder();
        }

        /// <summary>Завершение определения данных для теста.</summary>
        protected void EndDefineData()
        {
            ExpectedData = _dataBuilder.Build();
        }

        /// <summary>Определение поля табличных данных для теста.</summary>
        protected void Field<T>(string name)
        {
            _dataBuilder.AddField<T>(name);
        }

        /// <summary>Определение поля табличных данных для теста.</summary>
        protected void Field(string name, Type type)
        {
            _dataBuilder.AddField(name, type);
        }

        /// <summary>
        /// Определение строки в табличных данных для теста.
        /// </summary>
        protected void Row(params object[] rowdata)
        {
            _dataBuilder.AddRow(rowdata);
        }

        /// <summary>Ожидаемое количество полей.</summary>
        protected int ExpectedFieldsCount
        {
            get { return ExpectedData.Fields.Count; }
        }

        /// <summary>Ожидаемое количество строк.</summary>
        protected int ExpectedRowsCount
        {
            get { return ExpectedData.Rows.Count; }
        }

        /// <summary>
        /// Ожидаемое имя поля по данному индексу <paramref name="fieldIndex"/>.
        /// </summary>
        protected string ExpectedFieldName(int fieldIndex)
        {
            return ExpectedData.Fields[fieldIndex].Name;
        }

        /// <summary>
        /// Ожидаемый тип поля по данному индексу <paramref name="fieldIndex"/>.
        /// </summary>
        protected Type ExpectedFieldType(int fieldIndex)
        {
            return ExpectedData.Fields[fieldIndex].Type;
        }

        /// <summary>
        /// Ожидаемое значение поля по данному индексу <paramref name="fieldIndex"/>
        /// в текущей строке.
        /// </summary>
        protected object ExpectedFieldValue(int fieldIndex)
        {
            return _currentExpectedRowData[fieldIndex];
        }

        /// <summary>
        /// Ожидаемое значение поля по имени поля
        /// в текущей строке.
        /// </summary>
        protected object ExpectedFieldValue(string fieldName)
        {
            var index = IndexOf(fieldName);

            return _currentExpectedRowData[index];
        }

        private int IndexOf(string fieldName)
        {
            var indexes = ExpectedData.Fields
                                    .Select((f, i) => new { Index = i, FieldName = f.Name })
                                    .Where(s => s.FieldName == fieldName)
                                    .Select(s => s.Index);

            try
            {
                return indexes.Single();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(string.Format(
                    "Колонка \"{0}\" не найдена.",
                    fieldName));
            }
        }

        /// <summary>
        /// Установка текущей строки для проверки ожидаемых результатов.
        /// </summary>
        /// <param name="rowIndex">Индекс строки.</param>
        protected void SetCurrentExpectedRow(int rowIndex)
        {
            _currentExpectedRowData = ExpectedData.Rows[rowIndex];
        }

        #region Стратегии в зависимости от режима тестирования

        private interface ITestModeStrategy
        {
            void SetUp(ReadDataTestBase textFixture);

            void OnNewOneSObjectAsking(NewOneSObjectEventArgs args);

            void AssertSql(string expectedSql);

            void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters);
        }

        private sealed class RealModeStrategy : ITestModeStrategy
        {
            public void SetUp(ReadDataTestBase textFixture)
            {}

            public void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
            {}

            public void AssertSql(string expectedSql)
            {}

            public void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
            {}
        }

        private sealed class IsolatedModeStrategy : ITestModeStrategy
        {
            private IQuery _query;
            private Dictionary<string, object> _parameters; 

            public void SetUp(ReadDataTestBase textFixture)
            {
                _parameters = new Dictionary<string, object>();

                var queryMock = MockHelper.CreateDisposableMock<IQuery>();
                queryMock
                    .SetupProperty(q => q.Text);
                queryMock
                    .Setup(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback<string, object>((name, value) => _parameters[name] = value);
                queryMock
                    .Setup(q => q.Execute())
                    .Returns(() => QueryResultMockFactory.Create(textFixture.ExpectedData));
                _query = queryMock.Object;
            }

            public void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
            {
                if (args.RequiredType == typeof(IQuery))
                    args.CreatedInstance = _query;
            }

            public void AssertSql(string expectedSql)
            {
                Assert.AreEqual(expectedSql, _query.Text);
            }

            public void AssertSqlParameters(IDictionary<string, object> expectedSqlParameters)
            {
                CollectionAssert.AreEquivalent(expectedSqlParameters, _parameters);
            }
        }

        #endregion
    }
}
