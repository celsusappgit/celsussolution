using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Celsus.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(config =>
            {
                config.Service<BootStrap>(service =>
                {
                    service.ConstructUsing(s => new BootStrap());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                config.StartAutomatically();
                config.RunAsLocalSystem();
                config.SetServiceName("CelsusWorkerService");
                config.SetDescription("Celsus Worker Service");
                config.SetDisplayName("Celsus Worker Service");
                config.EnableServiceRecovery(x =>
                {
                    x.OnCrashOnly();
                    x.RestartService(0);
                });
            });
        }
    }
}
