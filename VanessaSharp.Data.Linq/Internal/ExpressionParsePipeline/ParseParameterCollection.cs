using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Коллекция параметров запроса,
    /// заполняемая при парсинге запроса.
    /// </summary>
    internal sealed class ParseParameterCollection
    {
        /// <summary>Счетчик.</summary>
        private int _counter;

        private readonly Dictionary<object, string> _parameters = new Dictionary<object, string>(); 

        /// <summary>Формирование коллекции параметров SQL-запроса.</summary>
        public ReadOnlyCollection<SqlParameter> GetSqlParameters()
        {
            return new ReadOnlyCollection<SqlParameter>(
                _parameters
                    .Select(p => new SqlParameter(p.Value, p.Key))
                    .ToArray()
                );
        }

        /// <summary>Получение имени параметра для значения.</summary>
        public string GetOrAddNewParameterName(object value)
        {
            string result;
            if (_parameters.TryGetValue(value, out result))
                return result;

            result = GetNewParameterName();
            _parameters.Add(value, result);

            return result;
        }

        private string GetNewParameterName()
        {
            _counter++;

            return "p" + _counter;
        }
    }
}
