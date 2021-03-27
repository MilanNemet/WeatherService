using Microsoft.Extensions.Configuration;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class WebService : IAsyncService
    {
        private readonly string AuthHeaderKey;
        private readonly string AuthHeaderValue;
        private readonly string ApiUrl;
        private readonly int Timeout;
        private static CancellationToken? s_token { get; set; } = null;
        private static CancellationToken Token => s_token ?? default;
        public WebService(IConfigurationRoot configuration, CancellationToken token)
        {
            var section = configuration.GetSection(GetType().Name);
            AuthHeaderKey = section.GetSection(nameof(AuthHeaderKey)).Value;
            AuthHeaderValue = section.GetSection(nameof(AuthHeaderValue)).Value;
            ApiUrl = section.GetSection(nameof(ApiUrl)).Value;
            if (!int.TryParse(section.GetSection(nameof(Timeout)).Value, out Timeout)) Timeout = 60;
            if (s_token == null) s_token = token;
        }

        public async Task<string> FetchAsync(InOutOptions options = InOutOptions.None)
        {
            string response;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(AuthHeaderKey, AuthHeaderValue);
                client.Timeout = TimeSpan.FromSeconds(Timeout);
                response = await client.GetStringAsync(ApiUrl, Token);
            }
            return response;
        }
    }
}
