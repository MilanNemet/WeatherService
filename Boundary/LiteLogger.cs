using System;
using System.IO;
using System.Text;

using Microsoft.Extensions.Configuration;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class LiteLogger : ILogger, IDisposable
    {
        private readonly Stream _outStream;
        private readonly IConfiguration _configuration;
        private readonly LogLevel _lowestLoglevel;
        public LiteLogger(Stream stream, IConfiguration configuration)
        {
            _outStream = stream;
            _configuration = configuration;
            var section = configuration.GetSection(nameof(ILogger));
            _lowestLoglevel = 
                (LogLevel)Enum.Parse(typeof(LogLevel), section.GetSection(nameof(LogLevel)).Value);
        }

        public void Log(LogLevel level, params object[] objects)
        {
            if (level < _lowestLoglevel) return;
            switch (level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    PrintEach(level, objects);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    PrintEach(level, objects);
                    break;
                case LogLevel.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    PrintEach(level, objects);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    PrintEach(level, objects);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    PrintEach(level, objects);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    PrintEach(level, objects);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    PrintEach(level, objects);
                    break;
            }
        }

        private void PrintEach(LogLevel level, object[] items)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append("\t");
            sb.Append(level);
            foreach (var item in items) sb.Append($"\t{item}");
            sb.Append(Environment.NewLine);

            Console.Write(sb);

            var buffer = Encoding.Unicode.GetBytes(sb.ToString());
            _outStream.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            var now = DateTime.Now;
            var fm = FileMode.Create;
            var logsBasePath = $"{AppDomain.CurrentDomain.BaseDirectory}Logs";
            Directory.CreateDirectory(logsBasePath);
            var logFilePath = $"{logsBasePath}/{now:yyyy-MM-dd}.log";
            if (File.Exists(logFilePath)) fm = FileMode.Append;
            var fs = new FileStream(logFilePath, fm);
            _outStream.Position = 0;
            _outStream.CopyTo(fs);
            fs.Flush(true);
        }
    }
}
