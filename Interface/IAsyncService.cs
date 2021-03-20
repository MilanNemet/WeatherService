using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Interface
{
    interface IAsyncService
    {
        Task<string> FetchAsync();
    }
}
