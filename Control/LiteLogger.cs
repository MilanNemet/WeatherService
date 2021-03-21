using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WeatherService.Interface;

namespace WeatherService.Control
{
    class LiteLogger : ILogger
    {
        public LiteLogger()
        {

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
                case LogLevel.Warning:
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
            Console.Write(DateTime.UtcNow);
            Console.Write("\t");
            Console.Write(level);
            foreach (var item in items) Console.Write($"\t{item}");
            Console.WriteLine();
        }
    }
}
