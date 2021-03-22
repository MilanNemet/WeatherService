using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WeatherService.Interface
{
    interface IAsyncPersister
    {
        Task PersistAsync(string data, InOutOptions options);
    }
}