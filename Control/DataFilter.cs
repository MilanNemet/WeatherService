using System.Linq;

using Microsoft.Extensions.Configuration;

using WeatherService.Entity;

namespace WeatherService.Control
{
    class DataFilter
    {
        private readonly string TargetRegion;
        public DataFilter(IConfigurationRoot configuration)
        {
            var section = configuration.GetSection(GetType().Name);
            TargetRegion = section.GetSection(nameof(TargetRegion)).Value;
        }

        public Region FilterRegions(Region[] regions)
        {
            return regions.ToList().Find(t => string.Equals(t.region, TargetRegion));
        }
    }
}
