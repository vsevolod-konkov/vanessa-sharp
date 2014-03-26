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

        /// <summary>Добавление поля.</summary>
        public void AddField(string name, Type type)
        {
            _fields.Add(new FieldDescription(name, type));
        }

        /// <summary>Добавление поля.</summary>
        public void AddField<T>(string name)
        {
            AddField(name, typeof(T));
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