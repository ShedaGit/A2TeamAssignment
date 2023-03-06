using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace A2TeamAssignment
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var insertQuery = appSettings["insertQuery"];
            var queryWoodDealContent = appSettings["searchReportWoodDeal"];

            var dealProcessor = new WoodDealsProcessor(queryWoodDealContent, insertQuery);

            await dealProcessor.ProcessDealsAsync();
        }
    }
}
