using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum ServiceInstallerHelperStatusEnum
    {
        NotIsAdmin = 1,
        IsAdmin = 2,
    }
    public class ServiceInstallerHelper : BaseModel<ServiceInstallerHelper>, MustInit
    {
        ServiceInstallerHelperStatusEnum status;
        public ServiceInstallerHelperStatusEnum Status
        {
            get
            {
                return status;
            }
            set
            {
                if (Equals(value, status)) return;
                status = value;
                NotifyPropertyChanged(() => Status);
            }
        }
        public bool IsAdmin
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return isAdmin;
            }
        }

        public void Init()
        {
            if (IsAdmin)
            {
                Status = ServiceInstallerHelperStatusEnum.IsAdmin;
            }
            else
            {
                Status = ServiceInstallerHelperStatusEnum.NotIsAdmin;
            }
        }

        public bool InstallService()
        {

            var workerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");

            var processHelper = new ProcessHelper
            {
                FileName = $"{Path.Combine(workerPath, "Celsus.Worker.exe")}",
                Arguments = $"install --autostart"
            };

            logger.Trace($"Installing Service with command line. Details: {processHelper.FileName + " " + processHelper.Arguments}");

            processHelper.RunProcess(true);

            if (processHelper.Exception != null)
            {
                logger.Error(processHelper.Exception, $"Process error while installing service.");
                return false;
            }

            if (processHelper.ErrorDatas.Count(x => x.IndexOf("[Failure]") >= 0) > 0)
            {
                var cmdOut = string.Join(Environment.NewLine, processHelper.ErrorDatas.ToArray());
                logger.Error($"Error installing Service. Details: {cmdOut}");
                return false;
            }
            if (processHelper.OutputDatas.Count(x => x.IndexOf("[Failure]") >= 0) > 0)
            {
                var cmdOut = string.Join(Environment.NewLine, processHelper.OutputDatas.ToArray());
                logger.Error($"Error installing Service. Details: {cmdOut}");
                return false;
            }
            if (processHelper.OutputDatas.Count(x => x.IndexOf("service can only be installed as an administrator") >= 0) > 0)
            {
                var cmdOut = string.Join(Environment.NewLine, processHelper.OutputDatas.ToArray());
                logger.Error($"Error installing Service. Details: {cmdOut}");
                return false;
            }

            return true;
        }

        public bool UnInstallService()
        {
            var workerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            var exePath = Path.Combine(workerPath, "Celsus.Worker.exe");
            if (Directory.Exists(workerPath) && File.Exists(exePath))
            {
                var processHelper = new ProcessHelper
                {
                    FileName = exePath,
                    Arguments = $"uninstall"
                };

                logger.Trace($"Uninstalling Service with command line. Details: {processHelper.FileName + " " + processHelper.Arguments}");

                processHelper.RunProcess();

                if (processHelper.Exception != null)
                {
                    logger.Error(processHelper.Exception, $"Process error while uninstalling service.");
                    return false;
                }

                // TODO success ve failure da ne veriyor bakmak lazım
                if (processHelper.ErrorDatas.Count(x => x.IndexOf("[Failure]") >= 0) > 0)
                {
                    var cmdOut = string.Join(Environment.NewLine, processHelper.ErrorDatas.ToArray());
                    logger.Error($"Error installing Service. Details: {cmdOut}");
                    return false;
                }
                if (processHelper.OutputDatas.Count(x => x.IndexOf("[Failure]") >= 0) > 0)
                {
                    var cmdOut = string.Join(Environment.NewLine, processHelper.OutputDatas.ToArray());
                    logger.Error($"Error installing Service. Details: {cmdOut}");
                    return false;
                }
                if (processHelper.OutputDatas.Count(x => x.IndexOf("[The uninstall has completed.]") >= 0) > 0)
                {
                    return true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
