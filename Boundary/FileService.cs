using Microsoft.Extensions.Configuration;

using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class FileService : IAsyncIO
    {
        private readonly string ForecastPath;
        private readonly string TodaysPath;
        private readonly string ResultPath;
        private readonly ILogger _logger;

        private static CancellationToken? s_token { get; set; } = null;
        private static CancellationToken Token => s_token ?? default;
        public FileService(IConfigurationRoot configuration, ILogger logger,CancellationToken token)
        {
            var section = configuration.GetSection(GetType().Name);
            ForecastPath = section.GetSection(nameof(ForecastPath)).Value;
            ResultPath = section.GetSection(nameof(ResultPath)).Value;
            TodaysPath = section.GetSection(nameof(TodaysPath)).Value;
            if (s_token == null) s_token = token;
            _logger = logger;
        }

        public async Task<string> FetchAsync(InOutOptions options)
        {
            string sourcePath =
                GetType()
                .GetField(options.ToString(), BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this) as string;
            string result;
            try
            {
                result = await File.ReadAllTextAsync(sourcePath, Token);
            }
            catch (System.Exception)
            {
                _logger.Log(LogLevel.Error, $"Failed to read from '{sourcePath}'! Check if the file exists, or if this process has the required IO persmissions!");
                throw;
            }

            return result;
        }

        public async Task PersistAsync(string data, InOutOptions options)
        {
            string sourcePath =
                GetType()
                .GetField(options.ToString(), BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this) as string;

            try
            {
                await File.WriteAllTextAsync(sourcePath, data, Token);
            }
            catch (System.Exception)
            {
                _logger.Log(LogLevel.Error, $"Failed to write to '{sourcePath}'! Check if this process has the required IO persmissions!");
                throw;
            }
        }
    }
}
