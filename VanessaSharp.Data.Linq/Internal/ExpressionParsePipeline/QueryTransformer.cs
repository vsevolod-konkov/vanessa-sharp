using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобращователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal static class QueryTransformer
    {
        /// <summary>Преобразование.</summary>
        /// <param name="query">Запрос.</param>
        public static ExpressionParseProduct Transform(SimpleQuery query)
        {
            Contract.Requires<ArgumentNullException>(query != null);

            var sql = "SELECT * FROM " + query.Source;

            return new CollectionReadExpressionParseProduct<OneSDataRecord>(
                new SqlCommand(sql, SqlParameter.EmptyCollection),
                OneSDataRecordReaderFactory.Default);
        }
    }
}
