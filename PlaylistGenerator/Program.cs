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
            int invalidLinksCount = 0;
            int failedLinksCount = 0;
            int invalidTweetsCount = 0;
            int duplicateCount = 0;
            DateTimeOffset lastRun = DateTimeOffset.MinValue; //Was DateTime.MinValue cast to DateTimeOffset (not valid) before, WHY!?
            if (System.IO.File.Exists("lastrun.txt"))
                lastRun = DateTimeOffset.Parse(System.IO.File.ReadAllText("lastrun.txt"));
            List<string> trackMediaIdList = new List<string>();
            Console.WriteLine("#ZuneTune Twitter Playlist Generator v0.1");
            Console.WriteLine("===========================================================");
            Console.WriteLine("Loading tweets...");
            try
            {
                using (XmlReader reader = XmlReader.Create(string.Format(TWITTERSEARCH, HttpUtility.UrlEncode("#ZuneTune"))))
                {
                    SyndicationFeed syndicationFeed = SyndicationFeed.Load(reader);
                    if (syndicationFeed.LastUpdatedTime <= lastRun)
                    {
                        Console.WriteLine("No updates since last run.");
                        Console.WriteLine("Press [Enter]");
                        Console.ReadLine();
                        return;
                    }
                    else
                    {
                        lastRun = syndicationFeed.LastUpdatedTime;
                        Console.WriteLine("Found {0} tweets, loading details...", syndicationFeed.Items.Count());
                        foreach (SyndicationItem syndicationItem in syndicationFeed.Items)
                        {
                            string text = syndicationItem.Title.Text;
                            int urlStart = text.IndexOf("http://");
                            List<string> urlList = new List<string>();
                            if (urlStart > -1)
                            {
                                int urlEnd = text.IndexOf(" ", urlStart);
                                if (urlEnd == -1)
                                {
                                    urlEnd = text.Length;
                                }
                                string link = text.Substring(urlStart, urlEnd - urlStart);
                                if (!urlList.Contains(link))
                                {
                                    urlList.Add(link);
                                }
                                else
                                {
                                    ++duplicateCount;
                                    Console.Write("-");
                                }
                            }
                            else
                            {
                                ++invalidTweetsCount;
                                Console.Write("-");
                            }
                            foreach (string url in urlList)
                            {
                                string expandedUrl = GetExpandedUrl(url);
                                if (string.IsNullOrEmpty(expandedUrl))
                                {
                                    ++failedLinksCount;
                                    Console.Write("-");
                                }
                                else
                                {
                                    int midPosition = expandedUrl.IndexOf("mid=");
                                    if (midPosition > -1)
                                    {
                                        int midStart = midPosition + 4;
                                        int midEnd = expandedUrl.IndexOf("&mtype=Track", midStart); //not very intelligent URL parsing...
                                        if (midEnd > -1)
                                        {
                                            string mid = expandedUrl.Substring(midStart, midEnd - midStart);
                                            if (!trackMediaIdList.Contains(mid, StringComparer.InvariantCultureIgnoreCase))
                                            {
                                                trackMediaIdList.Add(mid);
                                                Console.Write("*");
                                            }
                                            else
                                            {
                                                ++duplicateCount;
                                                Console.Write("-");
                                            }
                                        }
                                        else
                                        {
                                            ++invalidLinksCount;
                                            Console.Write("-");
                                        }
                                    }
                                    else
                                    {
                                        ++invalidLinksCount;
                                        Console.Write("-");
                                    }
                                }
                            }
                        }
                        Console.WriteLine(" - Done loading tweets!");
                        Console.WriteLine("=========================================");
                    }
                }
                Console.WriteLine("Building playlist for {0} tracks...", (object)trackMediaIdList.Count);
                int noRightsCount = 0;
                if (trackMediaIdList.Count > 0)
                {
                    Console.WriteLine("Writing playlist to: {0}", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Zune\\Playlists\\ZuneTune.zpl");
                    using (StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Zune\\Playlists\\ZuneTune.zpl"))
                    {
                        double playlistDuration = 0.0;
                        StringBuilder stringBuilder = new StringBuilder(1024);
                        foreach (string trackId in trackMediaIdList)
                        {
                            Track track = new Track(trackId);
                            if (track.HasRights)
                            {
                                playlistDuration += track.Duration.TotalSeconds;
                                stringBuilder.AppendLine(track.ToString());
                                Console.Write("*");
                            }
                            else
                            {
                                ++noRightsCount;
                                Console.Write("-");
                            }
                        }
                        streamWriter.WriteLine("<?zpl version=\"2.0\"?>");
                        streamWriter.WriteLine("<smil>");
                        streamWriter.WriteLine("<head>");
                        streamWriter.WriteLine("<guid>{418AA80C-84DC-4C8B-AD60-B4719D8E4C57}</guid>");
                        streamWriter.WriteLine("<meta name=\"generator\" content=\"ZuneTune -- 1.0\" />");
                        streamWriter.WriteLine("<meta name=\"itemCount\" content=\"{0}\" />", (trackMediaIdList.Count - noRightsCount));
                        streamWriter.WriteLine("<meta name=\"totalDuration\" content=\"" + playlistDuration.ToString() + "\" />");
                        streamWriter.WriteLine("<meta name=\"averageRating\" content=\"0\" />");
                        streamWriter.WriteLine("<meta name=\"creatorId\" content=\"{ECF302E2-CFFE-18C2-18E5-6437AE8B29DD}\" />");
                        streamWriter.WriteLine("<title>ZuneTune</title>");
                        streamWriter.WriteLine("</head>");
                        streamWriter.WriteLine("<body>");
                        streamWriter.WriteLine("<seq>");
                        streamWriter.Write(stringBuilder.ToString());
                        streamWriter.WriteLine("</seq>");
                        streamWriter.WriteLine("</body>");
                        streamWriter.WriteLine("</smil>");
                        Console.WriteLine(" - ZuneTune Playlist complete!");
                        Console.WriteLine("=========================================");
                        Console.WriteLine("Total Tracks: {0}", (trackMediaIdList.Count - noRightsCount));
                        Console.WriteLine("Duplicate Tracks: {0}", duplicateCount);
                        Console.WriteLine("No Rights: {0}", noRightsCount);
                        Console.WriteLine("Invalid Tweets: {0}", invalidTweetsCount);
                        Console.WriteLine("Invalid Links: {0}", invalidLinksCount);
                        Console.WriteLine("Failed Links: {0}", failedLinksCount);
                    }
                }
                System.IO.File.WriteAllText("lastrun.txt", lastRun.ToString());
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
