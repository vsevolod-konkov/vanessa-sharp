using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Стандартная реализация фабрики соединителей к информационной БД 1С.</summary>
    public sealed class OneSConnectorFactory : IOneSConnectorFactory
    {
        /// <summary>Версия 8.2</summary>
        private const string VERSION_8_2 = "8.2";

        /// <summary>Фабрика по умолчанию.</summary>
        public static IOneSConnectorFactory Default
        {
            get { return _default; }
        }
        private static readonly IOneSConnectorFactory _default = new OneSConnectorFactory();

        /// <summary>Версия 1С по умолчанию.</summary>
        public static string DefaultVersion
        {
            get { return VERSION_8_2; }
        }

        /// <summary>Создание коннектора.</summary>
        /// <param name="creationParams">Параметры-рекомендации создания коннектора.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не смогла создать экземпляр коннектора.</exception>
        public IOneSConnector Create(ConnectorCreationParams creationParams)
        {
            var typeName = GetConnectorTypeName(creationParams);

            try
            {
                return CreateByTypeName(typeName);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Ошибка при создании соединителя типа \"{0}\" к информационной БД 1C. Подробности: {1}",
                        typeName, e.Message), e);
            }
        }

        /// <summary>Создание коннектора по имени типа его реализации.</summary>
        /// <param name="typeName">Имя типа.</param>
        private IOneSConnector CreateByTypeName(string typeName)
        {
            var type = GetTypeByName(typeName);
            var typeIOneSConnector = typeof(IOneSConnector);

            if (!typeIOneSConnector.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format(
                    "Тип \"{0}\" указанный для создания коннектора к 1С не реализует требуемый интерфейс \"{1}\".",
                    typeName, typeIOneSConnector));
            }

            return (IOneSConnector)Activator.CreateInstance(type);
        }

        private string GetConnectorTypeName(ConnectorCreationParams creationParams)
        {
            string version = null;
            
            if (creationParams != null)
            {
                var result = creationParams.TypeName;
                if (result != null)
                    return result;

                version = creationParams.Version;
            }

            if (version == null)
                version = DefaultVersion;

            var typeName = GetConnectorTypeNameByVersion(version);
            Contract.Assert(typeName != null);

            return typeName;
        }

        private string GetConnectorTypeNameByVersion(string version)
        {
            if (version == VERSION_8_2)
                return "VanessaSharp.Proxy.V82.OneSConnector, VanessaSharp.Proxy.V82";

            throw new InvalidOperationException(string.Format(
                "Для версии \"{0}\" не найден тип реализации коннектора.",
                version));
        }

        /// <summary>Получение типа коннектора.</summary>
        /// <param name="typeName">Имя типа.</param>
        private static Type GetTypeByName(string typeName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(typeName));

            var result = Type.GetType(typeName);
            if (result == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Не удалось найти тип \"{0}\".", typeName));
            }

            return result;
        }
    }
}
