using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using WeatherService.Boundary;
using WeatherService.Control;
using WeatherService.Entity;
using WeatherService.Interface;

namespace WeatherService
{
    public class Program
    {
        static object lockSource = new object();
        public static async Task Main()
        {
            var sw = new Stopwatch();
            sw.Start();

            var logger = new LiteLogger(new MemoryStream());
            logger.Log(LogLevel.Info, "Building configuration...");


            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();


            logger.Log(LogLevel.Success, "Task completed!");
            logger.Log(LogLevel.Info, "Fetching data...");


            var fileService = new FileManager(config);
            var webService = new ApiFetcher(config);
            //var webService = new MockApiFetcher();

            var remoteFetchTask = webService.FetchAsync();
            var localFetchTask = fileService.FetchAsync(InOutOptions.ForecastPath);

            var jsonHelper = new JsonHelper();

            var parseRemoteDataTask = remoteFetchTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "RemoteFetchTask completed!");
                        logger.Log(LogLevel.Info, "Parsing remote data..."); 
                    }
                    return jsonHelper.FromJson<Region[]>(task.Result);
                });

            var parseLocalDataTask = localFetchTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "LocalFetchTask completed!");
                        logger.Log(LogLevel.Info, "Parsing local data..."); 
                    }
                    return jsonHelper.FromJson<Region>(task.Result);
                });

            await Task.WhenAll(parseRemoteDataTask, parseLocalDataTask);


            logger.Log(LogLevel.Success, "All parsing task completed!");
            logger.Log(LogLevel.Info, "Merging data...");


            Region localInstance = parseLocalDataTask.Result.Result;
            Region[] remoteRegions = parseRemoteDataTask.Result.Result;

            Region remoteInstance = new DataFilter(config).FilterRegions(remoteRegions);

            var dm = new DataMerger();
            Region newLocalInstance = dm.MergeRegions(localInstance, remoteInstance);


            logger.Log(LogLevel.Success, "Task completed!");
            logger.Log(LogLevel.Info, "Storing data...");


            var newLocalJson = await jsonHelper.ToJson(newLocalInstance);
            await fileService.PersistAsync(newLocalJson, InOutOptions.ForecastPath);


            logger.Log(LogLevel.Success, "Task completed!");
            logger.Log(LogLevel.Info, "Building UI data source...");


            var usb = new UiSourceBuilder(newLocalInstance, remoteInstance, fileService);
            usb.Build();


            logger.Log(LogLevel.Success, "Task completed!");


            sw.Stop();
            var overall = sw.Elapsed.TotalSeconds;
            logger.Log(LogLevel.Info, $"Finished in {overall} second{(overall != 1 ? "s" : "")}");
            if (overall <= 1)
                logger.Log(LogLevel.Warn, "This application is too fast, in addition to being so good!!!");


            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            logger.Dispose();
        }
    }
}
