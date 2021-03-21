using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class MockApiFetcher : IAsyncService
    {
        public async Task<string> FetchAsync()
        {
            return await System.IO.File.ReadAllTextAsync("../fetch.json");
        }
    }
}
