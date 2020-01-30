using System;
using NLog;

namespace StudentsFileSharingApp.Utility
{
    public static class Logger
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public static void Log(string target, string message, LogLevel logLevel, Exception exception)
            => logger.Log(logLevel, exception, $"{target} => {message}");
    }
}