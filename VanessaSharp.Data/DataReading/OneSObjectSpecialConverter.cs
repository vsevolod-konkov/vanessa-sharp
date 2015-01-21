using System;
using Microsoft.CSharp.RuntimeBinder;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Cпециальный конвертер для 
    /// объектов 1С, с которыми требуется работать через
    /// стандартные API .Net.
    /// </summary>
    internal sealed class OneSObjectSpecialConverter : IOneSObjectSpecialConverter
    {
        /// <summary>
        /// Экземпляр по умолчанию.
        /// </summary>
        public static OneSObjectSpecialConverter Default
        {
            get { return _default; }    
        }
        private static readonly OneSObjectSpecialConverter _default = new OneSObjectSpecialConverter();
        
        /// <summary>
        /// Преобразование объекта 1С к читателю данных.
        /// </summary>
        /// <param name="oneSObject">Объект 1С.</param>
        public OneSDataReader ToDataReader(object oneSObject)
        {
            var queryResult = DynamicCastQueryResult(oneSObject);

            return new OneSDataReader(queryResult);
        }

        private static IQueryResult DynamicCastQueryResult(dynamic obj)
        {
            try
            {
                return obj;
            }
            catch (RuntimeBinderException e)
            {
                throw new ArgumentException(string.Format(
                    "Полученный из 1С объект \"{0}\" не поддерживает интерфейс \"{1}\", необходимый для создания объекта \"{2}\".",
                    obj,
                    typeof(IQueryResult),
                    typeof(OneSDataReader)), 
                    e);
            }
        }
    }
}
