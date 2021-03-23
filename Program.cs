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

            var jsonHelper = new JsonHelper();
            var fileService = new FileManager(config);
            var webService = new ApiFetcher(config);
            //var webService = new MockApiFetcher();

            var remoteFetchTask = webService.FetchAsync();
            var localFetchTask = fileService.FetchAsync(InOutOptions.ForecastPath);
            var todaysFetchTask = fileService.FetchAsync(InOutOptions.TodaysPath);


            var parseRemoteDataTask = remoteFetchTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Remote fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing remote data...");
                    }
                    return jsonHelper.FromJson<Region[]>(task.Result);
                });
            var filterRemoteRegionsTask = parseRemoteDataTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Forecasts parsing task completed!");
                        logger.Log(LogLevel.Info, "Merging data...");
                    }

                    Region[] remoteRegions = parseRemoteDataTask.Result.Result;
                    return Task.Run(() => new DataFilter(config).FilterRegions(remoteRegions));
                });

            var parseLocalDataTask = localFetchTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Local fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing local data...");
                    }
                    return jsonHelper.FromJson<Region>(task.Result);
                });

            var parseTodaysDataTask = todaysFetchTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Today's fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing today's data...");
                    }
                    return jsonHelper.FromJson<Forecast[]>(task.Result);
                });


            var mergeTask =
                Task.WhenAll(filterRemoteRegionsTask, parseLocalDataTask)
                .ContinueWith(_ =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Forecasts parsing task completed!");
                        logger.Log(LogLevel.Info, "Merging data...");
                    }

                    Region localInstance = parseLocalDataTask.Result.Result;
                    Region remoteInstance = filterRemoteRegionsTask.Result.Result;

                    var dm = new DataMerger();

                    return Task.Run(() => dm.MergeRegions(localInstance, remoteInstance));
                });

            var forecastsSerializationTask = mergeTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "MergeTask completed!");
                        logger.Log(LogLevel.Info, "Serializing forecasts data...");
                    }

                    Region newLocalInstance = task.Result.Result;
                    return jsonHelper.ToJson(newLocalInstance);
                });

            var forecastsStoreTask = forecastsSerializationTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Forecasts serialization task completed!");
                        logger.Log(LogLevel.Info, "Storing forecasts data...");
                    }

                    var newLocalJson = task.Result.Result;

                    return fileService.PersistAsync(newLocalJson, InOutOptions.ForecastPath);
                });


            var todaysDataBuildTask =
                Task.WhenAll(parseTodaysDataTask, filterRemoteRegionsTask)
                .ContinueWith(_ =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Today's weather parsing task completed!");
                        logger.Log(LogLevel.Info, "Building today's data...");
                    }

                    Region remoteInstance = filterRemoteRegionsTask.Result.Result;
                    Forecast[] todaysForecasts = parseTodaysDataTask.Result.Result;

                    var tdb = new TodaysDataBuilder(remoteInstance, todaysForecasts);
                    return Task.Run(() => tdb.Build());
                });


            var uiSourceBuildTask = Task.WhenAll(mergeTask, todaysDataBuildTask)
                .ContinueWith(_ =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Antecedent tasks completed!");
                        logger.Log(LogLevel.Info, "Building UI data source...");
                    }

                    var newLocalInstance = mergeTask.Result.Result;
                    var newTodaysDatas = todaysDataBuildTask.Result.Result;

                    var usb = new UiSourceBuilder(newLocalInstance, newTodaysDatas);
                    return Task.Run(() => usb.Build());
                });


            var saveUiSourceTask = uiSourceBuildTask
                .ContinueWith(task =>
                {
                    lock (lockSource)
                    {
                        logger.Log(LogLevel.Success, "Building UI data completed!");
                        logger.Log(LogLevel.Info, "Saving UI data to source file...");
                    }

                    return fileService.PersistAsync(task.Result.Result, InOutOptions.ResultPath);
                });


            await Task.WhenAll(saveUiSourceTask, forecastsStoreTask);


            sw.Stop();
            var overall = sw.Elapsed.TotalSeconds;
            logger.Log(LogLevel.Success, "All task completed!");
            logger.Log(LogLevel.Info, $"Finished in {overall} second{(overall != 1 ? "s" : "")}");
            if (overall <= 1)
                logger.Log(LogLevel.Warn, "This application is too fast, in addition to being so good!");


            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            logger.Dispose(); //// remove??
        }
    }
}
