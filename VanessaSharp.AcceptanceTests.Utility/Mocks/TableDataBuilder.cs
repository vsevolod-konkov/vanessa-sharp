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

        /// <summary>Добавление скалярного поля.</summary>
        public void AddScalarField(string name, Type type)
        {
            _fields.Add(new FieldDescription(name, type));
        }

        /// <summary>Добавление скалярного поля.</summary>
        public void AddScalarField<T>(string name)
        {
            AddScalarField(name, typeof(T));
        }

        /// <summary>Добавление поля табличной части.</summary>
        public void AddTablePartField(string name, ReadOnlyCollection<FieldDescription> fields)
        {
            _fields.Add(new FieldDescription(name, fields));
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