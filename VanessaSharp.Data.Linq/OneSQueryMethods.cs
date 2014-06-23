using System.Linq;
using System.Reflection;

namespace VanessaSharp.Data.Linq
{
    /// <summary>Вспомогательные методы для создания запросов.</summary>
    internal static class OneSQueryMethods
    {
        /// <summary>Создание запроса получения записей из табличного источника данных 1С.</summary>
        /// <param name="sourceName">Имя табличного источника данных.</param>
        public static IQueryable<OneSDataRecord> GetRecords(string sourceName)
        {
            throw new InvalidCallLinqMethodException(MethodBase.GetCurrentMethod());
        }

        /// <summary>Создание запроса получения типизированных записей из табличного источника данных 1С.</summary>
        public static IQueryable<T> GetTypedRecords<T>()
        {
            throw new InvalidCallLinqMethodException(MethodBase.GetCurrentMethod());
        }
    }
}
