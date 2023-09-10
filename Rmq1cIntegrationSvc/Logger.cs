using System;
using System.IO;

namespace Rmq1cIntegrationSvc
{
    class Logger
    {
        object obj = new object();
        private string logPath;
        public Logger(string logPath = "")
        {
            if (String.IsNullOrEmpty(logPath))
            {
                this.logPath = AppDomain.CurrentDomain.BaseDirectory + "rmqlog.log";
            }
            else
            {
                this.logPath = logPath;
            }
        }
        public void WriteEntry(string message)
        {
            lock (obj)
            {
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    sw.WriteLine(String.Format("{0}: {1}", DateTime.Now.ToString("u"), message));
                    sw.Flush();
                }
            }
        }
    }
}