using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace ZuneTune
{
    public class Track
    {
        public string Id { get; private set; }

        public string Title { get; private set; }

        public TimeSpan Duration { get; private set; }

        public Album Album { get; private set; }

        public PrimaryArtist Artist { get; private set; }

        public Rights Rights { get; private set; }

        public bool HasRights
        {
            get
            {
                return this.Rights != null && this.Rights.Right != null && this.Rights.Right.Count() > 0;
            }
        }

        public Track(string trackId)
        {
            this.Id = trackId;
            try
            {
                using (XmlReader reader = XmlReader.Create("http://catalog.zune.net/v3.0/music/track/" + this.Id))
                {
                    SyndicationItem syndicationItem = SyndicationItem.Load(reader);
                    this.Duration = XmlConvert.ToTimeSpan(syndicationItem.ElementExtensions.ReadElementExtensions<string>("length", "http://schemas.zune.net/catalog/music/2007/10")[0]);
                    this.Album = syndicationItem.ElementExtensions.ReadElementExtensions<Album>("album", "http://schemas.zune.net/catalog/music/2007/10", new XmlSerializer(typeof(Album)))[0];
                    this.Rights = syndicationItem.ElementExtensions.ReadElementExtensions<Rights>("rights", "http://schemas.zune.net/catalog/music/2007/10", new XmlSerializer(typeof(Rights)))[0];
                    this.Artist = syndicationItem.ElementExtensions.ReadElementExtensions<PrimaryArtist>("primaryArtist", "http://schemas.zune.net/catalog/music/2007/10", new XmlSerializer(typeof(PrimaryArtist)))[0];
                    this.Title = syndicationItem.Title.Text;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nOops: {0}", e.ToString());
            }
        }

        public override string ToString()
        {
            return "<media serviceId=\"{" + HttpUtility.HtmlEncode(this.Id) + "}\" albumTitle=\"" + HttpUtility.HtmlEncode(this.Album.Title) + "\" albumArtist=\"" + HttpUtility.HtmlEncode(this.Album.Artist.Name) + "\" trackTitle=\"" + HttpUtility.HtmlEncode(this.Title) + "\" trackArtist=\"" + HttpUtility.HtmlEncode(this.Artist.Name) + "\" duration=\"" + ((long)this.Duration.TotalMilliseconds).ToString() + "\" />";
        }
    }
}
