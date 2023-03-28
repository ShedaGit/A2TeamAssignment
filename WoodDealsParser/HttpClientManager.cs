using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WoodDealsParser
{
    public class HttpClientManager : IDisposable
    {
        private HttpClient _httpClient;

        public HttpClientManager()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
                BaseAddress = new Uri("https://www.lesegais.ru")
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,ru;q=0.8,ja;q=0.7,hu;q=0.6,uk;q=0.5,de;q=0.4");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.lesegais.ru");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.lesegais.ru/open-area/deal");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"Google Chrome\";v=\"110\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<string> GetDealsAsync(string query)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://www.lesegais.ru/open-area/graphql"),
                Headers =
                {
                    { "Connection", "keep-alive" },
                    { "Sec-Fetch-Dest", "empty" },
                    { "Sec-Fetch-Mode", "cors" },
                    { "Sec-Fetch-Site", "same-origin" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36" },
                },
                Content = new StringContent(query, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
