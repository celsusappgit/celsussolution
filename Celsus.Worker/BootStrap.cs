using Hangfire;
using System;

namespace Celsus.Worker
{
    public class BootStrap
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private BackgroundJobServer _server;

        public BootStrap()
        {
            try
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage("SqlDbContextConnection");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when starting the job UseSqlServerStorage.");
            }

        }

        public void Start()
        {
            _server = new BackgroundJobServer();
            var jobSvc = new HangFireService();
            jobSvc.ScheduleJobs();
        }
        public void Stop()
        {
            _server.Dispose();
        }
    }

}
