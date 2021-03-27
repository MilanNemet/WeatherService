using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using WeatherService.Boundary;
using WeatherService.Control;
using WeatherService.Entity;
using WeatherService.Interface;

namespace WeatherService
{
    public class Program
    {
        static readonly bool mock = false;
        static readonly object s_lockSource = new object();
        static readonly CancellationTokenSource s_tokenSource = new CancellationTokenSource();
        static readonly CancellationToken s_token = s_tokenSource.Token;
        public static void Main()
        {
            var sw = new Stopwatch();
            sw.Start();

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            var logger = new LiteLogger(new MemoryStream(), config);
            logger.Log(LogLevel.Debug, "Fetching data...");

            var jsonHelper = new JsonHelper();
            IAsyncIO fileService = mock ? new MockAsyncIO() : new FileService(config, s_token);
            IAsyncService webService = mock ? new MockAsyncIO() : new WebService(config, s_token);

            var remoteFetchTask = webService.FetchAsync(InOutOptions.None);
            var localForecastFetchTask = fileService.FetchAsync(InOutOptions.ForecastPath);
            var localTodaysFetchTask = fileService.FetchAsync(InOutOptions.TodaysPath);

            var parseRemoteDataTask = remoteFetchTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Remote fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing remote data...");
                    }
                    return jsonHelper.FromJsonAsync<Region[]>(task.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            var filterRemoteRegionsTask = parseRemoteDataTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Remote data parsing task completed!");
                        logger.Log(LogLevel.Info, "Filtering data...");
                    }

                    Region[] remoteRegions = parseRemoteDataTask.Result.Result;
                    return Task.Run(() => new DataFilter(config).FilterRegions(remoteRegions));
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var parseLocalForecastDataTask = localForecastFetchTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Local forecast fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing local forecast data...");
                    }
                    return jsonHelper.FromJsonAsync<Region>(task.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var parseLocalTodaysDataTask = localTodaysFetchTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Local today's fetch task completed!");
                        logger.Log(LogLevel.Info, "Parsing local today's data...");
                    }
                    return jsonHelper.FromJsonAsync<Forecast[]>(task.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);


            var mergeTask =
                Task.WhenAll(filterRemoteRegionsTask, parseLocalForecastDataTask)
                .ContinueWith(_ =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Forecasts parsing and filtering tasks completed!");
                        logger.Log(LogLevel.Info, "Merging data...");
                    }

                    Region localInstance = parseLocalForecastDataTask.Result.Result;
                    Region remoteInstance = filterRemoteRegionsTask.Result.Result;

                    var dm = new DataMerger();

                    return Task.Run(() => dm.MergeRegions(localInstance, remoteInstance));
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var forecastsSerializationTask = mergeTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "MergeTask completed!");
                        logger.Log(LogLevel.Info, "Serializing forecasts data...");
                    }

                    Region newLocalInstance = task.Result.Result;
                    return jsonHelper.ToJsonAsync(newLocalInstance);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var forecastsStoreTask = forecastsSerializationTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Forecasts serialization task completed!");
                        logger.Log(LogLevel.Info, "Storing forecasts data...");
                    }

                    var newLocalJson = task.Result.Result;

                    return fileService.PersistAsync(newLocalJson, InOutOptions.ForecastPath);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);


            var todaysDataBuildTask =
                Task.WhenAll(parseLocalTodaysDataTask, filterRemoteRegionsTask)
                .ContinueWith(_ =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Today's weather parsing and filtering tasks completed!");
                        logger.Log(LogLevel.Info, "Building today's data...");
                    }

                    Region remoteInstance = filterRemoteRegionsTask.Result.Result;
                    Forecast[] todaysWeathers = parseLocalTodaysDataTask.Result.Result;

                    var tdb = new TodaysDataBuilder(remoteInstance, todaysWeathers);
                    return Task.Run(() => tdb.Build());
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            var todaysDataSerializationTask = todaysDataBuildTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Today's weather data build task completed!");
                        logger.Log(LogLevel.Info, "Serializing today's data...");
                    }

                    return jsonHelper.ToJsonAsync(task.Result.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            var todaysDataStoreTask = todaysDataSerializationTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Today's weather data serialization task completed!");
                        logger.Log(LogLevel.Info, "Storing today's data...");
                    }

                    return fileService.PersistAsync(task.Result.Result, InOutOptions.TodaysPath);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var uiDataBuildTask = Task.WhenAll(mergeTask, todaysDataBuildTask)
                .ContinueWith(_ =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Antecedent tasks completed!");
                        logger.Log(LogLevel.Info, "Building UI data source...");
                    }

                    var newLocalInstance = mergeTask.Result.Result;
                    var newTodaysDatas = todaysDataBuildTask.Result.Result;

                    var usb = new UiSourceBuilder(newLocalInstance, newTodaysDatas);
                    return Task.Run(() => usb.Build());
                }, TaskContinuationOptions.OnlyOnRanToCompletion);


            var uiDataStoreTask = uiDataBuildTask
                .ContinueWith(task =>
                {
                    lock (s_lockSource)
                    {
                        logger.Log(LogLevel.Success, "Building UI data completed!");
                        logger.Log(LogLevel.Info, "Saving UI data to source file...");
                    }

                    return fileService.PersistAsync(task.Result.Result, InOutOptions.ResultPath);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);


            try
            {
                Task.WaitAll(new Task[]
                {
                    localForecastFetchTask,
                    remoteFetchTask,
                    localTodaysFetchTask,
                    parseLocalForecastDataTask,
                    parseRemoteDataTask,
                    parseLocalTodaysDataTask,
                    mergeTask,
                    forecastsStoreTask,
                    todaysDataStoreTask,
                    uiDataStoreTask
                });

                sw.Stop();
                var overall = sw.Elapsed.TotalSeconds;
                logger.Log(LogLevel.Success, "All task completed!");
                logger.Log(LogLevel.Info, $"Finished in {overall} second{(overall != 1 ? "s" : "")}");
                if (overall <= 1)
                    logger.Log(LogLevel.Warn, "This application is too fast :)");
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                    logger.Log(LogLevel.Error,
                        $"Exception has been thrown at:{e.Source}::{e.TargetSite}" +
                        $"{Environment.NewLine}\t\t{e.Message}" +
                        $"{Environment.NewLine}\t\tInnerException if any: {e.InnerException?.Message ?? "-"}" +
                        $"{Environment.NewLine}\t\t{e.StackTrace}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Fatal, ex.ToString());
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            logger.Dispose();
        }
    }
}
