using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Интерфейс описания источника данных.
    /// </summary>
    [ContractClass(typeof(SourceDescriptionContract))]
    internal interface ISourceDescription
    {
        /// <summary>Получение имени источника данных.</summary>
        /// <param name="sourceResolver">Ресолвер имен источников данных.</param>
        string GetSourceName(ISourceResolver sourceResolver);
    }

    [ContractClassFor(typeof(ISourceDescription))]
    internal abstract class SourceDescriptionContract : ISourceDescription
    {
        /// <summary>Получение имени источника данных.</summary>
        /// <param name="sourceResolver">Ресолвер имен источников данных.</param>
        string ISourceDescription.GetSourceName(ISourceResolver sourceResolver)
        {
            Contract.Requires<ArgumentNullException>(sourceResolver != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return null;
        }
    }
}
