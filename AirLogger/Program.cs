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
            Logger logger = new Logger("34.232.39.163", 30000);
            await logger.Start();
            while (true) ;
        }
    }
}