using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Interface
{
    public enum LogLevel
    {
        Debug,
        Info,
        Success,
        Warn,
        Error,
        Fatal
    }
    public enum InOutOptions
    {
        None,
        ForecastPath,
        TodaysPath,
        ResultPath
    }
}
