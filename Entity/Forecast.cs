namespace WeatherService.Entity
{
    public class Forecast
    {
        public System.DateTime date { get; set; }
        public Sky_Icon sky_icon { get; set; }
        public int max_temp { get; set; }
        public int min_temp { get; set; }
        public int rain { get; set; }
        public int rain_percent { get; set; }
        public int wind { get; set; }
        public int water_temperature { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
    } 
}