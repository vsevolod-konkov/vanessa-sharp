using System;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Запрос типизированных кортежей.</summary>
    internal sealed class TupleQuery<T> : ISimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="filter">Выражение фильтрации.</param>
        public TupleQuery(Expression<Func<T, bool>> filter)
        {
            _filter = filter;
        }

        /// <summary>Выражение фильтрации.</summary>
        public Expression<Func<T, bool>> Filter
        {
            get { return _filter; }
        }
        private readonly Expression<Func<T, bool>> _filter;

        /// <summary>Преобразование.</summary>
        public ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider)
        {
            return new QueryTransformer(mappingProvider).Transform(this);
        }
    }
}
