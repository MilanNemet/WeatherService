using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class FileManager : IAsyncIO
    {
        private readonly string LocalPath;
        private readonly string OutputPath;
        public FileManager(IConfigurationRoot configuration)
        {
            var section = configuration.GetSection(GetType().Name);
            LocalPath = section.GetSection(nameof(LocalPath)).Value;
            OutputPath = section.GetSection(nameof(OutputPath)).Value;
        }

        public async Task<string> FetchAsync()
        {
            string result = await File.ReadAllTextAsync(LocalPath);
            return result;
        }

        public async Task PersistAsync(string data)
        {
            await File.WriteAllTextAsync(LocalPath, data);
        }

        public void PersistResult(string data)
        {
            File.WriteAllText(OutputPath, data);
        }
    }
}
