using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Interface
{
    public enum LogLevel
    {
        Info,
        Success,
        Warn,
        Error
    }
    public enum InOutOptions
    {
        None,
        ForecastPath,
        TodaysPath,
        ResultPath
    }
}
