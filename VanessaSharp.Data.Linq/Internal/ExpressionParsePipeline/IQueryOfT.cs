using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Интерфейс запроса.
    /// </summary>
    /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
    /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
    [ContractClass(typeof(QueryContract<,>))]
    internal interface IQuery<TInput, TOutput> : IQuery
    {
        /// <summary>Источник данных.</summary>
        ISourceDescription Source { get; }

        /// <summary>Выражение выборки.</summary>
        Expression<Func<TInput, TOutput>> Selector { get; }

        /// <summary>Выражение фильтрации.</summary>
        Expression<Func<TInput, bool>> Filter { get; }

        /// <summary>Выражение сортировки.</summary>
        ReadOnlyCollection<SortExpression> Sorters { get; }

        /// <summary>Максимальное количество строк.</summary>
        int? MaxCount { get; }

        /// <summary>Выборка различных.</summary>
        bool IsDistinct { get; }
    }

    [ContractClassFor(typeof(IQuery<,>))]
    internal abstract class QueryContract<TInput, TOutput> : IQuery<TInput, TOutput>
    {
        /// <summary>Источник данных.</summary>
        ISourceDescription IQuery<TInput, TOutput>.Source
        {
            get
            {
                Contract.Ensures(Contract.Result<ISourceDescription>() != null);
                
                return null;
            }
        }

        /// <summary>Выражение выборки.</summary>
        Expression<Func<TInput, TOutput>> IQuery<TInput, TOutput>.Selector
        {
            get { return null; }
        }

        /// <summary>Выражение фильтрации.</summary>
        Expression<Func<TInput, bool>> IQuery<TInput, TOutput>.Filter
        {
            get { return null; }
        }

        /// <summary>Выражение сортировки.</summary>
        ReadOnlyCollection<SortExpression> IQuery<TInput, TOutput>.Sorters
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<SortExpression>>() != null);
                Contract.Ensures(Contract.Result<ReadOnlyCollection<SortExpression>>().All(
                    s =>
                        {
                            var type = s.KeyExpression.Type;

                            return type.IsGenericType
                                   && type.GetGenericTypeDefinition() == typeof (Func<,>)
                                   && type.GetGenericArguments()[0] == typeof (TInput);
                        }));

                return null;
            }
        }

        /// <summary>Максимальное количество строк.</summary>
        public int? MaxCount { get { return 0; } }

        /// <summary>Выборка различных.</summary>
        bool IQuery<TInput, TOutput>.IsDistinct { get { return false; } }

        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        /// <param name="transformService">Сервис преобразования запросов.</param>
        public abstract ExpressionParseProduct Transform(IQueryTransformService transformService);
    }
}
