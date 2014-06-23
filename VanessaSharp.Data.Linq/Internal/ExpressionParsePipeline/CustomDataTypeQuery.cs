using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Типизированный по выходным элементам простой объект запроса.
    /// </summary>
    /// <typeparam name="T">Тип элементов.</typeparam>
    internal sealed class CustomDataTypeQuery<T> : SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        /// <param name="selectExpression">Выражение выборки данных их записи.</param>
        public CustomDataTypeQuery(
            string source, 
            Expression<Func<OneSDataRecord, bool>> filter,
            ReadOnlyCollection<SortExpression> sorters, 
            Expression<Func<OneSDataRecord, T>> selectExpression)
            
            : base(source, filter, sorters)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);
            Contract.Requires<ArgumentNullException>(typeof(T) != typeof(OneSDataRecord));

            _selectExpression = selectExpression;
        }

        /// <summary>Тип элемента.</summary>
        public override Type ItemType
        {
            get { return typeof(T); }
        }

        /// <summary>Выражение выборки данных их записи.</summary>
        public Expression<Func<OneSDataRecord, T>>  SelectExpression
        {
            get { return _selectExpression; }
        }
        private readonly Expression<Func<OneSDataRecord, T>> _selectExpression;

        /// <summary>Преобразование.</summary>
        public override ExpressionParseProduct Transform(IOneSMappingProvider mappingProvider)
        {
            return new QueryTransformer(mappingProvider).Transform(this);
        }
    }
}