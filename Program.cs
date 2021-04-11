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
        static readonly object s_lockSource = new();
        static readonly CancellationTokenSource s_tokenSource = new();
        static readonly CancellationToken s_token = s_tokenSource.Token;
        public static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            using var logger = new LiteLogger(new MemoryStream(), config);

            logger.Log(LogLevel.Debug, "Entering main loop...");
            await MainLoop(logger, config);

            sw.Stop();
            var overall = sw.Elapsed.TotalSeconds;
            logger.Log(LogLevel.Info, $"Finished in {overall} second{(overall != 1 ? "s" : "")}");
            if (overall <= 1)
                logger.Log(LogLevel.Warn, "This application is too fast :)");

            if (args.Length > 0 && args[0] == "-u")
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
        private static async Task MainLoop(ILogger logger, IConfigurationRoot config)
        {
            var section = config
                .GetSection(new StackTrace()
                .GetFrame(3)
                .GetMethod()
                .Name);

            double Delay;
            if (!double.TryParse(section.GetSection(nameof(Delay)).Value, out Delay))
            {
                Delay = 5;
                logger.Log(LogLevel.Warn, $"Couldn't parse the '{nameof(Delay)}' value from configuration! Using the default value: {Delay}");
            }

            uint MaxTries;
            if (!uint.TryParse(section.GetSection(nameof(MaxTries)).Value, out MaxTries))
            {
                MaxTries = 3;
                logger.Log(LogLevel.Warn, $"Couldn't parse the '{nameof(MaxTries)}' value from configuration! Using the default value: {MaxTries}");
            }

            uint counter = 0;
            bool success;
            do
            {
                ++counter;
                logger.Log(LogLevel.Debug, "Starting main task...");
                success = MainTask(logger, config);
                if (!success)
                {
                    Console.WriteLine();
                    var delayTask = Task.Delay(TimeSpan.FromSeconds(Delay));
                    var ww = Console.WindowWidth;
                    while (!delayTask.IsCompleted)
                    {
                        for (int i = 0; i < ww; i++)
                        {
                            ww = Console.WindowWidth;
                            Console.Write('█');
                            await Task.Delay((int)Math.Round(Delay * 1000 / ww));
                        }
                    }
                    Console.WriteLine();
                }
            }
            while (!success && counter < MaxTries);
        }
        private static bool MainTask(ILogger logger, IConfigurationRoot config)
        {
            var result = false;

            var jsonHelper = new JsonHelper();
            IAsyncIO fileService = mock ? new MockAsyncIO() : new FileService(config, logger, s_token);
            IAsyncService webService = mock ? new MockAsyncIO() : new WebService(config, logger, s_token);

            logger.Log(LogLevel.Debug, "Initializing subtasks...");
            logger.Log(LogLevel.Debug, "Fetching data...");

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

                    return Task.Run(() => DataMerger.MergeRegions(localInstance, remoteInstance));
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
                    if (newLocalInstance is NullRegion)
                        throw new OperationCanceledException("Merge task returned a NullRegion instance.", s_token)
                        {
                            Source = $"{newLocalInstance}::{nameof(newLocalInstance)}",
                        };
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

                result = true;
                Environment.SetEnvironmentVariable("WSRUN", "true");
                logger.Log(LogLevel.Success, "All task completed!");
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
                Environment.SetEnvironmentVariable("WSRUN", "false");
                logger.Log(LogLevel.Fatal, ex.ToString());
            }
            return result;
        }
    }
}
