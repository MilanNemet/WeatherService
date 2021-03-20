using System;

namespace WeatherService.Entity
{
    public class Region
    {
        public string region { get; set; }
        public Forecast[] forecasts { get; set; }
        public bool IsNullOrEmpty
        {
            get
            {
                return (this == null) ||
                (string.IsNullOrEmpty(region) && (forecasts == null || forecasts.Length == 0));
            }
        }
    }
}