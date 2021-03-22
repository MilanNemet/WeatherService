using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class MockApiFetcher : IAsyncService
    {
        public async Task<string> FetchAsync(InOutOptions options = InOutOptions.None)
        {
            return await System.IO.File.ReadAllTextAsync("../fetch.json");
        }
    }
}
