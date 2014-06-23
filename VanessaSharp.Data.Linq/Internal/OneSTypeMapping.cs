using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Карта соответствия типа табличному источнику данных 1С.
    /// </summary>
    internal sealed class OneSTypeMapping
    {
        public OneSTypeMapping(string sourceName, ReadOnlyCollection<OneSFieldMapping> fieldMappings)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Requires<ArgumentNullException>(fieldMappings != null);

            _sourceName = sourceName;
            _fieldMappings = fieldMappings;
        }

        /// <summary>Карты соответствия полей источника данных 1С членам типа.</summary>
        public ReadOnlyCollection<OneSFieldMapping> FieldMappings
        {
            get { return _fieldMappings; }
        }
        private readonly ReadOnlyCollection<OneSFieldMapping> _fieldMappings;

        /// <summary>
        /// Имя источника данных 1С соответсвующего данному типу.
        /// </summary>
        public string SourceName
        {
            get { return _sourceName; }
        }
        private readonly string _sourceName;
    }
}