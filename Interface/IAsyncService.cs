using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using WeatherService.Boundary;

namespace WeatherService.Interface
{
    interface IAsyncService
    {
        Task<string> FetchAsync(InOutOptions options);
    }
}
