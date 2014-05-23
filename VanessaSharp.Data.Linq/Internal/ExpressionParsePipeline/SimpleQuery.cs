using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой объект запроса.</summary>
    internal abstract class SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        protected SimpleQuery(string source, Expression<Func<OneSDataRecord, bool>> filter)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
            
            _source = source;
            _filter = filter;
        }

        /// <summary>Источник записей.</summary>
        public string Source
        {
            get { return _source; }
        }
        private readonly string _source;
        
        /// <summary>Выражение фильтрации.</summary>
        public Expression<Func<OneSDataRecord, bool>> Filter
        {
            get { return _filter; }
        }
        private readonly Expression<Func<OneSDataRecord, bool>> _filter;

        /// <summary>Тип элемента.</summary>
        public abstract Type ItemType { get; }

        /// <summary>Преобразование.</summary>
        public abstract ExpressionParseProduct Transform();
    }
}
