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
        static void ParsePage(string url)
        {
            Uri siteUrl;
            HashSet<string> sites = new HashSet<string>();
            Hashtable deadLinks = new Hashtable();
            StreamWriter allSites = new StreamWriter("report.txt");
            try
            {
                siteUrl = new Uri(url);
                
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(siteUrl);
                req.Timeout = 3000;
                req.MaximumAutomaticRedirections = 5;

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                int responseCode = (int)response.StatusCode;
                allSites.WriteLine("{0} - {1}", url, responseCode);

                if (responseCode % 100 > 3)
                {
                    deadLinks.Add(url, responseCode);
                }
                sites.Add(url);
                
                response.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("{0} - wrong url.", url);
                Console.WriteLine(e.Message);
                return;
            }

            Regex r = new Regex(@"<a.*?href=(""|')(?<href>.*?)(""|').*?>(?<value>.*?)</a>");
            string mainDomain = siteUrl.AbsoluteUri, mainHost = siteUrl.Host;
            using (WebClient web = new WebClient())
            {
                string code = web.DownloadString(url);

                foreach (Match match in r.Matches(code))
                {
                    string site = match.Groups["href"].Value;
                    if (site.Length > 0 && site[0] == '#')
                    {
                        site = url;
                    }
                    else
                    {
                        if (site.Length > 0 && site[0] == '/')
                        {
                            if (site.Length > 1 && site[1] == '/')
                            {
                                site = site.Substring(2, site.Length - 2);
                            }
                            else
                            {
                                site = site.Substring(1, site.Length - 1);
                                site = mainDomain + site;
                            }
                        }
                    }

                    Uri newSite;
                    try
                    {
                        newSite = new Uri(site);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} - {1}", site, e.Message);
                        continue;
                    }

                    if (newSite.Host != mainHost || !sites.Add(site))
                    {
                        continue;
                    }

                    try
                    {
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(newSite);
                        req.Timeout = 3000;
                        req.MaximumAutomaticRedirections = 5;

                        HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                        int responseCode = (int)response.StatusCode;
                        allSites.WriteLine(site + " - " + responseCode);

                        if (responseCode % 100 > 3)
                        {
                            deadLinks.Add(site, responseCode);
                        }

                        response.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} - {1}", site, e.Message);
                    }
                }
            }

            if (deadLinks.Count > 0)
            {
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
