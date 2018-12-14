using Celsus.Client.Shared.Types;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Types
{
    public enum ClickOnceHelperStatusEnum : int
    {
        Idle,
        Working,
        VersionIsUpToDate,
        NewVersionGet,
        Error
    }
    public class ClickOnceHelper : BaseModel
    {
        private System.Deployment.Application.ApplicationDeployment _currentVersion = null;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string StatusString { get; set; }

        ClickOnceHelperStatusEnum status;
        public ClickOnceHelperStatusEnum Status
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

        public int ProgressPercentage { get; private set; }
        public long UpdateSizeBytes { get; private set; }

        private void SetStatus(ClickOnceHelperStatusEnum newStatus)
        {
            Status = newStatus;
        }
        public void CheckForUpdate()
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                SetStatus(ClickOnceHelperStatusEnum.Working);
                if (_currentVersion == null)
                {
                    try
                    {
                        _currentVersion = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                        _currentVersion.UpdateProgressChanged += CurrentVersion_UpdateProgressChanged;
                        _currentVersion.CheckForUpdateProgressChanged += CurrentVersion_CheckForUpdateProgressChanged;
                        _currentVersion.CheckForUpdateCompleted += CurrentVersion_CheckForUpdateCompleted;
                        _currentVersion.UpdateCompleted += CurrentVersion_UpdateCompleted;
                    }
                    catch (Exception ex)
                    {
                        SetStatus(ClickOnceHelperStatusEnum.Error);
                        Logger.Error(ex, "CheckForUpdate01");
                    }
                }
                if (_currentVersion != null)
                {
                    try
                    {
                        _currentVersion.CheckForUpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        SetStatus(ClickOnceHelperStatusEnum.Error);
                        Logger.Error(ex, "CheckForUpdate02");
                    }
                }
                else
                {
                    SetStatus(ClickOnceHelperStatusEnum.Error);
                    Logger.Error("CheckForUpdate03");
                }
            }
            else
            {
                SetStatus(ClickOnceHelperStatusEnum.Idle);
            }
        }

        void CurrentVersion_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                SetStatus(ClickOnceHelperStatusEnum.Error);
                Logger.Error(e.Error, "CurrentVersion_UpdateCompleted01");

                if (e.Error.InnerException != null)
                {
                    Logger.Error(e.Error.InnerException, "CurrentVersion_UpdateCompleted02");
                }
                try
                {
                    if (e.Error.Data != null)
                    {
                        foreach (var item in e.Error.Data)
                        {
                            var en = e.Error.Data.GetEnumerator();
                            while (en.MoveNext())
                            {
                                if (en.Current != null)
                                {
                                    Logger.Error(en.Current.ToString(), "CurrentVersion_UpdateCompleted03");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "CurrentVersion_UpdateComplete04");
                }
            }
            else
            {
                if (e.Cancelled == true)
                {
                    SetStatus(ClickOnceHelperStatusEnum.Error);
                    Logger.Error("CurrentVersion_UpdateCompleted05");
                }
                else
                {
                    SetStatus(ClickOnceHelperStatusEnum.NewVersionGet);
                }
            }
        }

        void CurrentVersion_UpdateProgressChanged(object sender, System.Deployment.Application.DeploymentProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
        }

        void CurrentVersion_CheckForUpdateProgressChanged(object sender, System.Deployment.Application.DeploymentProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
        }

        void CurrentVersion_CheckForUpdateCompleted(object sender, System.Deployment.Application.CheckForUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                SetStatus(ClickOnceHelperStatusEnum.Error);
                Logger.Error(e.Error, "CurrentVersion_CheckForUpdateCompleted01");
                if (e.Error.InnerException != null)
                {
                    Logger.Error(e.Error.InnerException, "CurrentVersion_CheckForUpdateCompleted02");
                }
                try
                {
                    if (e.Error.Data != null)
                    {
                        foreach (var item in e.Error.Data)
                        {
                            var en = e.Error.Data.GetEnumerator();
                            while (en.MoveNext())
                            {
                                if (en.Current != null)
                                {
                                    Logger.Error(en.Current.ToString(), "CurrentVersion_CheckForUpdateCompleted03");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "CurrentVersion_CheckForUpdateCompleted04");
                }

                //_currentVersion.UpdateAsync();

            }
            else
            {
                if (e.Cancelled == true)
                {
                    SetStatus(ClickOnceHelperStatusEnum.Error);
                    Logger.Error("CurrentVersion_CheckForUpdateCompleted05");
                }
                else
                {
                    if (e.UpdateAvailable)
                    {
                        UpdateSizeBytes = e.UpdateSizeBytes;
                        _currentVersion.UpdateAsync();
                    }
                    else
                    {
                        SetStatus(ClickOnceHelperStatusEnum.VersionIsUpToDate);
                    }
                }
            }
        }
    }
}
