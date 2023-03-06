using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Threading;
using System.Text.RegularExpressions;
using System.Configuration;

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

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                while (pageNumber < totalPages)
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://www.lesegais.ru/open-area/graphql"),
                        Headers =
                        {
                            { "Accept", "*/*" },
                            { "Accept-Language", "en-US,en;q=0.9,ru;q=0.8,ja;q=0.7,hu;q=0.6,uk;q=0.5,de;q=0.4" },
                            { "Connection", "keep-alive" },
                            { "Origin", "https://www.lesegais.ru" },
                            { "Referer", "https://www.lesegais.ru/open-area/deal" },
                            { "Sec-Fetch-Dest", "empty" },
                            { "Sec-Fetch-Mode", "cors" },
                            { "Sec-Fetch-Site", "same-origin" },
                            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36" },
                            { "sec-ch-ua", "Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"Google Chrome\";v=\"110\"" },
                            { "sec-ch-ua-mobile", "?0" },
                            { "sec-ch-ua-platform", "\"Windows\"" },
                        },
                        Content = new StringContent(queryWoodDealContent, Encoding.UTF8, "application/json")
                    };


                    using (var dbManager = new DatabaseManager(appSettings["databaseConnectionString"]))
                    using (SqlCommand command = new SqlCommand(insertQuery, dbManager.GetConnection()))
                    using (var response = await httpClient.SendAsync(request)) 

                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();

                        Root deals = JsonConvert.DeserializeObject<Root>(body);

                        if (pageNumber == 0)
                        {
                            var total = deals.data.searchReportWoodDeal.total;
                            totalPages = (int)Math.Ceiling((double)total / pageSize);
                        }


                        var sellerNameParam = new SqlParameter("@SellerName", SqlDbType.NVarChar);
                        var sellerInnParam = new SqlParameter("@SellerInn", SqlDbType.Char);
                        var buyerNameParam = new SqlParameter("@BuyerName", SqlDbType.NVarChar);
                        var buyerInnParam = new SqlParameter("@BuyerInn", SqlDbType.Char);
                        var woodVolumeBuyerParam = new SqlParameter("@WoodVolumeBuyer", SqlDbType.Float);
                        var woodVolumeSellerParam = new SqlParameter("@WoodVolumeSeller", SqlDbType.Float);
                        var dealDateParam = new SqlParameter("@DealDate", SqlDbType.DateTime);
                        var dealNumberParam = new SqlParameter("@DealNumber", SqlDbType.NVarChar);

                        // add parameters to command
                        command.Parameters.Add(sellerNameParam);
                        command.Parameters.Add(sellerInnParam);
                        command.Parameters.Add(buyerNameParam);
                        command.Parameters.Add(buyerInnParam);
                        command.Parameters.Add(woodVolumeBuyerParam);
                        command.Parameters.Add(woodVolumeSellerParam);
                        command.Parameters.Add(dealDateParam);
                        command.Parameters.Add(dealNumberParam);

                        foreach (var deal in deals.data.searchReportWoodDeal.content)
                        {

                            if (IsValidName(deal.sellerName))
                            {
                                sellerNameParam.Value = deal.sellerName;
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidInn(deal.sellerInn))
                            {
                                sellerInnParam.Value = deal.sellerInn;
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidName(deal.buyerName))
                            {
                                buyerNameParam.Value = deal.buyerName;
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidInn(deal.sellerInn))
                            {
                                buyerInnParam.Value = deal.buyerInn;
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidDouble(deal.woodVolumeBuyer))
                            {
                                woodVolumeBuyerParam.Value = Math.Round(Double.Parse(deal.woodVolumeBuyer), 2);
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidDouble(deal.woodVolumeSeller))
                            {
                                woodVolumeSellerParam.Value = Math.Round(Double.Parse(deal.woodVolumeSeller), 2);
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidDate(deal.dealDate))
                            {
                                dealDateParam.Value = deal.dealDate;
                            }
                            else
                            {
                                continue;
                            }

                            if (IsValidNumeric(deal.dealNumber))
                            {
                                dealNumberParam.Value = deal.dealNumber;
                            }
                            else
                            {
                                continue;
                            }

                            int rowsAffected = command.ExecuteNonQuery();

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

                    queryWoodDealContent = queryWoodDealContent.Replace($"\"number\":{pageNumber}", $"\"number\":{pageNumber + 1}");
                    pageNumber++;

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }


        public static bool IsValidName(string inputName)
        {
            return !string.IsNullOrEmpty(inputName) && inputName.Length <= 255;
        }

        public static bool IsValidInn(string inn)
        {
            if (inn.Length == 10)
            {
                int[] multipliers = { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int sum = 0;

                for (int i = 0; i < 9; i++)
                {
                    sum += int.Parse(inn[i].ToString()) * multipliers[i];
                }

                int control = sum % 11;
                control = (control == 10) ? 0 : control;

                return control == int.Parse(inn[9].ToString());
            }
            else if (inn.Length == 12)
            {
                int[] multipliers1 = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int[] multipliers2 = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

                int sum1 = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum1 += int.Parse(inn[i].ToString()) * multipliers1[i];
                }

                int control1 = sum1 % 11;
                control1 = (control1 == 10) ? 0 : control1;

                if (control1 != int.Parse(inn[10].ToString()))
                {
                    return false;
                }

                int sum2 = 0;
                for (int i = 0; i < 11; i++)
                {
                    sum2 += int.Parse(inn[i].ToString()) * multipliers2[i];
                }

                int control2 = sum2 % 11;
                control2 = (control2 == 10) ? 0 : control2;

                return control2 == int.Parse(inn[11].ToString());
            }
            else
            {
                return false;
            }
        }

        private static bool IsValidDouble(string woodVolume)
        {
            return Regex.IsMatch(woodVolume, @"^\d+(\.\d+)?$");
        }

        private static bool IsValidDate(string dealDate)
        {
            if (Regex.IsMatch(dealDate, @"^(19|20)\d{2}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$"))
            {
                return DateTime.Parse(dealDate) <= DateTime.Now;
            }
            else
            {
                return false;
            }
        }

        private static bool IsValidNumeric(string dealNumber)
        {
            return Regex.IsMatch(dealNumber, @"^[0-9]+$");
        }
    }
}