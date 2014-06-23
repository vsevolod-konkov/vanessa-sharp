using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Интерфейс запроса.</summary>
    [ContractClass(typeof(ISimpleQueryContract))]
    internal interface ISimpleQuery
    {
        /// <summary>Преобразование.</summary>
        ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider);
    }

    [ContractClassFor(typeof(ISimpleQuery))]
    internal abstract class ISimpleQueryContract : ISimpleQuery
    {
        ExpressionParseProduct ISimpleQuery.Transform(IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Ensures(Contract.Result<ExpressionParseProduct>() != null);

            return null;
        }
    }
}
