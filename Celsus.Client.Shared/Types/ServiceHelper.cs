using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum ServiceHelperStatusEnum
    {
        GotError = 1,
        Ok = 2,
        NotInstalled = 3,
        Installed = 4,
        InstalledButOld = 5
    }
    public class ServiceHelper : BaseModel<ServiceHelper>, MustInit
    {

        private readonly object balanceLock = new object();

        private bool isInitted = false;


        ServiceControllerStatus? serviceControllerStatus;
        public ServiceControllerStatus? ServiceControllerStatus
        {
            get
            {
                return serviceControllerStatus;
            }
            set
            {
                if (Equals(value, serviceControllerStatus)) return;
                serviceControllerStatus = value;
                NotifyPropertyChanged(() => ServiceControllerStatus);

            }
        }

        ServiceHelperStatusEnum status;
        public ServiceHelperStatusEnum Status
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
                IndexerHelper.Instance.UpdateStatus();
            }
        }

        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }
            }
            CheckAll();
            lock (balanceLock)
            {
                isInitted = true;
            }
        }

        private void CheckAll()
        {
            CheckService();
            if (Status == ServiceHelperStatusEnum.Installed)
            {
                if (CheckVersion())
                {
                    Status = ServiceHelperStatusEnum.Ok;
                }
                else
                {
                    Status = ServiceHelperStatusEnum.InstalledButOld;
                }
            }
        }

        private void CheckService()
        {
            ServiceController[] services = null;
            try
            {
                services = ServiceController.GetServices();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"CheckService");
                Status = ServiceHelperStatusEnum.GotError;
            }
            var service = services.FirstOrDefault(s => s.DisplayName == "Celsus Worker Service");
            if (service != null)
            {
                ServiceControllerStatus = service.Status;
                Status = ServiceHelperStatusEnum.Installed;
            }
            else
            {
                ServiceControllerStatus = null;
                Status = ServiceHelperStatusEnum.NotInstalled;
            }
        }

        private bool CheckVersion()
        {
            var zipFolder = FileHelper.GetUnusedFolderName(Path.GetTempPath(), $"WorkerUnzipped");
            var result = UnzipWorkerZip(zipFolder);
            if (result == false)
            {
                return result;
            }
            var workerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            var compareResult = FileHelper.CompareFolders(zipFolder, workerPath);
            return compareResult;
        }

        private bool UnzipWorkerZip(string targetDir)
        {

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = null;
            try
            {
                resourceStream = assembly.GetManifestResourceStream("Celsus.Client.Shared.Worker.worker.zip");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetManifestResourceStream error.");
                return false;
            }

            byte[] zipData = null;
            try
            {
                using (MemoryStream _mem = new MemoryStream())
                {
                    resourceStream.CopyTo(_mem);
                    zipData = _mem.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"MemoryStream error.");
                return false;
            }

            var zipFile = FileHelper.GetUnusedFileName(Path.GetTempPath(), $"worker.zip");

            try
            {
                File.WriteAllBytes(zipFile, zipData);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"WriteAllBytes error.");
                return false;
            }

            try
            {
                ZipFile.ExtractToDirectory(zipFile, targetDir);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ExtractToDirectory error.");
                return false;
            }

            return true;
        }

        private bool Upgrade()
        {
            if (ServiceInstallerHelper.Instance.IsAdmin == false)
            {
                return false;
            }
            var result = ServiceInstallerHelper.Instance.UnInstallService();
            if (result == false)
            {
                return result;
            }
            result = DeleteFolder();
            if (result == false)
            {
                return result;
            }
            result = UnzipWorkerZip(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker"));
            if (result == false)
            {
                return result;
            }
            result = ServiceInstallerHelper.Instance.InstallService();

            return result;
        }

        private bool DeleteFolder()
        {
            var workerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            try
            {
                if (Directory.Exists(workerPath))
                {
                    Directory.Delete(workerPath, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Delete folder error.");
                return false;
            }
        }


        public bool InstallOrUpgrade()
        {
            if (Status == ServiceHelperStatusEnum.NotInstalled)
            {
                if (ServiceInstallerHelper.Instance.IsAdmin == false)
                {
                    return false;
                }
                var result = DeleteFolder();
                result = UnzipWorkerZip(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker"));
                result = CheckVersion();
                if (result == false)
                {
                    return false;
                }
                result = ServiceInstallerHelper.Instance.InstallService();
                if (result == false)
                {
                    return false;
                }
            }
            else if (Status == ServiceHelperStatusEnum.InstalledButOld)
            {
                var result = Upgrade();
                if (result == false)
                {
                    return false;
                }
            }
            CheckAll();
            return true;
        }
    }
}
