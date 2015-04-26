using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Базовый класс запроса.</summary>
    /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
    /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
    internal abstract class QueryBase<TInput, TOutput> : IQuery<TInput, TOutput>
    {
        /// <summary>Источник данных.</summary>
        public abstract ISourceDescription Source { get; }

        /// <summary>Выражение выборки.</summary>
        public abstract Expression<Func<TInput, TOutput>> Selector { get; }

        /// <summary>Выражение фильтрации.</summary>
        public abstract Expression<Func<TInput, bool>> Filter { get; }

        /// <summary>Выражение сортировки.</summary>
        public abstract ReadOnlyCollection<SortExpression> Sorters { get; }

        /// <summary>Выборка различных.</summary>
        public abstract bool IsDistinct { get; }

        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        /// <param name="transformService">Сервис преобразования запросов.</param>
        public ExpressionParseProduct Transform(IQueryTransformService transformService)
        {
            return transformService
                .CreateTransformer()
                .Transform(this);
        }
    }
}
