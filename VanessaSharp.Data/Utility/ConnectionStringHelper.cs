using System;

namespace VanessaSharp.Data.Utility
{
    /// <summary>Вспомогательные методы для работы со строкой подключения.</summary>
    internal static class ConnectionStringHelper
    {
        /// <summary>Получение пути к каталогу БД 1С из строки подключения.</summary>
        /// <param name="connectionString">Строка подключения.</param>
        public static string GetCatalogFromConnectionString(string connectionString)
        {
            var builder = new OneSConnectionStringBuilder();
            try
            {
                builder.ConnectionString = connectionString;
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Строка соединения \"{0}\" не является валидной.",
                    connectionString), e);
            }

            return builder.Catalog;
        }
    }
}
