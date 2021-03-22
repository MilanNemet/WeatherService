using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Interface
{
    interface ILogger
    {
        public void Log(LogLevel level, params object[] obj);
    }
}
