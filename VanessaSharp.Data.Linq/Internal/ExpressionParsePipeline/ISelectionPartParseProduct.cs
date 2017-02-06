using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    internal interface ISelectionPartParseProduct
    {
        Expression GetTablePartColumnAccessExpression(SqlExpression tablePartExpression, ColumnExpressionBuilder builder);
    }
}
