using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Console
{
    public class StreamLogger : ILogger, IDisposable
    {
        private Stream stream = null;

        private StreamWriter writer = null;

        public StreamLogger(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            writer = new StreamWriter(stream);
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Close();
            }
        }

        public override void LogError(string message)
        {
            writer.WriteLine(message);
        }

        public override void LogTrace(string message)
        {
            writer.WriteLine(message);
        }
    }
}
