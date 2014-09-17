using System.Xml.Serialization;

namespace ZuneTune.PlaylistGenerator
{
    [XmlRoot("right", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
    public class Right
    {
        [XmlElement("licenseType", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
        public string LicenseType { get; set; }
    }
}