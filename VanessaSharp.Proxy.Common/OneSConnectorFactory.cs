using System;
using System.Diagnostics.Contracts;
using System.Reflection;

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

        /// <summary>Создание соединителя по умолчанию.</summary>
        public static IOneSConnector Create()
        {
            return Default.Create(DefaultVersion);
        }
        
        /// <summary>Создание соединения в зависимости от версии.</summary>
        /// <param name="version">Версия.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="ArgumentNullException">В случае, если значение <paramref name="version"/> было пустым.</exception>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не может создать экземпляр коннектора заданной версии.</exception>
        public IOneSConnector Create(string version)
        {
            if (version == VERSION_8_2)
            {
                return (IOneSConnector)Activator.CreateInstance(
                    GetType("VanessaSharp.Proxy.V82", "VanessaSharp.Proxy.V82.OneSConnector"));
            }

            throw new InvalidOperationException(string.Format(
                "Невозможно создать соединитель к информационной БД 1C версии \"{0}\". " 
                + "Данная версия не поддерживается. Поддерживается только версия 8.2.", 
                version));
        }

        /// <summary>Получение типа коннектора.</summary>
        /// <param name="assemblyName">Имя сборки.</param>
        /// <param name="typeName">Имя типа.</param>
        private static Type GetType(string assemblyName, string typeName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(assemblyName));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(typeName));

            var result = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            if (result == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Не удалось найти тип \"{0}\" в сборке \"{1}\".",
                    typeName, assemblyName));
            }

            return result;
        }
    }
}
