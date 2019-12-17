using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Console
{
    public class ConsoleLogger : ILogger
    {
        public override void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public override void LogTrace(string message)
        {
            Console.WriteLine(message);
        }
    }
}
