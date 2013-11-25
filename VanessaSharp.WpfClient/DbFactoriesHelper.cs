using System.Data;

namespace VanessaSharp.WpfClient
{
    /// <summary>Вспомогательный класс для работы с описанием ADO.Net провайдеров.</summary>
    internal static class DbFactoriesHelper
    {
        /// <summary>Получение наименования ADO.Net провайдера для отображения.</summary>
        /// <param name="dataRow">Строка таблицы с описанием провайдера.</param>
        public static string GetDbProviderName(DataRow dataRow)
        {
            return (string)dataRow["Name"];
        }

        /// <summary>Получение наименование ADO.Net провайдера для отображения.</summary>
        /// <param name="dataRow">Строка таблицы с описанием провайдера.</param>
        public static string GetDbProviderInvariantName(DataRow dataRow)
        {
            return (string)dataRow["InvariantName"];
        }
    }
}
