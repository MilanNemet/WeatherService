using Microsoft.Extensions.Configuration;

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
    class DataMerger
    {
        public DataMerger()
        {

        }

        public Region MergeRegions(Region local, Region remote)
        {
            if (local.IsNullOrEmpty) return remote;

            Region updated;
            Forecast[] newForecastArray;

            try
            {
                var localCollection = local.forecasts.ToList();
                var remoteCollection = remote.forecasts.ToList();
                newForecastArray = localCollection.Union(remoteCollection).ToArray();

                updated = new Region()
                {
                    region = remote.region,
                    forecasts = newForecastArray
                };
            }
            catch (Exception)
            {
                updated = new NullRegion(); /***  for now... ***/
            }

            return updated;
        }
    }
}
