using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Linq;
using Leaf.xNet;
using Newtonsoft.Json;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace MassTokenChecker
{
    class Program
    {
        static void Main()
        {
            int Valid = 0;
            int Locked = 0;
            int Invalid = 0;
            Console.Title = $"AstroChecker  |   {Valid} Valid   |   {Locked} Locked  |   {Invalid} Invalid";
            List<string> valid = new List<string>();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(@"
           ░█████╗░░██████╗████████╗██████╗░░█████╗░  ░█████╗░██╗░░██╗███████╗░█████╗░██╗░░██╗███████╗██████╗░
           ██╔══██╗██╔════╝╚══██╔══╝██╔══██╗██╔══██╗  ██╔══██╗██║░░██║██╔════╝██╔══██╗██║░██╔╝██╔════╝██╔══██╗
           ███████║╚█████╗░░░░██║░░░██████╔╝██║░░██║  ██║░░╚═╝███████║█████╗░░██║░░╚═╝█████═╝░█████╗░░██████╔╝
           ██╔══██║░╚═══██╗░░░██║░░░██╔══██╗██║░░██║  ██║░░██╗██╔══██║██╔══╝░░██║░░██╗██╔═██╗░██╔══╝░░██╔══██╗
           ██║░░██║██████╔╝░░░██║░░░██║░░██║╚█████╔╝  ╚█████╔╝██║░░██║███████╗╚█████╔╝██║░╚██╗███████╗██║░░██║
           ╚═╝░░╚═╝╚═════╝░░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░  ░╚════╝░╚═╝░░╚═╝╚══════╝░╚════╝░╚═╝░░╚═╝╚══════╝╚═╝░░╚═╝");
            Console.ResetColor();
            Console.WriteLine("Enter path to tokens file: ");
            string tokens = Console.ReadLine();
            tokens = tokens.Replace("\"", "");
            //RemoveDupe(tokens);
            tokens = File.ReadAllText(tokens);
            foreach (Match match in Regex.Matches(tokens, @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}|mfa\.[\w-]{84}"))
            {
                try
                {
                    var wc = new WebClient();
                    wc.Headers.Add("Content-Type", "application/json");
                    wc.Headers.Add(HttpRequestHeader.Authorization, match.ToString());
                    wc.DownloadString("https://discordapp.com/api/v8/users/@me/guilds");
                    wc.Dispose();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" [VALID] {match.Value}"); Valid += 1;
                    Console.ResetColor();
                    valid.Add(match.Value);
                    //var request = new HttpRequest();
                    //{
                    //    request.UseCookies = false;
                    //    request.AddHeader("Authorization", match.ToString());
                    //    request.Get("https://discordapp.com/api/v8/users/@me");
                    //    string test = request.Response.ToString();
                    //    dynamic test2 = JsonConvert.DeserializeObject(test);
                    //    if (test2.email == string.Empty)
                    //    {
                    //        Console.WriteLine($" [VALID - Unverified] {match.Value}"); Valid += 1;
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine($" [VALID + Verified] {match.Value}"); Valid += 1;
                    //    }
                    //}
                }
                catch (WebException e)
                {
                    HttpWebResponse response = (HttpWebResponse)e.Response;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" [INVALID] {match.Value}"); Invalid += 1;
                        Console.ResetColor();
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($" [LOCKED] {match.Value}"); Locked += 1;
                        Console.ResetColor();
                    }
                    else if (response.StatusCode > HttpStatusCode.Forbidden)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            var wc = new WebClient();
                            wc.Headers.Add("Content-Type", "application/json");
                            wc.Headers.Add(HttpRequestHeader.Authorization, match.ToString());
                            wc.DownloadString("https://discordapp.com/api/v8/users/@me/guilds");
                            wc.Dispose();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($" [VALID] {match.Value}"); Valid += 1;
                            Console.ResetColor();
                            valid.Add(match.Value);
                        }
                        catch (WebException x)
                        {
                            HttpWebResponse rsp = (HttpWebResponse)x.Response;
                            if (rsp.StatusCode == HttpStatusCode.Unauthorized) // 401 Token Invalid
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($" [INVALID] {match.Value}"); Invalid += 1;
                                Console.ResetColor();
                            }
                            else if (rsp.StatusCode == HttpStatusCode.Forbidden) // 403 Token Locked
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($" [LOCKED] {match.Value}"); Locked += 1;
                                Console.ResetColor();
                            }
                            else if (rsp.StatusCode == (HttpStatusCode)429)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($" [RATE LIMITED] {match.Value}");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                Console.Title = $"AstroChecker  |   {Valid} Valid   |   {Locked} Locked  |   {Invalid} Invalid";
            }
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(@"
           ░█████╗░░██████╗████████╗██████╗░░█████╗░  ░█████╗░██╗░░██╗███████╗░█████╗░██╗░░██╗███████╗██████╗░
           ██╔══██╗██╔════╝╚══██╔══╝██╔══██╗██╔══██╗  ██╔══██╗██║░░██║██╔════╝██╔══██╗██║░██╔╝██╔════╝██╔══██╗
           ███████║╚█████╗░░░░██║░░░██████╔╝██║░░██║  ██║░░╚═╝███████║█████╗░░██║░░╚═╝█████═╝░█████╗░░██████╔╝
           ██╔══██║░╚═══██╗░░░██║░░░██╔══██╗██║░░██║  ██║░░██╗██╔══██║██╔══╝░░██║░░██╗██╔═██╗░██╔══╝░░██╔══██╗
           ██║░░██║██████╔╝░░░██║░░░██║░░██║╚█████╔╝  ╚█████╔╝██║░░██║███████╗╚█████╔╝██║░╚██╗███████╗██║░░██║
           ╚═╝░░╚═╝╚═════╝░░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░  ░╚════╝░╚═╝░░╚═╝╚══════╝░╚════╝░╚═╝░░╚═╝╚══════╝╚═╝░░╚═╝");
            Console.ResetColor();
            Console.WriteLine($"   Valid Tokens: {Valid}\n   Locked Tokens: {Locked}\n   Invalid Tokens: {Invalid}");
            File.AppendAllText(".\\ValidTokens.txt", string.Join("\n", valid));
            Console.WriteLine("   Saved all valid tokens to ValidTokens.txt");
            Console.Read();
        }
        //public static void RemoveDupe(string path)
        //{
        //    string[] lines = File.ReadAllLines(path);
        //    lines = lines.GroupBy(x => x).Where(z => z.Count() > 1).Select(z => z.Key).ToArray();
        //    File.WriteAllLines(path, lines);
        //}
    }
}
