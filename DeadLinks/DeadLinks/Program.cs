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
        static string mainHost;
        static Uri baseUri;

        static HttpWebResponse GetLinkResponse(Uri baseUri, out Uri newUrl, string relativeUrl = "")
        {
            Uri url = null;
            newUrl = null;
            if (relativeUrl == null || relativeUrl == "")
            {
                url = baseUri;
            }
            else
            {
                if (!Uri.TryCreate(baseUri, relativeUrl, out url))
                {
                    return null;
                }
            }

            newUrl = url;
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
            req.MaximumAutomaticRedirections = 5;
            req.Timeout = 5000;

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
                if (errResp != null)
                {
                    errResp.Close();
                }

                //if (errResp != null)
                //{
                //    Console.WriteLine("Dead link : {0} - {1}", errResp.ResponseUri, (int)errResp.StatusCode);
                //}
                
                return errResp;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return response;
        }

        static void ParsePage(Uri url)
        {
            HashSet<string> allStringLinks = new HashSet<string>();
            HashSet<Uri> allLinks = new HashSet<Uri>();

            using (WebClient web = new WebClient())
            {
                web.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
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

                foreach (Match match in r.Matches(code))
                {
                    string newLink = match.Groups["url"].Value;
                    allStringLinks.Add(newLink);
                }
                foreach (string link in allStringLinks)
                {
                    Uri newUri;
                    var response = GetLinkResponse(url, out newUri, link);
                    int responseCode = 0;
                    if (response != null)
                    {
                        string newHost = newUri.Host;
                        if (newHost == mainHost && sites.Add(response.ResponseUri.ToString()))
                        {
                            allLinks.Add(newUri);
                            responseCode = (int)response.StatusCode;
                            allSites.WriteLine("{0} - {1}", response.ResponseUri, responseCode);

                            if (responseCode != 200 && responseCode != 301)
                            {
                                if (!deadLinks.Contains(response.ResponseUri.ToString()))
                                {
                                    deadLinks.Add(response.ResponseUri.ToString(), responseCode);
                                }
                            }
                        }

                        response.Close();
                    }
                }
                foreach (var link in allLinks)
                {
                    ParsePage(link);
                }
            }
        }

        static void StartParse(string url)
        {
            baseUri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out baseUri))
            {
                Console.WriteLine("Wrong URL {0}", url);
                return;
            }

            Uri temp;
            var mainResponse = GetLinkResponse(baseUri, out temp, null);
            if (mainResponse != null)
            {
                mainHost = temp.Host;
            }

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
