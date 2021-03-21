using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WeatherService.Entity;

namespace WeatherService.Control
{
    class UiSourceBuilder
    {
        private readonly Region _local;
        private readonly Region _remote;
        public UiSourceBuilder(Region local, Region remote)
        {
            _local = local;
            _remote = remote;
        }

        public void Build()
        {
            var sb = new StringBuilder();
        }
    }
}
