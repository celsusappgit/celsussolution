using Celsus.DataLayer;
using Celsus.Types;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celsus.Worker
{
    /// <summary>
    /// HangFireService - We will add all the jobs to job manager,
    /// and we will use CRON expressions to schedule each job
    /// </summary>
    public class HangFireService
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Schedule the jobs
        /// </summary>
        public void ScheduleJobs()
        {
            logger.Info($"ScheduleJobs");
            StopJobs();
            var jobs = AddJobs();
            foreach (var job in jobs)
            {
                ScheduleJob(job);
            }
        }

        /// <summary>
        /// Schedule each job based on the job configuration details
        /// </summary>
        /// <param name="job"></param>
        private void ScheduleJob(JobConfigDetails job)
        {
            logger.Info($"Starting job: {job.Id}");
            if (string.IsNullOrEmpty(job.Cron) || string.IsNullOrEmpty(job.TypeName))
            {
                return;
            }

            try
            {
                BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));

                var jobManager = new RecurringJobManager();
                jobManager.RemoveIfExists(job.Id);
                var type = Type.GetType(job.TypeName);
                if (type != null && job.Enabled)
                {
                    var jobSchedule = new Job(type, type.GetMethod("Start"), job.Source.Id);
                    jobManager.AddOrUpdate(job.Id, jobSchedule, job.Cron, TimeZoneInfo.Local);
                    logger.Info($"Job {job.Id} has started");
                }
                else
                {
                    logger.Warn($"Job {job.Id} of type {type} is not found or job is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when starting the job.");
            }
        }

        /// <summary>
        /// Stop and remove jobs from job manager
        /// </summary>
        public void StopJobs()
        {
            using (var conn = JobStorage.Current.GetConnection())
            {
                var manager = new RecurringJobManager();
                foreach (var job in conn.GetRecurringJobs())
                {
                    manager.RemoveIfExists(job.Id);
                    logger.Info($"Job has been stopped: {job.Id}");
                }
            }
        }

        /// <summary>
        /// Add all jobs with in a specific type name to the list
        /// Configure the job id, CRON expression for each job
        /// </summary>
        /// <returns></returns>
        public List<JobConfigDetails> AddJobs()
        {

            List<SourceDto> sources = null;
            List<FileTypeDto> fileTypes = null;
            using (var context = new SqlDbContext())
            {
                sources = context.Sources.Where(x => x.IsActive == true).ToList();
                fileTypes = context.FileTypes.Where(x => x.IsActive == true).ToList();
            }
            var configDetails = new List<JobConfigDetails>();

            foreach (var source in sources)
            {
                var jobConfigDetails = new JobConfigDetails
                {
                    Source = source,
                    Id = source.Id + " " + source.Name,
                    FileTypes = fileTypes.Where(x => x.SourceId == source.Id).ToList(),
                    Enabled = true,
                    //Cron = source.Cron,
                    TypeName = "Celsus.Worker.Service, Celsus.Worker"
                };
                bool isDebug = false;
#if DEBUG
                isDebug = true;
#endif
                if (isDebug)
                {
                    jobConfigDetails.Cron = "* * * * *";
                }
                configDetails.Add(jobConfigDetails);
            }


            return configDetails;
        }
    }

}
