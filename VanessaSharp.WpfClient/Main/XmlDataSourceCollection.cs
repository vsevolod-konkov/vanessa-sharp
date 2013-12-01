using System.Xml.Serialization;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Коллекция кортежей с описанием источников данных для XML-сериализации.</summary>
    [XmlRoot("data-sources")]
    public sealed class XmlDataSourceCollection
    {
        /// <summary>Источники данных.</summary>
        [XmlElement("data-source")]
        public XmlDataSource[] DataSources;
    }
}
