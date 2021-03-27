using System;
using System.IO;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class MockAsyncIO : IAsyncIO
    {
        private ThrowOption ThrowBehavior { get; }
        public int Duration { get; }
        public MockAsyncIO()
        {
            var rand = new Random().Next(0, 3);
            if (rand == 0) ThrowBehavior = (ThrowOption)rand;
            if (rand % 2 == 1) ThrowBehavior = ThrowBehavior | ThrowOption.ThrowOnFetch;
            if(rand >= 2) ThrowBehavior = ThrowBehavior | ThrowOption.ThrowOnPersist;

            rand *= 1000;
            rand += new Random().Next(0, 999);
            Duration = rand;
        }
        public MockAsyncIO(int option) : this()
        {
            ThrowBehavior = (ThrowOption)option;
        }
        public async Task<string> FetchAsync(InOutOptions options = InOutOptions.None)
        {
            if (ThrowBehavior.HasFlag(ThrowOption.ThrowOnFetch))
            {
                throw new IOException($"Mock exception has been thrown at {this.GetType().Name}");
            }
            await Task.Delay(Duration);
            return await File.ReadAllTextAsync("../fetch.json");
        }

        public async Task PersistAsync(string data, InOutOptions options)
        {
            if (ThrowBehavior.HasFlag(ThrowOption.ThrowOnPersist))
            {
                throw new IOException($"Mock exception has been thrown at {this.GetType().Name}");
            }
            await Task.Delay(Duration);
            return;
        }

        [Flags]
        enum ThrowOption
        {
            None,
            ThrowOnFetch,
            ThrowOnPersist
        }
    }
}
