using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Базовый класс запроса.</summary>
    /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
    /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
    internal abstract class QueryBase<TInput, TOutput> : IQuery<TInput, TOutput>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Описание источника данных.</param>
        /// <param name="selector">Выражение выборки.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="maxCount">Максимальное количество строк.</param>
        protected QueryBase(
            ISourceDescription source,
            Expression<Func<TInput, TOutput>> selector,
            Expression<Func<TInput, bool>> filter,
            ReadOnlyCollection<SortExpression> sorters,
            bool isDistinct,
            int? maxCount)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentException>(
                ((typeof(TInput) == typeof(OneSDataRecord)) && (source is ExplicitSourceDescription))
                ||
                ((typeof(TInput) != typeof(OneSDataRecord)) && (source is SourceDescriptionByType<TInput>))
                );

            Contract.Requires<ArgumentException>(
                (typeof(TInput) == typeof(TOutput) && selector == null)
                ||
                (typeof(TInput) == typeof(TOutput) || selector != null));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(sorters.All(s =>
            {
                var type = s.KeyExpression.Type;

                return type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Func<,>)
                    && type.GetGenericArguments()[0] == typeof(TInput);
            }));

            _source = source;
            _selector = selector;
            _filter = filter;
            _sorters = sorters;
            _isDistinct = isDistinct;
            _maxCount = maxCount;
        }

        /// <summary>Описание источника данных 1С.</summary>
        public ISourceDescription Source
        {
            get { return _source; }
        }
        private readonly ISourceDescription _source;

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

        /// <summary>Выборка различных.</summary>
        public bool IsDistinct
        {
            get { return _isDistinct; }
        }
        private readonly bool _isDistinct;

        /// <summary>Максимальное количество строк.</summary>
        public int? MaxCount
        {
            get { return _maxCount; }
        }
        private readonly int? _maxCount;

        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        /// <param name="transformService">Сервис преобразования запросов.</param>
        public ExpressionParseProduct Transform(IQueryTransformService transformService)
        {
            return Transform(
                transformService
                    .CreateTransformer());
        }

        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        protected abstract ExpressionParseProduct Transform(IQueryTransformer transformer);
    }
}
