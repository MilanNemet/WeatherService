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
        private readonly ILogger _logger;

        private static CancellationToken? s_token { get; set; }
        private static CancellationToken Token => s_token ?? default;
        public WebService(IConfigurationRoot configuration, ILogger logger, CancellationToken token)
        {
            var section = configuration.GetSection(GetType().Name);
            AuthHeaderKey = section.GetSection(nameof(AuthHeaderKey)).Value;
            AuthHeaderValue = section.GetSection(nameof(AuthHeaderValue)).Value;
            ApiUrl = section.GetSection(nameof(ApiUrl)).Value;
            _logger = logger;

            if (!int.TryParse(section.GetSection(nameof(Timeout)).Value, out Timeout))
            {
                Timeout = 60;
                _logger.Log(LogLevel.Warn, $"Couldn't parse the '{nameof(Timeout)}' value from configuration! Using the default value: {Timeout}");
            }
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
                try
                {
                    response = await client.GetStringAsync(ApiUrl, Token);
                }
                catch (HttpRequestException)
                {
                    _logger.Log(LogLevel.Error, $"Failed to complete a request to '{ApiUrl}'! Check your network connection or modify the URL in the 'appsettings.json' file.");
                    throw;
                }
            }
            return response;
        }
    }
}
