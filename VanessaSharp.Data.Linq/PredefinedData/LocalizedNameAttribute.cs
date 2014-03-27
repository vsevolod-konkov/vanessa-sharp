using System;

namespace VanessaSharp.Data.Linq.PredefinedData
{
    // TODO: Временное решение, связанное с локализацией названий таблиц и полей
    /// <summary>Атрибут локализованного имени.</summary>
    internal sealed class LocalizedNameAttribute : Attribute
    {
        public LocalizedNameAttribute(string localizedName)
        {
            _localizedName = localizedName;
        }

        /// <summary>
        /// Локализованное имя.
        /// </summary>
        public string LocalizedName
        {
            get { return _localizedName; }
        }
        private readonly string _localizedName;
    }
}
