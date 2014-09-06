using System.Xml.Serialization;

namespace ZuneTune
{
    [XmlRoot("album", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
    public class Album
    {
        [XmlElement("title", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
        public string Title { get; set; }

        [XmlElement("primaryArtist", Namespace = "http://schemas.zune.net/catalog/music/2007/10")]
        public PrimaryArtist Artist { get; set; }
    }
}
