using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой запрос получения записей.</summary>
    internal sealed class DataRecordsQuery : SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        public DataRecordsQuery(
            string source, 
            Expression<Func<OneSDataRecord, bool>> filter,
            ReadOnlyCollection<SortExpression> sorters) 
            
            : base(source, filter, sorters)
        {}

        /// <summary>Тип элемента.</summary>
        public override Type ItemType
        {
            get { return typeof(OneSDataRecord); }
        }

        /// <summary>Преобразование.</summary>
        public override ExpressionParseProduct Transform()
        {
            return new QueryTransformer().Transform(this);
        }
    }
}
