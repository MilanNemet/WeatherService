﻿using System;
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
        private const double yesterday = 1;
        private readonly Region _remote;
        private readonly Forecast[] _todaysWeathers;
        private List<Forecast> result { get; set; }

        public TodaysDataBuilder(Region remote, Forecast[] todaysWeathers)
        {
            _remote = remote;
            _todaysWeathers = todaysWeathers;
        }

        public Forecast[] Build()
        {
            result = _todaysWeathers.ToList();
            ReplaceMissingDaysByNull();
            AppendTodaysWeather();
            return result.ToArray();
        }
        private void ReplaceMissingDaysByNull()
        {
            if
            (
                result.Count > 0 &&
                result.Last().date.Date < DateTime.Now.Subtract(TimeSpan.FromDays(yesterday)).Date
            )
            {
                var diff = DateTime.Now.Date.Subtract(result.Last().date.Date);

                for (int i = 0; i < diff.Days - yesterday; i++)
                {
                    result.Add(null);
                }
            }
        }
        private void AppendTodaysWeather()
        {
            Forecast todaysWeather =
                _remote.forecasts.ToList().Find(f => f.date.Date == DateTime.Now.Date);

            if
            (
                result.Count <= 0 || 
                result.Last() == null || 
                result.Last().date.Date != DateTime.Now.Date
            )
            {
                result.Add(todaysWeather);
            }
        }
    }
}
