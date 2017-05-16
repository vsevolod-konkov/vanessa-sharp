using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Карта соответствия типа табличному источнику данных 1С вида табличной части.
    /// </summary>
    internal sealed class OneSTablePartTypeMapping
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="ownerType">Тип владельца - соответствующий табличному источнику данных 1С.</param>
        /// <param name="fieldMappings">Карты соответствия полей источника данных 1С членам типа.</param>
        public OneSTablePartTypeMapping(Type ownerType, ReadOnlyCollection<OneSFieldMapping> fieldMappings)
        {
            Contract.Requires<ArgumentNullException>(fieldMappings != null);
            
            _ownerType = ownerType;
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
        /// Тип владельца - соответствующий табличному источнику данных 1С.
        /// Может быть не задан.
        /// </summary>
        public Type OwnerType
        {
            get { return _ownerType; }
        }
        private readonly Type _ownerType;

        
    }
}
