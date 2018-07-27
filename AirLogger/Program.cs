using Sulakore.Crypto;
using Sulakore.Network;
using Sulakore.Network.Protocol;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AirLogger
{
    class Program
    {
        static void Main(string[] args) => Task.Run(() => MainAsync(args)).Wait();

        static async Task MainAsync(string[] args)
        {
            string hostname = "game-us.habbo.com";
            IPHostEntry entry = await Dns.GetHostEntryAsync(hostname);
            Logger logger = new Logger(entry.AddressList[0].ToString(), 30000);
            await logger.Start();
            Console.ReadKey();
        }
    }
}