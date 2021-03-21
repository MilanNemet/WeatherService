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

        public override bool Equals(object obj)
        {
            var forecast = obj as Forecast;
            return Equals(forecast);
        }
        public bool Equals(Forecast fc)
        {
            return date.Date == fc.date.Date;
        } 
        public override int GetHashCode()
        {
            return date.GetHashCode();
        }
    }
}