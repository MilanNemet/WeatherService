using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace WeatherService.Control
{
    class JsonHelper
    {
        private readonly JsonSerializerOptions _options;

        public JsonHelper()
        {
            _options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.Strict,
                WriteIndented = true
            };
        }

        public async Task<T> FromJson<T>(string json)
        {
            var result = await JsonSerializer.DeserializeAsync<T>(new MemoryStream(Encoding.UTF8.GetBytes(json)), _options);

            return result;
        }

        public async Task<string> ToJson<T>(T value)
        {
            using var stream = new MemoryStream();
            Task result = JsonSerializer.SerializeAsync(stream, value, _options);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
