using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace DeadLinks
{
    class Program
    {
        static HashSet<string> sites = new HashSet<string>();
        static Hashtable deadLinks = new Hashtable();
        static StreamWriter allSites = new StreamWriter("report.txt");
        static Regex r = new Regex(@"(?: href\s*=)(?:[\s""']*)(?!#|mailto|location.|javascript|.*css|.*this\.)(?<url>.*?)(?:[\s>""'])", RegexOptions.IgnoreCase);
        static WebClient web;
        static string mainHost;
        static int allCount;
        const int MAX_PAGES = 300;

        static Uri GetUrl(string baseUrl, string relativeUrl = "")
        {
            Uri baseUri = null;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUri))
            {
                return null;
            }

            Uri uri = null;
            if (relativeUrl == null || relativeUrl == "")
            {
                uri = baseUri;
            }
            else
            {
                if (!Uri.TryCreate(baseUri, relativeUrl, out uri))
                {
                    return null;
                }
            }

            return uri;
        }

        static HttpWebResponse GetLinkResponse(Uri url)
        {
            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
            }
            catch (Exception)
            {
                Console.WriteLine("Strange URL : {0}", url.ToString());
                return null;
            } 

            req.Method = "HEAD";
            req.AllowAutoRedirect = true;
            req.MaximumAutomaticRedirections = 3;
            req.Timeout = 5000;
            req.Proxy = null;

            HttpWebResponse response = null;
            try
            {
                if ((response = req.GetResponse() as HttpWebResponse) == null)
                {
                    return null;
                }
            }
            catch (WebException e)
            {
                var errResp = (HttpWebResponse)(e.Response);
                if (response != null)
                {
                    response.Close();
                }
                
                return errResp;
            }

            return response;
        }

        static void ParsePage(Uri url)
        {
            sites.Add(url.AbsoluteUri);
            Console.WriteLine("Go to {0}", url.AbsoluteUri);
            //using (WebClient web = new WebClient())
            {
                string code = "";
                try
                {
                    code = web.DownloadString(url);
                }
                catch (Exception)
                {
                    Console.WriteLine("Download failed {0}", url);
                    return;
                }

                HashSet<string> allStringLinks = new HashSet<string>();
                HashSet<Uri> allLinks = new HashSet<Uri>();
                allStringLinks.Add("");
                foreach (Match match in r.Matches(code))
                {
                    string newLink = match.Groups["url"].Value;
                    if (!newLink.StartsWith("#"))
                    {
                        allStringLinks.Add(newLink);
                    }
                }
                foreach (string link in allStringLinks)
                {
                    if (allCount > MAX_PAGES)
                    {
                        break;
                    }

                    Uri newUri = GetUrl(url.AbsoluteUri, link);
                    if (newUri == null || newUri.Host != mainHost || sites.Contains(newUri.AbsoluteUri))
                    {
                        continue;
                    }

                    var response = GetLinkResponse(newUri);
                    
                    if (response != null)
                    {
                        allCount++;
                        allLinks.Add(newUri);

                        int responseCode = (int)response.StatusCode;
                        allSites.WriteLine("{0} - {1}", newUri.AbsoluteUri, responseCode);

                        if (responseCode != 200 && responseCode != 301)
                        {
                            if (!deadLinks.Contains(newUri.AbsoluteUri))
                            {
                                deadLinks.Add(newUri.AbsoluteUri, responseCode);
                            }
                        }

                        response.Close();
                    }
                }

                foreach (var link in allLinks)
                {
                    if (allCount > MAX_PAGES)
                    {
                        break;
                    }

                    if (!sites.Contains(link.AbsoluteUri))
                    {
                        ParsePage(link);
                    }

                    Console.WriteLine(allCount.ToString());
                }
            }
        }

        static void StartParse(string url)
        {
            Uri baseUri = GetUrl(url);
            if (baseUri == null)
            {
                Console.WriteLine("Wrong URL {0}", url);
                return;
            }

            var mainResponse = GetLinkResponse(baseUri);
            if (mainResponse != null)
            {
                mainHost = baseUri.Host;
            }

            allCount = 0;
            web = new WebClient();
            web.Proxy = new System.Net.WebProxy();
            web.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
            ParsePage(baseUri);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not enough arguments\n");
                return;
            }
            else
            {
                if (args.Length > 1)
                {
                    Console.WriteLine("Too many arguments\n");
                    return;
                }
            }

            allSites.WriteLine("First {0} pages", MAX_PAGES);
            StartParse(args[0]);

            if (deadLinks.Count > 0)
            {
                allSites.WriteLine();
                allSites.WriteLine("Dead links: ");
                foreach (DictionaryEntry pair in deadLinks)
                {
                    allSites.WriteLine("{0} - {1}", pair.Key, pair.Value);
                }
            }

            allSites.Close();
        }
    }
}
