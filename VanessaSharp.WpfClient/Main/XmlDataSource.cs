using System.Xml.Serialization;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Кортеж с описанием источника данных для XML-сериализации.</summary>
    public sealed class XmlDataSource
    {
        [XmlAttribute("db-provider")]
        public string DbProviderInvariantName;

        [XmlAttribute("db-connection-string")]
        public string DbConnectionString;
    }
}
