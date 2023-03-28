using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WoodDealsParser
{
    public class WoodDealsProcessor : IDisposable
    {
        private readonly string _insertQuery;
        private readonly int _pageSize = 100;

        private string _queryWoodDealContent;
        private int _totalPages = 1;

        private HttpClientManager _httpClientManager;
        private DatabaseManager _dbManager;

        public WoodDealsProcessor(string queryWoodDealContent, string insertQuery)
        {
            _queryWoodDealContent = queryWoodDealContent;
            _insertQuery = insertQuery;
        }

        public void Dispose()
        {
            _httpClientManager?.Dispose();
            _dbManager?.Dispose();
        }

        public async Task ProcessDealsAsync()
        {
            using (_httpClientManager = new HttpClientManager())
            using (var httpClient = new HttpClient())
            {
                try
                {
                    int pageNumber = 0;

                    using (_dbManager = new DatabaseManager(ConfigurationManager.AppSettings["databaseConnectionString"]))
                    using (SqlCommand command = new SqlCommand(_insertQuery, _dbManager.GetConnection()))
                    {
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

                        while (pageNumber < _totalPages)
                        {
                            var body = await _httpClientManager.GetDealsAsync(_queryWoodDealContent);

                            Root deals = JsonConvert.DeserializeObject<Root>(body);

                            if (pageNumber == 0)
                            {
                                var total = deals.data.searchReportWoodDeal.total;
                                _totalPages = (int)Math.Ceiling((double)total / _pageSize);
                            }

                            foreach (var deal in deals.data.searchReportWoodDeal.content)
                            {
                                var validator = new WoodDealsValidator();

                                if (validator.IsValidDeal(deal, parameters))
                                {
                                    parameters[0].Value = deal.sellerName;
                                    parameters[1].Value = deal.sellerInn;
                                    parameters[2].Value = deal.buyerName;
                                    parameters[3].Value = deal.buyerInn;
                                    parameters[4].Value = deal.woodVolumeBuyer;
                                    parameters[5].Value = deal.woodVolumeSeller;
                                    parameters[6].Value = deal.dealDate;
                                    parameters[7].Value = deal.dealNumber;

                                    _dbManager.ExecuteNonQuery(command);
                                }
                            }

                            _queryWoodDealContent = _queryWoodDealContent.Replace($"\"number\":{pageNumber}", $"\"number\":{pageNumber + 1}");
                            pageNumber++;

                            await Task.Delay(TimeSpan.FromSeconds(10));
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
            }
        }
    }
}
