using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Запрос типизированных кортежей.</summary>
    /// <typeparam name="TInput">Тип элементов входящей последовательности.</typeparam>
    /// <typeparam name="TOutput">Тип элементов выходящей последовательности.</typeparam>
    internal sealed class TupleQuery<TInput, TOutput> : ISimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="selector">Выражение выборки.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        public TupleQuery(Expression<Func<TInput, TOutput>> selector, Expression<Func<TInput, bool>> filter, ReadOnlyCollection<SortExpression> sorters)
        {
            Contract.Requires<ArgumentException>(
                (typeof(TInput) == typeof(TOutput) && selector == null) 
                || 
                (typeof(TInput) == typeof(TOutput) || selector != null));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(sorters.All(s =>
                {
                    var type = s.KeyExpression.Type;

                    return type.IsGenericType 
                        && type.GetGenericTypeDefinition() == typeof (Func<,>) 
                        && type.GetGenericArguments()[0] == typeof (TInput);
                }));

            _selector = selector;
            _filter = filter;
            _sorters = sorters;
        }

        /// <summary>Выражение выборки.</summary>
        public Expression<Func<TInput, TOutput>> Selector
        {
            get { return _selector; }
        }
        private readonly Expression<Func<TInput, TOutput>> _selector;

        /// <summary>Выражение фильтрации.</summary>
        public Expression<Func<TInput, bool>> Filter
        {
            get { return _filter; }
        }
        private readonly Expression<Func<TInput, bool>> _filter;

        /// <summary>Выражения сортировки.</summary>
        public ReadOnlyCollection<SortExpression> Sorters
        {
            get { return _sorters; }
        }
        private readonly ReadOnlyCollection<SortExpression> _sorters;

        /// <summary>Преобразование.</summary>
        public ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider)
        {
            return new QueryTransformer(mappingProvider).Transform(this);
        }
    }
}
