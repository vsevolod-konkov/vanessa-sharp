using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Стандартная реализация фабрики соединителей к информационной БД 1С.</summary>
    public sealed class OneSConnectorFactory : IOneSConnectorFactory
    {
        /// <summary>Фабрика по умолчанию.</summary>
        public static IOneSConnectorFactory Default
        {
            get { return _default; }
        }
        private static readonly IOneSConnectorFactory _default = new OneSConnectorFactory();

        /// <summary>Создание коннектора.</summary>
        /// <param name="creationParams">Параметры-рекомендации создания коннектора.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не смогла создать экземпляр коннектора.</exception>
        public IOneSConnector Create(ConnectorCreationParams creationParams)
        {
            if (creationParams != null)
            {
                if (creationParams.Version.HasValue)
                    return OneSConnector.CreateFromVersion(creationParams.Version.Value);

                if (creationParams.ConnectorProgId != null)
                    return OneSConnector.CreateFromProgId(creationParams.ConnectorProgId);

                if (creationParams.TypeName != null)
                    return CreateByTypeName(creationParams.TypeName);
            }

            return CreateForAnyVersions();
        }

        /// <summary>Создание коннектора по имени типа его реализации.</summary>
        /// <param name="typeName">Имя типа.</param>
        private static IOneSConnector CreateByTypeName(string typeName)
        {
            var type = GetTypeByName(typeName);
            var typeIOneSConnector = typeof(IOneSConnector);

            if (!typeIOneSConnector.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format(
                    "Тип \"{0}\" указанный для создания коннектора к 1С не реализует требуемый интерфейс \"{1}\".",
                    typeName, typeIOneSConnector));
            }

            try
            {
                return (IOneSConnector)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Ошибка при создании соединителя типа \"{0}\" к информационной БД 1C. Подробности: {1}",
                        typeName, e.Message), e);
            }
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

        private static IOneSConnector CreateForAnyVersions()
        {
            var versions = Enum.GetValues(typeof(OneSVersion));
            Array.Reverse(versions);

            foreach (OneSVersion version in versions)
            {
                var connector = OneSConnector.TryCreateFromVersion(version);
                if (connector != null)
                    return connector;
            }

            throw new InvalidOperationException(
                "Невозможно создать экземпляр 1С-соединителя. Не найден co-класс COM-соединителя 1С к информационной базе.");
        }
    }
}
