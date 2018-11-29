using System;
using Topshelf;

namespace Celsus.Worker
{
    public class Program
    {
        /// <summary>
        /// Configure TopShelf to construct service using HangFire
        /// Bootstrap class - has Hangfire configuration
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            var rc = HostFactory.Run(config =>
                          {
                              config.Service<BootStrap>(service =>
                              {
                                  service.ConstructUsing(s => new BootStrap());
                                  service.WhenStarted(s => s.Start());
                                  service.WhenStopped(s => s.Stop());
                              });
                              config.UseNLog();
                              config.StartAutomatically();
                              config.RunAsLocalSystem();
                              config.SetServiceName("CelsusWorkerService");
                              config.SetDescription("Celsus Worker Service");
                              config.SetDisplayName("Celsus Worker Service");
                              config.EnableServiceRecovery(x =>
                              {
                                  x.OnCrashOnly();
                                  x.RestartService(1);
                              });
                          });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  
            Environment.ExitCode = exitCode;

        }
    }

}
