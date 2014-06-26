using System;
using System.Diagnostics.Contracts;
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
        public TupleQuery(Expression<Func<TInput, TOutput>> selector, Expression<Func<TInput, bool>> filter)
        {
            Contract.Requires<ArgumentException>(
                (typeof(TInput) == typeof(TOutput) && selector == null) 
                || 
                (typeof(TInput) == typeof(TOutput) || selector != null));

            _filter = filter;
            _selector = selector;
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

        /// <summary>Преобразование.</summary>
        public ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider)
        {
            return new QueryTransformer(mappingProvider).Transform(this);
        }
    }
}
