using System;
using System.Text;

using WeatherService.Boundary;
using WeatherService.Entity;

namespace WeatherService.Control
{
    class UiSourceBuilder
    {
        private readonly Region _local;
        private readonly Region _remote;
        private readonly FileManager _fm;
        public UiSourceBuilder(Region local, Region remote, FileManager fm)
        {
            _local = local;
            _remote = remote;
            _fm = fm;
        }

        public void Build()
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

            _fm.PersistResult(sb.ToString());
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
