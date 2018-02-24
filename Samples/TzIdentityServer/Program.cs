using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TzIdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "TzIdentityServer";

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
