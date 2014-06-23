using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Запрос типизированных кортежей.</summary>
    internal sealed class TupleQuery<T> : ISimpleQuery
    {
        /// <summary>Преобразование.</summary>
        public ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider)
        {
            return new QueryTransformer(mappingProvider).Transform(this);
        }
    }
}
