using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой объект запроса.</summary>
    internal abstract class SimpleQuery : ISimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        protected SimpleQuery(string source, Expression<Func<OneSDataRecord, bool>> filter, ReadOnlyCollection<SortExpression> sorters)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
            Contract.Requires<ArgumentNullException>(sorters != null);
  
            _source = source;
            _filter = filter;
            _sorters = sorters;
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

        /// <summary>Выражения сортировки.</summary>
        public ReadOnlyCollection<SortExpression> Sorters
        {
            get { return _sorters; }
        }
        private readonly ReadOnlyCollection<SortExpression> _sorters;

        /// <summary>Тип элемента.</summary>
        public abstract Type ItemType { get; }

        /// <summary>Преобразование.</summary>
        public abstract ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider);
    }
}
