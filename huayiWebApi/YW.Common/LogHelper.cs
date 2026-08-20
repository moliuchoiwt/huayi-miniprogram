using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.IO;


namespace YW.Common
{

    public class LogHelper
    {
        private static ILoggerRepository repository { get; set; }
        private static ILog _log;
        private static ILog log
        {
            get
            {
                if (_log == null)
                {
                    Configure();
                }
                return _log;
            }
        }

        public static void Configure(string repositoryName = "NETCoreRepository", string configFile = "log4net.config")
        {
            repository = LogManager.CreateRepository(repositoryName);
            XmlConfigurator.Configure(repository, new FileInfo(configFile));
            _log = LogManager.GetLogger(repositoryName, "RollingLogFileAppender");
        }

        public static void Info(string msg)
        {
            log.Info(msg);
        }

        public static void Warn(string msg)
        {
            log.Warn(msg);
        }
        public static void Error(string msg, Exception exception = null)
        {
            log.Error(msg, exception);
        }

        public static void Debug(string msg)
        {
            log.Debug(msg);
        }

        public static void Fatal(string msg)
        {
            log.Fatal(msg);
        }
        
    }
}
