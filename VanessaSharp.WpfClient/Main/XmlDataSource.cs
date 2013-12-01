using System.Xml.Serialization;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Кортеж с описанием источника данных для XML-сериализации.</summary>
    public sealed class XmlDataSource
    {
        /// <summary>Имя ADO.Net провайдера источника данных.</summary>
        [XmlAttribute("db-provider")]
        public string DbProviderInvariantName;

        /// <summary>Строка подключения к источнику данных.</summary>
        [XmlAttribute("db-connection-string")]
        public string DbConnectionString;
    }
}
