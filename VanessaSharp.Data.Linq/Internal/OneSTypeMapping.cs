using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Карта соответствия типа табличному источнику данных 1С.
    /// </summary>
    internal sealed class OneSTypeMapping
    {
        /// <summary>Конструктор.</summary>
        /// <param name="sourceName">
        /// Имя источника данных 1С соответсвующего данному типу.
        /// </param>
        /// <param name="fieldMappings">
        /// Карты соответствия полей источника данных 1С членам типа.
        /// </param>
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
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<OneSFieldMapping>>() != null);
                
                return _fieldMappings;
            }
        }
        private readonly ReadOnlyCollection<OneSFieldMapping> _fieldMappings;

        /// <summary>
        /// Имя источника данных 1С соответсвующего данному типу.
        /// </summary>
        public string SourceName
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return _sourceName;
            }
        }
        private readonly string _sourceName;
    }
}