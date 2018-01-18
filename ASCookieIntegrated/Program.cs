using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Cookie365;

namespace Cookie365
{
    class Program
    {
        // Import DLL to set Cookies in IE
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        static void Main(string[] args)
        {
            // Set Default Argument values
            bool quiet = false;
            bool debug = false;
            string sharepointUrl = null;
            Uri sharepointUri = null;
            string username = null;
            string password = null;
            bool useIntegratedAuth = true;
            string disk = null;
            bool mount = false;
            double expire = 0;
            string homedir = "";


            //Parse args
            Args CommandLine = new Args(args);
            if (CommandLine["m"] != null)
            {
                OtherMaps(false, CommandLine);
            }
            else
            {
                if (CommandLine["s"] == null)
                    Console.WriteLine("Error: SharePoint URL not specified !\n\nUsage: Cookie365 -s URL [-u user@domain.com | -d domain.com] [-p {password}] [-quiet] [-mount [disk] [-homedir]]");
                else
                {
                    try
                    {
                        // Retrieve SharePoint URL and Create URI
                        sharepointUrl = CommandLine["s"];
                        sharepointUri = new Uri(sharepointUrl);

                        // If username is specified use it, otherwise get the user UPN from AD
                        if (CommandLine["u"] != null) username = CommandLine["u"];
                        else
                        {
                            username = System.DirectoryServices.AccountManagement.UserPrincipal.Current.UserPrincipalName;
                            if (username != null) { if (CommandLine["d"] != null) username = username.Split('@')[0] + "@" + CommandLine["d"]; }
                            else
                            {
                                Console.WriteLine("Username cannot be empty");
                                return;
                            }
                        }

                        // If password is specified, use it, otherwise try integrated authentication
                        if (CommandLine["p"] != null) { password = CommandLine["p"]; useIntegratedAuth = false; }

                        // Set the flag for quiet mode
                        if (CommandLine["quiet"] != null)
                        {
                            quiet = true;
                        }
                        else if (CommandLine["debug"] != null) { debug = true; }

                        // If asked to mount sharepoint as a share
                        disk = CommandLine["mount"];
                        if (disk != null)
                        {
                            mount = true;
                            if (disk == "true") disk = "*";
                        }

                        if (CommandLine["expire"] != null)
                        {
                            expire = Convert.ToDouble(CommandLine["expire"]);
                        }

                        if (CommandLine["homedir"] != null)
                        {
                            String user = username.Split('@')[0];
                            String domain = username.Split('@')[1];
                            homedir = "DavWWWRoot\\personal\\" + user + "_" + domain.Split('.')[0] + "_" + domain.Split('.')[1] + "_" + domain.Split('.')[2] + "\\Documents";
                        }

                        // if not quiet, display parameters
                        if (!quiet)
                        {
                            // Message
                            Console.WriteLine("============= Cookie365 v0.7 - (C)opyright 2014-2017 Fabio Cuneaz =============\n");
                            Console.WriteLine("SharePoint URL: " + sharepointUrl);
                            Console.WriteLine("User: " + username);
                            Console.WriteLine("Use Windows Integrated Authentication: " + useIntegratedAuth.ToString());
                            if (homedir != "") Console.WriteLine("HomeDir: " + homedir);
                            if (mount) Console.WriteLine("Mount as disk: " + disk);
                        }

                        // Run Asynchronously and wait for cookie retrieval
                        RunAsync(sharepointUri, username, password, useIntegratedAuth, !quiet, debug).Wait();

                        // If
                        if (SpoAuthUtility.Current != null)
                        {
                            if (!quiet) Console.Write("Setting Cookies in OS...");
                            try
                            {
                                // Create the cookie collection object for sharepoint URI
                                CookieCollection cookies = SpoAuthUtility.Current.cookieContainer.GetCookies(sharepointUri);

                                // Extract the base URL in case the URL provided contains nested paths (e.g. https://contoso.sharepoint.com/abc/ddd/eed)
                                // The cookie has to be set for the domain (contoso.sharepoint.com), otherwise it will not work
                                String baseUrl = sharepointUri.Scheme + "://" + sharepointUri.Host;

                                if (InternetSetCookie(baseUrl, null, cookies["FedAuth"].ToString() + "; Expires = " + cookies["FedAuth"].Expires.AddMinutes(expire).ToString("R")))
                                {
                                    if (InternetSetCookie(baseUrl, null, cookies["rtFA"].ToString() + "; Expires = " + cookies["rtFA"].Expires.AddMinutes(expire).ToString("R"))) 
                                    {
                                        if (!quiet)
                                        {
                                            Console.WriteLine("[OK]. Expiry = " + cookies["FedAuth"].Expires.AddMinutes(expire).ToString("R"));
                                        }
                                        if (mount)
                                        {
                                            try
                                            {
                                                String cmdArgs = "/c net use " + disk + " \\\\" + sharepointUri.Host + "@ssl" + sharepointUri.PathAndQuery.Replace("/", "\\") + homedir;
                                                if (!quiet) Console.Write("Mounting Share..." + cmdArgs);
                                                System.Diagnostics.Process Process = new System.Diagnostics.Process();
                                                Process.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", cmdArgs);
                                                Process.StartInfo.RedirectStandardOutput = true;
                                                Process.StartInfo.UseShellExecute = false;
                                                //Process.StartInfo.CreateNoWindow = true;
                                                Process.Start();
                                                Process.WaitForExit();
                                                String output = Process.StandardOutput.ReadToEnd();
                                                if (!quiet)
                                                {
                                                    Console.WriteLine("[OK]");
                                                    Console.WriteLine(output);
                                                }
                                            }
                                            catch (Exception e)
                                            { Console.WriteLine("[ERROR Mounting Share]:" + e.Message); }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            { Console.WriteLine("[ERROR setting Cookies]:" + e.Message); }

                        }
                    }
                    catch (Exception e)
                    { Console.WriteLine("[ERROR]:" + e.Message); }
                }
            }
        }

        private static void OtherMaps(bool quiet, Args CommandLine)
        {
            if (CommandLine["m"] != null)
            {
                //String username = System.DirectoryServices.AccountManagement.UserPrincipal.Current.UserPrincipalName;
                String username = Environment.UserName;  //+ "@ashbyschool.org.uk";
                String password = CommandLine["p"];

                //if (username != null) { if (CommandLine["d"] != null) username = username.Split('@')[0] + "@" + CommandLine["d"]; }
                String line = CommandLine["m"];
                String[] parts = line.Split(',');
                String[] baseparts = parts[0].Split('~');
                Uri tempuri = new Uri(baseparts[1]);
                if (CommandLine["u"] != null)
                {
                    RunAsync(tempuri, username, password, false, false, false).Wait();

                }



                CookieCollection cookies = SpoAuthUtility.Current.cookieContainer.GetCookies(tempuri);
                String baseUrl = tempuri.Scheme + "://" + tempuri.Host;
                InternetSetCookie(baseUrl, null, cookies["FedAuth"].ToString() + "; Expires = " + cookies["FedAuth"].Expires.AddMinutes(0).ToString("R"));
                InternetSetCookie(baseUrl, null, cookies["rtFA"].ToString() + "; Expires = " + cookies["rtFA"].Expires.AddMinutes(0).ToString("R"));


                foreach (String part in parts)
                {
                    String[] drivepaths = part.Split('~');
                    String drive = drivepaths[0] + ":";
                    String path = drivepaths[1];
                    Uri sharepointUri = new Uri(path);
                    DoMap(quiet, sharepointUri, drive, "");
                }
            }
            
        }

        private static void DoMap(bool quiet, Uri sharepointUri, string disk, string homedir)
        {
            String cmdArgs = "/c net use " + disk + " \\\\" + sharepointUri.Host + "@ssl" + sharepointUri.PathAndQuery.Replace("/", "\\") + homedir;
            cmdArgs = cmdArgs.Replace("\\\\", "\\");
            // Uncomment to make drives persist
            // cmdArgs = String.Format("{0} /PERSISTENT:YES", cmdArgs); 
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", cmdArgs);
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.UseShellExecute = false;
            //Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            Process.WaitForExit();
            String output = Process.StandardError.ReadToEnd();
            Console.WriteLine(output);
        }

        static async Task RunAsync(Uri sharepointUri, string username, string password, bool useIntegratedAuth, bool verbose, bool debug)
        {
            await SpoAuthUtility.Create(sharepointUri, username, password, useIntegratedAuth, verbose, debug);
        }
    }
}