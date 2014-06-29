using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Сервис для преобразования запросов.</summary>
    [ContractClass(typeof(QueryTransformServiceContract))]
    internal interface IQueryTransformService
    {
        /// <summary>Создание преобразователя запросов.</summary>
        IQueryTransformer CreateTransformer();
    }

    [ContractClassFor(typeof(IQueryTransformService))]
    internal abstract class QueryTransformServiceContract : IQueryTransformService
    {
        /// <summary>Создание преобразователя запросов.</summary>
        IQueryTransformer IQueryTransformService.CreateTransformer()
        {
            Contract.Ensures(Contract.Result<IQueryTransformer>() != null);

            return null;
        }
    }
}
