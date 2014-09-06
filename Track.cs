// Decompiled with JetBrains decompiler
// Type: ZuneTune.Track
// Assembly: ZuneTune, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D51F632B-29C0-47E3-8196-9F622D280AE8
// Assembly location: C:\Users\Dorian\Documents\Visual Studio 2013\Projects\ZuneTune\ZuneTune.exe

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
                return this.Rights != null && this.Rights.Right != null && Enumerable.Count<Right>((IEnumerable<Right>)this.Rights.Right) > 0;
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
            catch (Exception ex)
            {
                Console.WriteLine("\nOops: {0}", (object)((object)ex).ToString());
            }
        }

        public override string ToString()
        {
            return "<media serviceId=\"{" + HttpUtility.HtmlEncode(this.Id) + "}\" albumTitle=\"" + HttpUtility.HtmlEncode(this.Album.Title) + "\" albumArtist=\"" + HttpUtility.HtmlEncode(this.Album.Artist.Name) + "\" trackTitle=\"" + HttpUtility.HtmlEncode(this.Title) + "\" trackArtist=\"" + HttpUtility.HtmlEncode(this.Artist.Name) + "\" duration=\"" + ((long)this.Duration.TotalMilliseconds).ToString() + "\" />";
        }
    }
}
