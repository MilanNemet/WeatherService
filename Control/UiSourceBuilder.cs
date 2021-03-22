using System;
using System.Text;

using WeatherService.Boundary;
using WeatherService.Entity;
using WeatherService.Interface;

namespace WeatherService.Control
{
    class UiSourceBuilder
    {
        private readonly Region _local;
        private readonly Forecast[] _todaysDatas;
        public UiSourceBuilder(Region newLocal, Forecast[] todaysDatas)
        {
            _local = newLocal;
            _todaysDatas = todaysDatas;
        }

        public string Build()
        {
            var sb = new StringBuilder();

            var dates = BuildDates();
            var maxValues = BuildMaxValues();
            var progMaxValues = BuildProgMaxValues();
            var minValues = BuildMinValues();
            var progMinValues = BuildProgMinValues();

            sb.Append(dates);
            sb.Append(maxValues);
            sb.Append(progMaxValues);
            sb.Append(minValues);
            sb.Append(progMinValues);

            return sb.ToString();
        }

        private string BuildDates()
        {
            var sb = new StringBuilder();
            
            sb.Append("let dates = [ ");
            foreach (var fc in _local.forecasts)
            {
                sb.Append($"\"{fc.date:yyyy.MM.dd}\"");
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(" ];");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        private string BuildMaxValues()
        {
            var sb = new StringBuilder();

            sb.Append("let maxValues = [");


            return sb.ToString();
        }
        private string BuildProgMaxValues()
        {
            var sb = new StringBuilder();

            sb.Append("let progMaxValues = [");
            foreach (var fc in _local.forecasts)
            {
                sb.Append(fc.max_temp);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(" ];");

            return sb.ToString();
        }
        private string BuildMinValues()
        {
            var sb = new StringBuilder();

            sb.Append("let minValues = [");


            return sb.ToString();
        }
        private string BuildProgMinValues()
        {
            var sb = new StringBuilder();
            
            sb.Append("let progMinValues = [");


            return sb.ToString();
        }
    }
}
