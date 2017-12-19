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

namespace AS365Cookie
{
    public class Program
    {
        // Import DLL to set Cookies in IE
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        public void GetCookie365(string[] args)
        {
            bool quiet = false;
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

            if (CommandLine["s"] == null) { }
            //Console.WriteLine("Error: SharePoint URL not specified !\n\nUsage: Cookie365 -s URL [-u user@domain.com | -d domain.com] [-p {password}] [-quiet] [-mount [disk] [-homedir]]");
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
                            //("Username cannot be empty");
                            return;
                        }
                    }

                    // If password is specified, use it, otherwise try integrated authentication
                    if (CommandLine["p"] != null) { password = CommandLine["p"]; useIntegratedAuth = false; }

                    // Set the flag for quiet mode
                    if (CommandLine["quiet"] != null) { quiet = true; }

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


                    // Run Asynchronously and wait for cookie retrieval
                    RunAsync(sharepointUri, username, password, useIntegratedAuth, !quiet);
                    System.Threading.Thread.Sleep(2000);
                    // If
                    if (SpoAuthUtility.Current != null)
                    {
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
                                    if (mount)
                                    {
                                        try
                                        {
                                            DoMap(quiet, sharepointUri, disk, homedir);
                                            OtherMaps(quiet, CommandLine);
                                        }
                                        catch (Exception e)
                                        { }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { }

                    }
                }
                catch (Exception e)
                { }
            }
        }

        private static void OtherMaps(bool quiet, Args CommandLine)
        {
            if (CommandLine["m"] != null)
            {
                //String username = System.DirectoryServices.AccountManagement.UserPrincipal.Current.UserPrincipalName;
                String username = Environment.UserName + "@ashbyschool.org.uk";
                String password = CommandLine["p"];

                //if (username != null) { if (CommandLine["d"] != null) username = username.Split('@')[0] + "@" + CommandLine["d"]; }
                String line = CommandLine["m"];
                String[] parts = line.Split(',');
                String[] baseparts = parts[0].Split('~');
                Uri tempuri = new Uri(baseparts[1]);
                if (CommandLine["u"] != null)
                {
                    RunAsync(tempuri, username, password, false, !quiet).Wait();

                }
                else
                {

                    RunAsync(tempuri, username, null, true, !quiet).Wait();

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
            // Uncomment to make drives persist
            // cmdArgs = String.Format("{0} /PERSISTENT:YES", cmdArgs); 
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", cmdArgs);
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.UseShellExecute = false;
            Process.Start();
            Process.WaitForExit();
            String output = Process.StandardOutput.ReadToEnd();

        }

        static async Task RunAsync(Uri sharepointUri, string username, string password, bool useIntegratedAuth, bool verbose)
        {
            await SpoAuthUtility.Create(sharepointUri, username, password, useIntegratedAuth, verbose);
        }
    }
}
