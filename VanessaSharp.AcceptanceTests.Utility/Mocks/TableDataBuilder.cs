using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Построитель немутабельных табличных данных.</summary>
    public sealed class TableDataBuilder
    {
        private readonly List<FieldDescription> _fields = new List<FieldDescription>();

        private readonly List<ReadOnlyCollection<object>> _rows = new List<ReadOnlyCollection<object>>(); 

        /// <summary>
        /// Добавление нетестируемого поля.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        public void AddAnyField(string name)
        {
            _fields.Add(new AnyFieldDescription(name));
        }
        
        /// <summary>Добавление скалярного поля.</summary>
        public void AddScalarField(string name, Type type, string dataTypeName)
        {
            _fields.Add(new ScalarFieldDescription(name, type, dataTypeName));
        }

        /// <summary>Добавление поля табличной части.</summary>
        public void AddTablePartField(string name, ReadOnlyCollection<FieldDescription> fields)
        {
            _fields.Add(new TablePartFieldDescription(name, fields));
        }

        /// <summary>Добавление строки.</summary>
        public void AddRow(params object[] rowdata)
        {
            _rows.Add(new ReadOnlyCollection<object>(rowdata));
        }

        /// <summary>
        /// Построение немутабельного объекта табличных данных.
        /// </summary>
        public TableData Build()
        {
            return new TableData(
                new ReadOnlyCollection<FieldDescription>(_fields),
                new ReadOnlyCollection<ReadOnlyCollection<object>>(_rows)
                );
        }
    }
}