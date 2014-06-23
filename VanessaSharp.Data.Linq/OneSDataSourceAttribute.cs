using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Атрибут маркировки типа, для указания
    /// какому источнику данных 1С он соответствует.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class OneSDataSourceAttribute : Attribute
    {
        /// <summary>Конструктор принимающий имя источника данных 1С.</summary>
        /// <param name="sourceName">Имя источника данных 1С.</param>
        public OneSDataSourceAttribute(string sourceName)
        {
            _sourceName = sourceName;
        }

        /// <summary>Имя источника данных 1С.</summary>
        public string SourceName
        {
            get { return _sourceName; }
        }
        private readonly string _sourceName;
    }
}
