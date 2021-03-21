using Microsoft.Extensions.Configuration;

using System;
using System.Threading.Tasks;

using WeatherService.Boundary;
using WeatherService.Control;
using WeatherService.Entity;
using WeatherService.Interface;

namespace WeatherService
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Building configuration...");


            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();


            Console.WriteLine("Fetching data...");


            IAsyncIO fileService = new FileManager(config);
            //IAsyncService webService = new ApiFetcher(config);
            IAsyncService webService = new MockApiFetcher();


            var remoteFetchTask = webService.FetchAsync();
            var localFetchTask = fileService.FetchAsync();

            var firstCompletedFetchTask = await Task.WhenAny(remoteFetchTask, localFetchTask);


            if (firstCompletedFetchTask.IsCompleted) Console.WriteLine("Parsing JSON...");


            var jsonHelper = new JsonHelper();

            var parseRemoteDataTask = remoteFetchTask.ContinueWith(
                task => jsonHelper.FromJson<Region[]>(task.Result));

            var parseLocalDataTask = localFetchTask.ContinueWith(
                task => jsonHelper.FromJson<Region>(task.Result));

            await Task.WhenAll(parseRemoteDataTask, parseLocalDataTask);


            Console.WriteLine("Merging data...");

            Region localInstance = parseLocalDataTask.Result.Result;
            Region[] remoteRegions = parseRemoteDataTask.Result.Result;

            Region remoteInstance = new DataFilter(config).FilterRegions(remoteRegions);

            var dm = new DataMerger();
            Region newLocalInstance = dm.MergeRegions(localInstance, remoteInstance);

            var newLocalJson = await jsonHelper.ToJson(newLocalInstance);
            await fileService.PersistAsync(newLocalJson);
        }
    }
}
