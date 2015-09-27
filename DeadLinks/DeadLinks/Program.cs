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
        static HttpWebResponse GetLinkResponse(string baseUrl, out Uri newUrl, string relativeUrl = "")
        {
            Uri baseUri = null;
            newUrl = null;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUri))
            {
                return null;
            }

            Uri url = null;
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

            req.Method = "GET";
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

        static void ParsePage(string url)
        {
            HashSet<string> sites = new HashSet<string>();
            Hashtable deadLinks = new Hashtable();
            StreamWriter allSites = new StreamWriter("report.txt");
            Regex r = new Regex(@"(?: href\s*=)(?:[\s""']*)(?!#|mailto|location.|javascript|.*css|.*this\.)(?<url>.*?)(?:[\s>""'])", RegexOptions.IgnoreCase);
            Uri temp;
            var mainResponse = GetLinkResponse(url, out temp, null);
            if (mainResponse != null)
            {
                string hostName = temp.Host;
                using (WebClient web = new WebClient())
                {
                    string code = web.DownloadString(url);

                    foreach (Match match in r.Matches(code))
                    {
                        string newLink = match.Groups["url"].Value;
                        Uri newUri;
                        var response = GetLinkResponse(url, out newUri, newLink);
                        int responseCode = 0;
                        string newHost = response == null ? "" : newUri.Host;
                        if (response != null)
                        {
                            if (newHost == hostName)
                            {
                                responseCode = (int)response.StatusCode;

                                if (sites.Add(response.ResponseUri.ToString()))
                                {
                                    allSites.WriteLine("{0} - {1}", response.ResponseUri, responseCode);
                                }

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
                }
            }
            else 
            {
                Console.WriteLine("Bad link : {0}", url);
            }
            
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

            ParsePage(args[0]);
        }
    }
}
