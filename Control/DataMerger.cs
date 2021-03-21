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

            Region updated = new NullRegion();

            return updated;
        }
    }
}
