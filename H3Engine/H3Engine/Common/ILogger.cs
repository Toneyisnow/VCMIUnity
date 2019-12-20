using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Common
{
    public class ILogger
    {
        public virtual void LogTrace(string message)
        {

        }

        public virtual void LogError(string message)
        {

        }


    }

    public class LoggerInstance
    {
        private static ILogger loggerImplement = new ILogger(); // Empty Logger for Default Value

        public static ILogger GetLogger()
        {
            return loggerImplement;
        }

        public static void SetConsoleLogger(ILogger logger)
        {
            loggerImplement = logger;
        }
    }
}
