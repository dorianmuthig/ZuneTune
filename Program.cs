using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Xml;

/* *******************************************
 * #ZuneTune Twitter Playlist Generator v0.1
 *  -- created by @Tyren in Summer 2009
 * 
 * Code reproduced through reverse engineering 
 * executable made available at: bit.ly/hL7QY
 * 
 * Code slightly modified to work w/ .NET 4.5
 * All runtime bugs retained, so far.
 * 
 *********************************************/

namespace ZuneTune
{
    class Program
    {
        static string TWITTERSEARCH = "http://search.twitter.com/search.atom?q={0}&rpp=100"; //Twitter API v1 is deprecated, will no longer work.

        static void Main(string[] args)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            DateTimeOffset dateTimeOffset = DateTimeOffset.MinValue; //Was DateTime.MinValue cast to DateTimeOffset (not valid) before, WHY!?
            if (System.IO.File.Exists("lastrun.txt"))
                dateTimeOffset = DateTimeOffset.Parse(System.IO.File.ReadAllText("lastrun.txt"));
            List<string> list1 = new List<string>();
            Console.WriteLine("#ZuneTune Twitter Playlist Generator v0.1");
            Console.WriteLine("===========================================================");
            Console.WriteLine("Loading tweets...");
            try
            {
                using (XmlReader reader = XmlReader.Create(string.Format(Program.TWITTERSEARCH, (object)HttpUtility.UrlEncode("#ZuneTune"))))
                {
                    SyndicationFeed syndicationFeed = SyndicationFeed.Load(reader);
                    if (syndicationFeed.LastUpdatedTime <= dateTimeOffset)
                    {
                        Console.WriteLine("No updates since last run.");
                        Console.WriteLine("Press [Enter]");
                        Console.ReadLine();
                        return;
                    }
                    else
                    {
                        dateTimeOffset = syndicationFeed.LastUpdatedTime;
                        Console.WriteLine("Found {0} tweets, loading details...", (object)Enumerable.Count<SyndicationItem>(syndicationFeed.Items));
                        foreach (SyndicationItem syndicationItem in syndicationFeed.Items)
                        {
                            string text = syndicationItem.Title.Text;
                            int startIndex1 = text.IndexOf("http://");
                            List<string> list2 = new List<string>();
                            if (startIndex1 > -1)
                            {
                                int num5 = text.IndexOf(" ", startIndex1);
                                if (num5 == -1)
                                    num5 = text.Length;
                                string str = text.Substring(startIndex1, num5 - startIndex1);
                                if (!list2.Contains(str))
                                {
                                    list2.Add(str);
                                }
                                else
                                {
                                    ++num4;
                                    Console.Write("-");
                                }
                            }
                            else
                            {
                                ++num3;
                                Console.Write("-");
                            }
                            foreach (string url in list2)
                            {
                                string expandedUrl = Program.GetExpandedUrl(url);
                                if (string.IsNullOrEmpty(expandedUrl))
                                {
                                    ++num2;
                                    Console.Write("-");
                                }
                                else
                                {
                                    int num5 = expandedUrl.IndexOf("mid=");
                                    if (num5 > -1)
                                    {
                                        int startIndex2 = num5 + 4;
                                        int num6 = expandedUrl.IndexOf("&mtype=Track", startIndex2);
                                        if (num6 > -1)
                                        {
                                            string str = expandedUrl.Substring(startIndex2, num6 - startIndex2);
                                            if (!Enumerable.Contains<string>((IEnumerable<string>)list1, str, (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase))
                                            {
                                                list1.Add(str);
                                                Console.Write("*");
                                            }
                                            else
                                            {
                                                ++num4;
                                                Console.Write("-");
                                            }
                                        }
                                        else
                                        {
                                            ++num1;
                                            Console.Write("-");
                                        }
                                    }
                                    else
                                    {
                                        ++num1;
                                        Console.Write("-");
                                    }
                                }
                            }
                        }
                        Console.WriteLine(" - Done loading tweets!");
                        Console.WriteLine("=========================================");
                    }
                }
                Console.WriteLine("Building playlist for {0} tracks...", (object)list1.Count);
                int num7 = 0;
                if (list1.Count > 0)
                {
                    Console.WriteLine("Writing playlist to: {0}", (object)(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Zune\\Playlists\\ZuneTune.zpl"));
                    using (StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Zune\\Playlists\\ZuneTune.zpl"))
                    {
                        double num5 = 0.0;
                        StringBuilder stringBuilder = new StringBuilder(1024);
                        foreach (string trackId in list1)
                        {
                            Track track = new Track(trackId);
                            if (track.HasRights)
                            {
                                num5 += track.Duration.TotalSeconds;
                                stringBuilder.AppendLine(track.ToString());
                                Console.Write("*");
                            }
                            else
                            {
                                ++num7;
                                Console.Write("-");
                            }
                        }
                        streamWriter.WriteLine("<?zpl version=\"2.0\"?>");
                        streamWriter.WriteLine("<smil>");
                        streamWriter.WriteLine("<head>");
                        streamWriter.WriteLine("<guid>{418AA80C-84DC-4C8B-AD60-B4719D8E4C57}</guid>");
                        streamWriter.WriteLine("<meta name=\"generator\" content=\"ZuneTune -- 1.0\" />");
                        streamWriter.WriteLine("<meta name=\"itemCount\" content=\"{0}\" />", (object)(list1.Count - num7));
                        streamWriter.WriteLine("<meta name=\"totalDuration\" content=\"" + ((long)num5).ToString() + "\" />");
                        streamWriter.WriteLine("<meta name=\"averageRating\" content=\"0\" />");
                        streamWriter.WriteLine("<meta name=\"creatorId\" content=\"{ECF302E2-CFFE-18C2-18E5-6437AE8B29DD}\" />");
                        streamWriter.WriteLine("<title>ZuneTune</title>");
                        streamWriter.WriteLine("</head>");
                        streamWriter.WriteLine("<body>");
                        streamWriter.WriteLine("<seq>");
                        streamWriter.Write(((object)stringBuilder).ToString());
                        streamWriter.WriteLine("</seq>");
                        streamWriter.WriteLine("</body>");
                        streamWriter.WriteLine("</smil>");
                        Console.WriteLine(" - ZuneTune Playlist complete!");
                        Console.WriteLine("=========================================");
                        Console.WriteLine("Total Tracks: {0}", (object)(list1.Count - num7));
                        Console.WriteLine("Duplicate Tracks: {0}", (object)num4);
                        Console.WriteLine("No Rights: {0}", (object)num7);
                        Console.WriteLine("Invalid Tweets: {0}", (object)num3);
                        Console.WriteLine("Invalid Links: {0}", (object)num1);
                        Console.WriteLine("Failed Links: {0}", (object)num2);
                    }
                }
                System.IO.File.WriteAllText("lastrun.txt", dateTimeOffset.ToString());
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    Console.WriteLine("Twitter is over capacity, try again later.");
                }
            }
            Console.WriteLine("=========================================");
            Console.WriteLine("Press [Enter]");
            Console.ReadLine();
        }

        static string GetExpandedUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.Method = "GET";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Timeout = 10000;
            try
            {
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    return ((NameValueCollection)httpWebResponse.Headers)["Location"];
                }
            }
            catch (Exception e)
            {
                if (e != null) //more convenient for debugging, construct avoids compiler warning
                {

                }
            }
            return null;
        }
    }
}
