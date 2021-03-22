using Microsoft.Extensions.Configuration;

using System.IO;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class FileManager : IAsyncIO
    {
        private readonly string ForecastPath;
        private readonly string TodaysPath;
        private readonly string ResultPath;
        public FileManager(IConfigurationRoot configuration)
        {
            var section = configuration.GetSection(GetType().Name);
            ForecastPath = section.GetSection(nameof(ForecastPath)).Value;
            ResultPath = section.GetSection(nameof(ResultPath)).Value;
            TodaysPath = section.GetSection(nameof(TodaysPath)).Value;
        }

        public async Task<string> FetchAsync(InOutOptions options)
        {
            string sourcePath =
                GetType().GetProperty(options.ToString()).GetValue(this, null) as string;

            string result = await File.ReadAllTextAsync(sourcePath);

            //string result = await File.ReadAllTextAsync(ForecastPath);
            return result;
        }

        public async Task PersistAsync(string data, InOutOptions options)
        {
            string sourcePath =
                GetType().GetProperty(options.ToString()).GetValue(this, null) as string;

            await File.WriteAllTextAsync(sourcePath, data);
            //await File.WriteAllTextAsync(ForecastPath, data);
        }

        //public void PersistResult(string data)
        //{
        //    File.WriteAllText(ResultPath, data);
        //}
    }
}
