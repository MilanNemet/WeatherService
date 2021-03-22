using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WeatherService.Boundary;
using WeatherService.Entity;
using WeatherService.Interface;

namespace WeatherService.Control
{
    class TodaysDataBuilder
    {
        private readonly Region _remote;
        private readonly Forecast[] _todaysWeathers;

        public TodaysDataBuilder(Region remote, Forecast[] todaysWeathers)
        {
            _remote = remote;
            _todaysWeathers = todaysWeathers;
        }

        public Forecast[] Build()
        {
            var todaysWeather = _remote.forecasts.ToList().Find(f => f.date.Date == DateTime.Now.Date);
            var updated = _todaysWeathers.ToList();
            updated.Add(todaysWeather);

            return updated.ToArray();
        }
    }
}
