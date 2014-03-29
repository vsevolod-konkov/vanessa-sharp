using System;
using System.Collections.ObjectModel;
using VanessaSharp.AcceptanceTests.Utility.Mocks;

namespace VanessaSharp.AcceptanceTests.Utility
{
    /// <summary>Базовый класс для тестов чтения данных.</summary>
    public abstract class ReadDataTestBase : ConnectedTestsBase
    {
        private TableDataBuilder _dataBuilder;
        private ReadOnlyCollection<object> _currentExpectedRowData;
        
        protected ReadDataTestBase(TestMode testMode, bool shouldBeOpen) 
            : base(testMode, shouldBeOpen)
        {
        }

        protected ReadDataTestBase(TestMode testMode) : base(testMode)
        {
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

        /// <summary>
        /// Определение строки в табличных данных для теста.
        /// </summary>
        protected void Row(params object[] rowdata)
        {
            _dataBuilder.AddRow(rowdata);
        }

        /// <summary>Ожидаемое количество полей.</summary>
        protected virtual int ExpectedFieldsCount
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
        protected virtual string ExpectedFieldName(int fieldIndex)
        {
            return ExpectedData.Fields[fieldIndex].Name;
        }

        /// <summary>
        /// Ожидаемый тип поля по данному индексу <paramref name="fieldIndex"/>.
        /// </summary>
        protected virtual Type ExpectedFieldType(int fieldIndex)
        {
            return ExpectedData.Fields[fieldIndex].Type;
        }

        /// <summary>
        /// Ожидаемое значение поля по данному индексу <paramref name="fieldIndex"/>
        /// в текущей строке.
        /// </summary>
        protected virtual object ExpectedFieldValue(int fieldIndex)
        {
            return _currentExpectedRowData[fieldIndex];
        }

        /// <summary>
        /// Установка текущей строки для проверки ожидаемых результатов.
        /// </summary>
        /// <param name="rowIndex">Индекс строки.</param>
        protected void SetCurrentExpectedRow(int rowIndex)
        {
            _currentExpectedRowData = ExpectedData.Rows[rowIndex];
        }
    }
}
