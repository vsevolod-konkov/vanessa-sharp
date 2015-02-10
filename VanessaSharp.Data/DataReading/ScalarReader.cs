using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IScalarReader"/>.
    /// </summary>
    internal sealed class ScalarReader : IScalarReader
    {
        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static ScalarReader Default
        {
            get { return _default; }    
        }
        private static readonly ScalarReader _default = new ScalarReader();

        /// <summary>Чтение скалярного значения.</summary>
        /// <param name="queryResult">Результат запроса.</param>
        public object ReadScalar(IQueryResult queryResult)
        {
            if (queryResult.IsEmpty())
                return null;

            object rawValue;
            using (var selection = queryResult.Choose(QueryResultIteration.Default))
            {
                if (!selection.Next())
                    return null;

                rawValue = selection.Get(0);
            }

            if (rawValue == null)
                return null;

            Func<object, object> valueConverter;
            using (var columns = queryResult.Columns)
            using (var column = columns.Get(0))
            using (var columnType = column.ValueType)
            {
                var type = TypeDescriptionConverter.Default.ConvertFrom(columnType);
                // TODO: Copy logic !!!

                if (type == typeof(Guid))
                    valueConverter = v => OneSObjectSpecialConverter.Default.ToGuid(v);
                else if (type == typeof(OneSDataReader))
                    valueConverter = OneSObjectSpecialConverter.Default.ToDataReader;
                else
                    valueConverter = v => v;
            }

            return valueConverter(rawValue);
        }
    }
}
