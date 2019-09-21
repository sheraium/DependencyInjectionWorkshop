﻿namespace DependencyInjectionWorkshop.Adapter
{
    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}