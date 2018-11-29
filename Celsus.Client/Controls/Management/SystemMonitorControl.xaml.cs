using Celsus.Client.Shared.Types;
using Celsus.Client.Types.Models;
using Celsus.DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Celsus.Client.Controls.Management
{
    public partial class SystemMonitorControlModel : BaseModel<SystemMonitorControlModel>, MustInit
    {
        #region Members

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private bool isInitted = false;

        #endregion

        public string ServiceStatus
        {
            get
            {
                if (ServiceHelper.Instance.Status == ServiceHelperStatusEnum.Ok)
                {
                    if (RolesHelper.Instance.IndexerRoleComputerName == Environment.MachineName)
                    {
                        return ServiceHelper.Instance.ServiceControllerStatus.Value.ToString();
                    }
                    else
                    {
                        return ServiceHelper.Instance.Status.ToString();
                    }
                }
                else
                {
                    return ServiceHelper.Instance.Status.ToString();
                }
            }
        }

        public int ActiveSources
        {
            get
            {
                return Repo.Instance.Sources.Count(x => x.SourceDto.IsActive == true);
            }
        }

        int directoryCount;
        public int DirectoryCount
        {
            get
            {
                return directoryCount;
            }
            set
            {
                if (Equals(value, directoryCount)) return;
                directoryCount = value;
                NotifyPropertyChanged(() => DirectoryCount);
            }
        }

        int fileCount;
        public int FileCount
        {
            get
            {
                return fileCount;
            }
            set
            {
                if (Equals(value, fileCount)) return;
                fileCount = value;
                NotifyPropertyChanged(() => FileCount);
            }
        }

        int fileSuccessedCount;
        public int FileSuccessedCount
        {
            get
            {
                return fileSuccessedCount;
            }
            set
            {
                if (Equals(value, fileSuccessedCount)) return;
                fileSuccessedCount = value;
                NotifyPropertyChanged(() => FileSuccessedCount);
            }
        }
        int fileOmittedCount;
        public int FileOmittedCount
        {
            get
            {
                return fileOmittedCount;
            }
            set
            {
                if (Equals(value, fileOmittedCount)) return;
                fileOmittedCount = value;
                NotifyPropertyChanged(() => FileOmittedCount);
            }
        }
        int fileErrorCount;
        public int FileErrorCount
        {
            get
            {
                return fileErrorCount;
            }
            set
            {
                if (Equals(value, fileErrorCount)) return;
                fileErrorCount = value;
                NotifyPropertyChanged(() => FileErrorCount);
            }
        }

        public async void Init()
        {
            if (isInitted)
            {
                return;
            }
            await semaphoreSlim.WaitAsync();
            try
            {
                await InitInternal();
                isInitted = true;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<bool> InitInternal()
        {
            ServiceHelper.Instance.PropertyChanged += Instance_PropertyChanged;
            Repo.Instance.PropertyChanged += Instance_PropertyChanged;

            try
            {
                using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                {
                    var query = from fileSystemItem in context.FileSystemItems
                                join source in context.Sources on fileSystemItem.SourceId equals source.Id
                                select fileSystemItem;

                    var directoryQuery = query.Where(x => x.IsDirectory == true);

                    var fileQuery = query.Where(x => x.IsDirectory == false);

                    var fileQuerySuccess = query.Where(x => x.IsDirectory == false && x.FileSystemItemStatusEnum == Celsus.Types.FileSystemItemStatusEnum.Done);

                    var fileQueryOmitted = query.Where(x => x.IsDirectory == false && x.FileSystemItemStatusEnum == Celsus.Types.FileSystemItemStatusEnum.Omitted);

                    var fileQueryError = query.Where(x => x.IsDirectory == false && x.FileSystemItemStatusEnum == Celsus.Types.FileSystemItemStatusEnum.StopedWithError);

                    DirectoryCount = await directoryQuery.CountAsync();

                    FileCount = await fileQuery.CountAsync();

                    FileSuccessedCount = await fileQuerySuccess.CountAsync();

                    FileOmittedCount = await fileQueryOmitted.CountAsync();

                    FileErrorCount = await fileQueryError.CountAsync();

                }
            }

            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }

            return true;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(() => ServiceStatus);
            NotifyPropertyChanged(() => ActiveSources);
        }
    }
    public partial class SystemMonitorControl : UserControl
    {
        public SystemMonitorControl()
        {
            InitializeComponent();
            DataContext = SystemMonitorControlModel.Instance;
        }
    }
}
