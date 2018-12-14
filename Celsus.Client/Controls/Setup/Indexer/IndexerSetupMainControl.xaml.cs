using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;

namespace Celsus.Client.Controls.Setup.Database
{
    public partial class IndexerSetupMainControlModel : BaseModel<IndexerSetupMainControlModel>, MustInit
    {
        #region Members

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private bool isInitted = false;

        #endregion

        public IndexerHelper IndexerHelper { get { return IndexerHelper.Instance; } }
        public TesseractHelper TesseractHelper { get { return TesseractHelper.Instance; } }

        public ImageMagickHelper ImageMagickHelper { get { return ImageMagickHelper.Instance; } }

        public XPdfToolsHelper XPdfToolsHelper { get { return XPdfToolsHelper.Instance; } }

        public ServiceHelper ServiceHelper { get { return ServiceHelper.Instance; } }

        public Visibility CollapsedButVisibleInDesign
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(new Button()))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        //object status;
        //public object Status
        //{
        //    get
        //    {
        //        return status;
        //    }
        //    set
        //    {
        //        if (Equals(value, status)) return;
        //        status = value;
        //        NotifyPropertyChanged(() => Status);
        //        NotifyPropertyChanged(() => StatusVisibility);
        //    }
        //}

        //public Visibility StatusVisibility
        //{
        //    get
        //    {
        //        return Status == null ? Visibility.Collapsed : Visibility.Visible;
        //    }
        //}
        //
        object tesseractStatus;
        public object TesseractStatus
        {
            get
            {
                return tesseractStatus;
            }
            set
            {
                if (Equals(value, tesseractStatus)) return;
                tesseractStatus = value;
                NotifyPropertyChanged(() => TesseractStatus);
                NotifyPropertyChanged(() => TesseractStatusVisibility);
            }
        }

        public Visibility TesseractStatusVisibility
        {
            get
            {
                return TesseractStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        //
        object imageMagickStatus;
        public object ImageMagickStatus
        {
            get
            {
                return imageMagickStatus;
            }
            set
            {
                if (Equals(value, imageMagickStatus)) return;
                imageMagickStatus = value;
                NotifyPropertyChanged(() => ImageMagickStatus);
                NotifyPropertyChanged(() => ImageMagickStatusVisibility);
            }
        }

        public Visibility ImageMagickStatusVisibility
        {
            get
            {
                return ImageMagickStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        //
        object xpdfToolsStatus;
        public object XPdfToolsStatus
        {
            get
            {
                return xpdfToolsStatus;
            }
            set
            {
                if (Equals(value, xpdfToolsStatus)) return;
                xpdfToolsStatus = value;
                NotifyPropertyChanged(() => XPdfToolsStatus);
                NotifyPropertyChanged(() => XPdfToolsStatusVisibility);
            }
        }

        public Visibility XPdfToolsStatusVisibility
        {
            get
            {
                return XPdfToolsStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        //
        object serviceStatus;
        public object ServiceStatus
        {
            get
            {
                return serviceStatus;
            }
            set
            {
                if (Equals(value, serviceStatus)) return;
                serviceStatus = value;
                NotifyPropertyChanged(() => ServiceStatus);
                NotifyPropertyChanged(() => ServiceStatusVisibility);
            }
        }

        public Visibility ServiceStatusVisibility
        {
            get
            {
                return ServiceStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        //
        object indexerRoleStatus;
        public object IndexerRoleStatus
        {
            get
            {
                return indexerRoleStatus;
            }
            set
            {
                if (Equals(value, indexerRoleStatus)) return;
                indexerRoleStatus = value;
                NotifyPropertyChanged(() => IndexerRoleStatus);
                NotifyPropertyChanged(() => IndexerRoleStatusVisibility);
            }
        }

        public Visibility IndexerRoleStatusVisibility
        {
            get
            {
                return IndexerRoleStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (Equals(value, isBusy)) return;
                isBusy = value;
                NotifyPropertyChanged(() => IsBusy);
            }
        }

        bool isBusyIndexerRole;
        public bool IsBusyIndexerRole
        {
            get
            {
                return isBusyIndexerRole;
            }
            set
            {
                if (Equals(value, isBusyIndexerRole)) return;
                isBusyIndexerRole = value;
                NotifyPropertyChanged(() => IsBusyIndexerRole);
                NotifyPropertyChanged(() => SetAsIndexerRoleCommand);
            }
        }


        bool isBusyTesseract;
        public bool IsBusyTesseract
        {
            get
            {
                return isBusyTesseract;
            }
            set
            {
                if (Equals(value, isBusyTesseract)) return;
                isBusyTesseract = value;
                NotifyPropertyChanged(() => IsBusyTesseract);
                NotifyPropertyChanged(() => InstallTesseractCommand);
            }
        }

        bool isBusyImageMagick;
        public bool IsBusyImageMagick
        {
            get
            {
                return isBusyImageMagick;
            }
            set
            {
                if (Equals(value, isBusyImageMagick)) return;
                isBusyImageMagick = value;
                NotifyPropertyChanged(() => IsBusyImageMagick);
                NotifyPropertyChanged(() => InstallImageMagickCommand);
            }
        }

        bool isBusyXPdsTools;
        public bool IsBusyXPdfTools
        {
            get
            {
                return isBusyXPdsTools;
            }
            set
            {
                if (Equals(value, isBusyXPdsTools)) return;
                isBusyXPdsTools = value;
                NotifyPropertyChanged(() => IsBusyXPdfTools);
                NotifyPropertyChanged(() => InstallXPdfToolsCommand);
            }
        }

        ICommand installTesseractCommand;
        public ICommand InstallTesseractCommand
        {
            get
            {
                if (installTesseractCommand == null)
                    installTesseractCommand = new RelayCommand(param => InstallTesseract(param), param => { return !IsBusyTesseract; });
                return installTesseractCommand;
            }
        }
        private void InstallTesseract(object obj)
        {
            IsBusy = true;

            try
            {
                IsBusyTesseract = true;
                Setup("TesseractPath", DownloadUris.Tesseract);
            }
            catch (Exception ex)
            {
                IsBusy = false;
                TesseractStatus = "ErrorInInstallingTesseract".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => TesseractStatus = "Error in installing Tesseract.".ConvertToBindableText()));
                logger.Error(ex, $"Error in installing Tesseract.");
            }
            IsBusy = false;
        }

        ICommand installImageMagickCommand;
        public ICommand InstallImageMagickCommand
        {
            get
            {
                if (installImageMagickCommand == null)
                    installImageMagickCommand = new RelayCommand(param => InstallImageMagick(param), param => { return !IsBusyImageMagick; });
                return installImageMagickCommand;
            }
        }
        private void InstallImageMagick(object obj)
        {
            IsBusy = true;

            try
            {
                IsBusyImageMagick = true;
                Setup("ImageMagickPath", DownloadUris.ImageMagick);
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ImageMagickStatus = "ErrorInInstallingImageMagick".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ImageMagickStatus = "Error in installing ImageMagick.".ConvertToBindableText()));
                logger.Error(ex, $"Error in installing ImageMagick.");
            }
            IsBusy = false;
        }

        ICommand installXPdfToolsCommand;
        public ICommand InstallXPdfToolsCommand
        {
            get
            {
                if (installXPdfToolsCommand == null)
                    installXPdfToolsCommand = new RelayCommand(param => InstallXPdfTools(param), param => { return !IsBusyXPdfTools; });
                return installXPdfToolsCommand;
            }
        }
        private void InstallXPdfTools(object obj)
        {
            IsBusy = true;

            try
            {
                IsBusyXPdfTools = true;
                Setup("XPdfToolsPath", DownloadUris.XPdfTools);
            }
            catch (Exception ex)
            {
                IsBusy = false;
                XPdfToolsStatus = "ErrorInInstallingXPdfTools".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => XPdfToolsStatus = "Error in installing XPdfTools.".ConvertToBindableText()));
                logger.Error(ex, $"Exception has been thrown when downloading SQL Server setup file.");
            }
            IsBusy = false;
        }

        ICommand installServiceCommand;
        public ICommand InstallServiceCommand
        {
            get
            {
                if (installServiceCommand == null)
                    installServiceCommand = new RelayCommand(param => InstallService(param), param => CanInstallService(param));
                return installServiceCommand;
            }
        }

        private bool CanInstallService(object param)
        {
            if (RolesHelper.Instance.IsIndexerRoleThisComputer == false)
            {
                return true;
            }
            //else
            //{
            //    if (string.Compare(RolesHelper.Instance.IndexerRoleComputerName, Environment.MachineName, StringComparison.InvariantCultureIgnoreCase) == 0)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        private void InstallService(object obj)
        {
            IsBusy = true;

            try
            {
                if (ServiceInstallerHelper.Instance.IsAdmin == false)
                {
                    ServiceStatus = "YouNeedToStartCelsusWithAdministratorPri".ConvertToBindableText();
                    return;
                }
                var result = ServiceHelper.Instance.InstallOrUpgrade();
                if (result == false)
                {
                    ServiceStatus = "ErrorInInstallingService".ConvertToBindableText();
                }
                else
                {
                    ServiceStatus = "ServiceInstalled".ConvertToBindableText();
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ServiceStatus = "ErrorInInstallingService".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ServiceStatus = "Error in installing service.".ConvertToBindableText()));
                logger.Error(ex, $"Exception has been thrown when downloading SQL Server setup file.");
            }
            IsBusy = false;
        }

        ICommand setAsIndexerRoleCommand;
        public ICommand SetAsIndexerRoleCommand
        {
            get
            {
                if (setAsIndexerRoleCommand == null)
                    setAsIndexerRoleCommand = new RelayCommand(param => SetAsIndexerRole(param), param => CanSetAsIndexerRole(param));
                return setAsIndexerRoleCommand;
            }
        }


        private bool CanSetAsIndexerRole(object param)
        {
            if (IsBusyIndexerRole)
            {
                return false;
            }
            if (RolesHelper.Instance.MaxAllowedIndexerRoleCount.HasValue && RolesHelper.Instance.IndexerRoleCount.HasValue)
            {
                if (RolesHelper.Instance.IndexerRoleCount.Value < RolesHelper.Instance.MaxAllowedIndexerRoleCount.Value)
                {
                    if (RolesHelper.Instance.IsIndexerRoleThisComputer == false)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private async void SetAsIndexerRole(object param)
        {
            IsBusyIndexerRole = true;
            try
            {
                if (RolesHelper.Instance.MaxAllowedIndexerRoleCount.HasValue && RolesHelper.Instance.IndexerRoleCount.HasValue)
                {
                    if (RolesHelper.Instance.IndexerRoleCount.Value < RolesHelper.Instance.MaxAllowedIndexerRoleCount.Value)
                    {
                        if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense)
                        {
                            if (LicenseHelper.Instance.SetIndexer(ComputerHelper.Instance.ServerId))
                            {
                                var addIndexerRole = await RolesHelper.Instance.AddIndexerRole();
                                if (addIndexerRole == true)
                                {
                                    IndexerRoleStatus = "IndexerRoleSettedSuccessfully".ConvertToBindableText();
                                }
                                else
                                {
                                    LicenseHelper.Instance.SetIndexer("");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IsBusyIndexerRole = false;
                IndexerRoleStatus = "ErrorInSettingIndexerRole".ConvertToBindableText();
                logger.Error(ex, $"Error in setting as Indexer Role.");
            }

            IsBusyIndexerRole = false;

            NotifyPropertyChanged(() => SetAsIndexerRoleCommand);
        }
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var settingName = (e.UserState as DownloadFileAsyncState).SettingName;
            var componentName = settingName.ToString().Replace("Path", "");

            if (componentName == "Tesseract")
            {
                TesseractStatus = "Downloaded100".ConvertToBindableText(componentName, e.ProgressPercentage);
            }
            else if (componentName == "ImageMagick")
            {
                ImageMagickStatus = "Downloaded100".ConvertToBindableText(componentName, e.ProgressPercentage);
            }
            else if (componentName == "XPdfTools")
            {
                XPdfToolsStatus = "Downloaded100".ConvertToBindableText(componentName, e.ProgressPercentage);
            }
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = "Downloaded100".ConvertToBindableText(componentName, e.ProgressPercentage)));
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var exceptionString = string.Empty;

            var state = e.UserState as DownloadFileAsyncState;
            var downloadedFilePath = state.DownloadPath;
            var settingName = state.SettingName;
            var componentName = settingName.ToString().Replace("Path", "");
            if (e.Error != null)
            {
                exceptionString = e.Error.ToString();
                logger.Error(e.Error, $"{componentName} setup file downloaded with error. IsCancelled: {e.Cancelled}. Error: {exceptionString}");

                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = "ErrorInDownload".ConvertToBindableText()));

                if (componentName == "Tesseract")
                {
                    TesseractStatus = "ErrorInDownload".ConvertToBindableText();
                }
                else if (componentName == "ImageMagick")
                {
                    ImageMagickStatus = "ErrorInDownload".ConvertToBindableText();
                }
                else if (componentName == "XPdfTools")
                {
                    XPdfToolsStatus = "ErrorInDownload".ConvertToBindableText();
                }
            }
            else
            {
                logger.Trace($"{componentName} setup file downloaded. IsCancelled: {e.Cancelled}");

            }

            if (e.Cancelled == false && e.Error == null)
            {
                ExtractInBackground(state);
            }
            else
            {
                if (componentName == "Tesseract")
                {
                    isBusyTesseract = false;
                }
                else if (componentName == "ImageMagick")
                {
                    IsBusyImageMagick = false;
                }
                else if (componentName == "XPdfTools")
                {
                    IsBusyXPdfTools = false;
                }
            }
        }

        #region BackgroundWorker
        public void ExtractInBackground(DownloadFileAsyncState state)
        {
            var downloadedFilePath = state.DownloadPath;
            var settingName = state.SettingName;
            var componentName = settingName.ToString().Replace("Path", "");

            var backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            backgroundWorker.RunWorkerAsync(new BackgroundWorkerState() { DownloadPath = downloadedFilePath, SettingName = settingName });
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arg = (e.Argument as BackgroundWorkerState);
            var downloadedFilePath = arg.DownloadPath;
            var settingName = arg.SettingName;
            var componentName = settingName.ToString().Replace("Path", "");

            (sender as BackgroundWorker).ReportProgress(1, arg);

            var extractPath = FileHelper.GetUnusedFolderName(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), componentName);

            try
            {
                ZipFile.ExtractToDirectory(downloadedFilePath, extractPath);

                logger.Trace($"{componentName} setup file extracted. ExtractPath: {extractPath}");

                e.Result = new BackgroundWorkerResult() { ExtractPath = extractPath, SettingName = settingName };

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"{componentName} setup file extract error.");
                e.Result = null;

            }

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var result = e.UserState as BackgroundWorkerState;
            var componentName = result.SettingName.ToString().Replace("Path", "");

            if (componentName == "Tesseract")
            {
                TesseractStatus = "Extracting".ConvertToBindableText(componentName);
            }
            else if (componentName == "ImageMagick")
            {
                ImageMagickStatus = "Extracting".ConvertToBindableText(componentName);
            }
            else if (componentName == "XPdfTools")
            {
                XPdfToolsStatus = "Extracting".ConvertToBindableText(componentName);
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (e.Result as BackgroundWorkerResult);
            var extractPath = result.ExtractPath;
            var settingName = result.SettingName;
            var componentName = result.SettingName.ToString().Replace("Path", "");

            //Status = "Done0".ConvertToBindableText(componentName);

            if (componentName == "ImageMagick")
            {

                SettingsHelper.Instance.ImageMagickPath = extractPath;
            }
            if (componentName == "Tesseract")
            {
                SettingsHelper.Instance.TesseractPath = extractPath;
            }
            if (componentName == "XPdfTools")
            {
                SettingsHelper.Instance.XPdfToolsPath = extractPath;
            }

            if (componentName == "Tesseract")
            {
                TesseractStatus = "Done0".ConvertToBindableText(componentName);
            }
            else if (componentName == "ImageMagick")
            {
                ImageMagickStatus = "Done0".ConvertToBindableText(componentName);
            }
            else if (componentName == "XPdfTools")
            {
                XPdfToolsStatus = "Done0".ConvertToBindableText(componentName);
            }

            if (componentName == "Tesseract")
            {
                isBusyTesseract = false;
            }
            else if (componentName == "ImageMagick")
            {
                IsBusyImageMagick = false;
            }
            else if (componentName == "XPdfTools")
            {
                IsBusyXPdfTools = false;
            }

            //SettingsManager.Instance.AddOrUpdateSetting(settingName, extractPath);

        }



        #endregion


        public async void Init()
        {
            if (isInitted)
            {
                return;
            }
            await semaphoreSlim.WaitAsync();
            try
            {
                //await AddRole(); 
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

        public IndexerSetupMainControlModel()
        {
            //IndexerHelper.PropertyChanged += IndexerHelper_PropertyChanged;
        }

        //private async void IndexerHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (IndexerHelper.Instance.Status == IndexerHelperStatusEnum.Ok)
        //        await AddRole();
        //}

        //private static async Task AddRole()
        //{
        //    if (IndexerHelper.Instance.Status == IndexerHelperStatusEnum.Ok)
        //    {
        //        await RolesHelper.Instance.AddServerRole(Celsus.Types.ServerRoleEnum.Indexer);
        //    }
        //}

        private void Setup(string settingName, string url)
        {


            var componentName = settingName.ToString().Replace("Path", "");

            logger.Trace($"Install {componentName} started.");

            var downloadPath = FileHelper.GetUnusedFileName(System.IO.Path.GetTempPath(), $"{componentName}.7z");

            logger.Trace($"{componentName} setup file path is {downloadPath}");

            //Status = "Downloading0".ConvertToBindableText(componentName);

            if (componentName == "Tesseract")
            {
                TesseractStatus = "Downloading0".ConvertToBindableText(componentName);
            }
            else if (componentName == "ImageMagick")
            {
                ImageMagickStatus = "Downloading0".ConvertToBindableText(componentName);
            }
            else if (componentName == "XPdfTools")
            {
                XPdfToolsStatus = "Downloading0".ConvertToBindableText(componentName);
            }

            try
            {
                logger.Trace($"{componentName} setup file will be downloaded from {url}");

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressChanged;
                    webClient.DownloadFileCompleted += DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(url), downloadPath, new DownloadFileAsyncState() { DownloadPath = downloadPath, SettingName = settingName });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when downloading {componentName} setup file.");

                if (componentName == "Tesseract")
                {
                    isBusyTesseract = false;
                }
                else if (componentName == "ImageMagick")
                {
                    IsBusyImageMagick = false;
                }
                else if (componentName == "XPdfTools")
                {
                    IsBusyXPdfTools = false;
                }
            }
        }


    }
    public partial class IndexerSetupMainControl : UserControl
    {
        public IndexerSetupMainControl()
        {
            InitializeComponent();
            DataContext = IndexerSetupMainControlModel.Instance;
        }
    }

    public class DownloadFileAsyncState
    {
        public DownloadFileAsyncState()
        {
        }

        public string DownloadPath { get; set; }
        public string SettingName { get; set; }
    }

    internal class BackgroundWorkerState
    {
        public BackgroundWorkerState()
        {
        }

        public string DownloadPath { get; set; }
        public string SettingName { get; set; }
    }

    internal class BackgroundWorkerResult
    {
        public BackgroundWorkerResult()
        {
        }

        public string ExtractPath { get; set; }
        public string SettingName { get; set; }
    }
}
