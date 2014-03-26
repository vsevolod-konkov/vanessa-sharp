using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Табличные данные.</summary>
    public sealed class TableData
    {
        public TableData(
            ReadOnlyCollection<FieldDescription> fields,
            ReadOnlyCollection<ReadOnlyCollection<object>> rows)
        {
            _fields = fields;
            _rows = rows;
        }

        /// <summary>Поля таблицы.</summary>
        public ReadOnlyCollection<FieldDescription> Fields
        {
            get { return _fields; }
        }
        private readonly ReadOnlyCollection<FieldDescription> _fields;

        /// <summary>Строки таблицы.</summary>
        public ReadOnlyCollection<ReadOnlyCollection<object>> Rows
        {
            get { return _rows; }
        }
        private readonly ReadOnlyCollection<ReadOnlyCollection<object>> _rows;
    }
}
