using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой объект запроса.</summary>
    internal abstract class SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        protected SimpleQuery(string source)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
            
            _source = source;
        }

        /// <summary>Источник записей.</summary>
        public string Source
        {
            get { return _source; }
        }
        private readonly string _source;

        /// <summary>Тип элемента.</summary>
        public abstract Type ItemType { get; }

        /// <summary>Преобразование.</summary>
        public abstract ExpressionParseProduct Transform();
    }
}
