using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Collections.Generic;

namespace A2TeamAssignment
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int pageSize = 100;
            int pageNumber = 0;
            int totalPages = 1;

            var appSettings = ConfigurationManager.AppSettings;
            var insertQuery = appSettings["insertQuery"];
            var queryWoodDealContent = appSettings["searchReportWoodDeal"];

            using (var httpClientManager = new HttpClientManager())
            {
                try
                {
                    var body = await httpClientManager.GetDealsAsync(queryWoodDealContent);

                    while (pageNumber < totalPages)
                    {
                        using (var dbManager = new DatabaseManager(appSettings["databaseConnectionString"]))
                        using (SqlCommand command = new SqlCommand(insertQuery, dbManager.GetConnection()))
                        {
                            Root deals = JsonConvert.DeserializeObject<Root>(body);

                            if (pageNumber == 0)
                            {
                                var total = deals.data.searchReportWoodDeal.total;
                                totalPages = (int)Math.Ceiling((double)total / pageSize);
                            }

                            var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@SellerName", SqlDbType.NVarChar),
                            new SqlParameter("@SellerInn", SqlDbType.Char),
                            new SqlParameter("@BuyerName", SqlDbType.NVarChar),
                            new SqlParameter("@BuyerInn", SqlDbType.Char),
                            new SqlParameter("@WoodVolumeBuyer", SqlDbType.Float),
                            new SqlParameter("@WoodVolumeSeller", SqlDbType.Float),
                            new SqlParameter("@DealDate", SqlDbType.DateTime),
                            new SqlParameter("@DealNumber", SqlDbType.NVarChar)
                        };

                            command.Parameters.AddRange(parameters.ToArray());

                            foreach (var deal in deals.data.searchReportWoodDeal.content)
                            {
                                var validator = new WoodDealsValidator();

                                int rowsAffected = 0;

                                if (validator.IsValidDeal(deal, parameters))
                                {
                                    parameters[0].Value = deal.sellerName;
                                    parameters[1].Value = deal.sellerInn;
                                    parameters[2].Value = deal.buyerName;
                                    parameters[3].Value = deal.buyerInn;
                                    parameters[4].Value = Math.Round(Double.Parse(deal.woodVolumeBuyer), 2);
                                    parameters[5].Value = Math.Round(Double.Parse(deal.woodVolumeSeller), 2);
                                    parameters[6].Value = deal.dealDate;
                                    parameters[7].Value = deal.dealNumber;

                                    rowsAffected = dbManager.ExecuteNonQuery(command);
                                }

                                if (rowsAffected > 0)
                                {
                                    Console.WriteLine("The deal was inserted successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("The deal already exists in the database.");
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException($"Error requesting deals: {ex.Message}", ex);
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Error executing command: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected Error: {ex.Message}", ex);
                }

                queryWoodDealContent = queryWoodDealContent.Replace($"\"number\":{pageNumber}", $"\"number\":{pageNumber + 1}");
                pageNumber++;

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
