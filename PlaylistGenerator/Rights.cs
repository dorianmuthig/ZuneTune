using System.Xml.Serialization;

namespace ZuneTune.PlaylistGenerator
{
    [XmlRoot("rights", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
    public class Rights
    {
        [XmlElement("right", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
        public Right[] Right { get; set; }
    }
}
