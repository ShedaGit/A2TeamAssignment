using System;
using System.Configuration;
using System.Threading;

namespace A2TeamAssignment
{
    class Program
    {
        static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var insertQuery = appSettings["insertQuery"];
            var queryWoodDealContent = appSettings["searchReportWoodDeal"];

            var dealProcessor = new WoodDealsProcessor(queryWoodDealContent, insertQuery);

            var timer = new Timer(ParseWoodDeals, dealProcessor, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static async void ParseWoodDeals(object state)
        {
            var dealProcessor = (WoodDealsProcessor)state;
            await dealProcessor.ProcessDealsAsync();
        }
    }
}
