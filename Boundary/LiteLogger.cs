using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Boundary
{
    class LiteLogger : ILogger, IDisposable
    {
        private Stream outStream;
        public LiteLogger(Stream stream)
        {
            outStream = stream;
        }

        public void Dispose()
        {
            var now = DateTime.UtcNow;
            var fm = FileMode.Create;
            //var path = $"./Logs/{now.Year}-{now.Month}-{now.Day}.log";
            var path = $"./Logs/{now:yyyy-MM-dd}.log";
            if (File.Exists(path)) fm = FileMode.Append;
            var fs = new FileStream(path, fm);
            outStream.Position = 0;
            outStream.CopyTo(fs);
            fs.Flush(true);
        }

        public void Log(LogLevel level, params object[] objects)
        {
            switch (level)
            {
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
                default:
                    PrintEach(level, objects);
                    break;
            }
        }

        private void PrintEach(LogLevel level, object[] items)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.UtcNow);
            sb.Append("\t");
            sb.Append(level);
            foreach (var item in items) sb.Append($"\t{item}");
            sb.Append(Environment.NewLine);

            Console.Write(sb);

            var buffer = Encoding.Unicode.GetBytes(sb.ToString());
            outStream.Write(buffer, 0, buffer.Length);
        }
    }
}
