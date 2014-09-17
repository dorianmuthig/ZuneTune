using System.Xml.Serialization;

namespace ZuneTune.PlaylistGenerator
{
    [XmlRoot("primaryArtist", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
    public class PrimaryArtist
    {
        [XmlElement("name", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
        public string Name { get; set; }
    }
}