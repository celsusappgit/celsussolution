using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;

namespace Celsus.Client.Controls.Setup.Database
{
    public partial class DatabaseSetupMainControlModel : BaseModel<DatabaseSetupMainControlModel>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;

        #endregion

        public DatabaseHelper DatabaseHelper { get { return DatabaseHelper.Instance; } }

        public RolesHelper RolesHelper { get { return RolesHelper.Instance; } }

        object installStatus;
        public object InstallStatus
        {
            get
            {
                return installStatus;
            }
            set
            {
                if (Equals(value, installStatus)) return;
                installStatus = value;
                NotifyPropertyChanged(() => InstallStatus);
                NotifyPropertyChanged(() => InstallStatusVisibility);
            }
        }

        public Visibility InstallStatusVisibility
        {
            get
            {
                return InstallStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        object updateStatus;
        public object UpdateStatus
        {
            get
            {
                return updateStatus;
            }
            set
            {
                if (Equals(value, updateStatus)) return;
                updateStatus = value;
                NotifyPropertyChanged(() => UpdateStatus);
                NotifyPropertyChanged(() => UpdateStatusVisibility);
            }
        }

        public Visibility UpdateStatusVisibility
        {
            get
            {
                return UpdateStatus == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        object busyContent;
        public object BusyContent
        {
            get
            {
                return busyContent;
            }
            set
            {
                if (Equals(value, busyContent)) return;
                busyContent = value;
                NotifyPropertyChanged(() => BusyContent);
            }
        }



        object status;
        public object Status
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
                NotifyPropertyChanged(() => StatusVisibility);
            }
        }

        public Visibility StatusVisibility
        {
            get
            {
                return Status == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

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
                NotifyPropertyChanged(() => EnterConnectionParamatersCommand);
                NotifyPropertyChanged(() => SetAsDatabaseRoleCommand);
                NotifyPropertyChanged(() => UpdateCelsusDatabaseCommand);
                NotifyPropertyChanged(() => InstallSQLCommand);
            }
        }

        ICommand enterConnectionParamatersCommand;
        public ICommand EnterConnectionParamatersCommand
        {
            get
            {
                if (enterConnectionParamatersCommand == null)
                    enterConnectionParamatersCommand = new RelayCommand(param => EnterConnectionParamaters(param), param => { return !IsBusy; });
                return enterConnectionParamatersCommand;
            }
        }

        ICommand setAsDatabaseRoleCommand;
        public ICommand SetAsDatabaseRoleCommand
        {
            get
            {
                if (setAsDatabaseRoleCommand == null)
                    setAsDatabaseRoleCommand = new RelayCommand(param => SetAsDatabaseRole(param), param => { return !IsBusy && RolesHelper.DatabaseRoleCount == 0; });
                return setAsDatabaseRoleCommand;
            }
        }


        private async void SetAsDatabaseRole(object param)
        {
            IsBusy = true;
            string serverId = string.Empty;
            string ipAddress = string.Empty;
            string machineName = string.Empty;
            try
            {
                if (DatabaseHelper.ConnectionInfo.IsServerCurrentMachine)
                {
                    serverId = ComputerHelper.Instance.ServerId;
                    ipAddress = ComputerHelper.Instance.IPAddress;
                    machineName = Environment.MachineName;
                }
                else
                {
                    serverId = Guid.Empty.ToString().ToUpper();

                    if (DatabaseHelper.ConnectionInfo.MachineNameIsAnIPAddress)
                    {
                        ipAddress = DatabaseHelper.ConnectionInfo.MachineName;
                        machineName = ComputerHelper.Instance.GetMachineNameFromIPAddress(ipAddress);
                    }
                    else
                    {
                        ipAddress = ComputerHelper.Instance.GetIPAddressFromMachineName(DatabaseHelper.ConnectionInfo.MachineName);
                        machineName = DatabaseHelper.ConnectionInfo.MachineName;
                    }
                }

                await RolesHelper.Instance.AddDatabaseRole(serverId, ipAddress, machineName);
            }
            catch (Exception)
            {
                throw;
            }
            IsBusy = false;
        }

        ICommand updateCelsusDatabaseCommand;
        public ICommand UpdateCelsusDatabaseCommand
        {
            get
            {
                if (updateCelsusDatabaseCommand == null)
                    updateCelsusDatabaseCommand = new RelayCommand(param => UpdateCelsusDatabase(param), param => { return !IsBusy; });
                return updateCelsusDatabaseCommand;
            }
        }

        private async void UpdateCelsusDatabase(object param)
        {
            await UpdateCelsusDatabaseHelper();
        }

        private async Task<bool> UpdateCelsusDatabaseHelper()
        {
            try
            {
                var result = await DatabaseHelper.Instance.Upgrade();
                if (result)
                {
                    UpdateStatus = "DatabaseUpgradedSuccessfully".ConvertToBindableText();
                    //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => UpdateStatus = "Database upgraded successfully.".ConvertToBindableText()));
                }
                return result;
            }
            catch (Exception ex)
            {
                UpdateStatus = "DatabaseUpgradeError".ConvertToBindableText();
                logger.Error(ex, "Error in UpdateCelsusDatabaseHelper");
                //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Status = "Database upgrade error.".ConvertToBindableText()));
            }
            return false;
        }


        private void EnterConnectionParamaters(object obj)
        {
            var connectionStringControl = new ConnectionStringControl();
            RadWindow newWindow = new RadWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = (App.Current.MainWindow as FirstWindow),
                Content = connectionStringControl,
                SizeToContent = true,
                Header = TranslationSource.Instance["ConnectionStringControl"]
            };
            RadWindowInteropHelper.SetAllowTransparency(newWindow, false);
            newWindow.ShowDialog();
            if (DatabaseHelper.Instance.Status == DatabaseHelperStatusEnum.CelsusDatabaseReachable)
            {

            }
        }

        ICommand installSQLCommand;
        public ICommand InstallSQLCommand
        {
            get
            {
                if (installSQLCommand == null)
                    installSQLCommand = new RelayCommand(param => InstallSQL(param), param => { return !IsBusy; });
                return installSQLCommand;
            }
        }

        private void InstallSQL(object obj)
        {
            IsBusy = true;

            logger.Trace($"Install SQL Server started.");

            InstallStatus = "StartingDownload".ConvertToBindableText();

            var sqlSetupPath = FileHelper.GetUnusedFileName(System.IO.Path.GetTempPath(), "SqlExe.zip");

            logger.Trace($"SQL Server setup file path is {sqlSetupPath}");

            var url = DownloadUris.SqlExpress;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                try
                {
                    var uri = ApplicationDeployment.CurrentDeployment.UpdateLocation;
                    var formattedUri = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped) + string.Join("", uri.Segments, 0, uri.Segments.Length - 1);
                    url = formattedUri.Substring(0, formattedUri.Length - 1);
                    url = url + "/Setup/SQLEX.zip";
                    logger.Trace(url);
                }
                catch (InvalidDeploymentException)
                {
                }
            }

            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = "Starting download.".ConvertToBindableText()));

            try
            {
                logger.Trace($"SQL Server setup file will be downloaded from {url}");

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressChanged;
                    webClient.DownloadFileCompleted += DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(url), sqlSetupPath, sqlSetupPath);
                }

            }
            catch (Exception ex)
            {
                IsBusy = false;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "ErrorInStartingDownload".ConvertToBindableText()));
                logger.Error(ex, $"Exception has been thrown when downloading SQL Server setup file.");
            }
        }

        private void DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Downloaded100".ConvertToBindableText("SQLServerSetupFile", e.ProgressPercentage)));
        }

        private async void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            var exceptionString = string.Empty;
            if (e.Error != null)
            {
                InstallStatus = "ErrorInStartingDownload".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Error downlading setup file.".ConvertToBindableText()));
                exceptionString = e.Error.ToString();
                logger.Error(e.Error, $"SQL Server setup file downloaded with error. IsCancelled: {e.Cancelled}. Error: {exceptionString}");
                IsBusy = false;
            }
            else
            {
                InstallStatus = "SQLServerSetupFileDownloaded".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "SQL Server setup file downloaded.".ConvertToBindableText()));
                logger.Trace($"SQL Server setup file downloaded. IsCancelled: {e.Cancelled}");
                //IsBusy = false;
            }

            if (e.Cancelled == false && e.Error == null)
            {
                InstallStatus = "ExtractingSetupFile".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Extracting setup file.".ConvertToBindableText()));

                var extractPath = FileHelper.GetUnusedFolderName(System.IO.Path.GetTempPath(), "SqlExtract");

                var setupExePath = System.IO.Path.Combine(System.IO.Path.Combine(extractPath, "SQLEX"), "Setup.exe");

                logger.Trace($"SQL Server setup file downloaded. ExtractPath: {extractPath}, SetupExePath: {setupExePath}");

                try
                {
                    await Task.Run(() => ZipFile.ExtractToDirectory(e.UserState.ToString(), extractPath));
                    //ZipFile.ExtractToDirectory(e.UserState.ToString(), extractPath);
                    logger.Trace($"SQL Server setup file extracted. ExtractPath: {extractPath}");

                    InstallStatus = "SQLServerSetupFileExtracted".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "SQL Server setup file extracted.".ConvertToBindableText()));
                }
                catch (Exception ex)
                {
                    InstallStatus = $"SQL Server setup file extract error.".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = $"SQL Server setup file extract error.".ConvertToBindableText()));
                    logger.Error(ex, $"SQL Server setup file extract error.");
                    IsBusy = false;
                    return;
                }

                InstallStatus = "CreatingConfigFile".ConvertToBindableText();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Creating config file.".ConvertToBindableText()));

                var uri = new Uri("/Resources/Setup/Database/Config.ini", UriKind.Relative);

                StreamResourceInfo resourceStream = null;
                try
                {
                    resourceStream = Application.GetResourceStream(uri);
                }
                catch (Exception ex)
                {
                    InstallStatus = "ConfigFileResourceError".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Config file resource error.".ConvertToBindableText()));
                    logger.Error(ex, $"Config file resource error.");
                    IsBusy = false;
                    return;
                }

                var configFilePath = FileHelper.GetUnusedFileName(System.IO.Path.GetTempPath(), "Config.ini");

                logger.Trace($"Config file path {configFilePath}");

                try
                {
                    using (var reader = new StreamReader(resourceStream.Stream))
                    {
                        var text = reader.ReadToEnd();
                        File.WriteAllText(configFilePath, text);
                    }
                    InstallStatus = "ConfigFileSaved".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Config file saved.".ConvertToBindableText()));
                }
                catch (Exception ex)
                {
                    InstallStatus = "ConfigFileWriteError".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Config file write error.".ConvertToBindableText()));
                    logger.Error(ex, $"Config file write error.");
                    IsBusy = false;
                    return;
                }

                var options = $" /ConfigurationFile=\"{configFilePath}\"";

                logger.Trace($"Process start options: {options}, FileName: {setupExePath}");

                InstallStatus = "SetupIsRunningDoNotCloseThisWindowItWill".ConvertToBindableText();

                BusyContent = "SetupIsRunningDoNotCloseThisWindowItWill".ConvertToBindableText();

                //RadWindow.Alert(new DialogParameters() { Content = LocHelper.GetWord("Setup will begin shortly.Do not close black window.It will take long time to install.Please be patient."), DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Information", Owner = App.Current.MainWindow });
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "SetupIsRunningDoNotCloseThisWindowItWill".ConvertToBindableText()));

                var processHelper = new ProcessHelper
                {
                    FileName = setupExePath,
                    Arguments = options
                };


                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Setup completed successfully.".ConvertToBindableText()));

                await Task.Run(() => processHelper.RunProcess(true));

                BusyContent = null;

                InstallStatus = "SetupCompletedSuccessfully".ConvertToBindableText();

                if (processHelper.Exception != null)
                {
                    InstallStatus = "ProcessError".ConvertToBindableText();
                    //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => InstallStatus = "Process error.".ConvertToBindableText()));
                    logger.Error(processHelper.Exception, $"Process error while installing SQL Server.");
                    return;
                }

                if (processHelper.ErrorDatas.Count() > 0)
                {
                    foreach (var item in processHelper.ErrorDatas)
                    {
                        logger.Trace($"ErrorData {item}");
                    }
                }
                if (processHelper.OutputDatas.Count() > 0)
                {
                    foreach (var item in processHelper.OutputDatas)
                    {
                        logger.Trace($"OutputData {item}");
                    }
                }
                //if (processHelper.ErrorDatas.Count(x => x.IndexOf("[Failure]") >= 0) > 0)
                //{
                //    var cmdOut = string.Join(Environment.NewLine, processHelper.ErrorDatas.ToArray());
                //    logger.Error($"Error installing Service. Details: {cmdOut}");
                //}
                if (processHelper.OutputDatas.Count(x => x.IndexOf("Success") >= 0) > 0)
                {
                    SettingsHelper.Instance.ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Celsus;Integrated Security=True;Connect Timeout=15";
                }

                InstallStatus = "WeWillUpdateCelsusDatabaseNow".ConvertToBindableText();

                var updateResult = await UpdateCelsusDatabaseHelper();

                if (updateResult)
                {
                    SetAsDatabaseRole(null);
                }
                IsBusy = false;
                logger.Trace($"Process ended.");
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
                isInitted = true;
            }
        }
    }
    public partial class DatabaseSetupMainControl : UserControl
    {
        public DatabaseSetupMainControl()
        {
            InitializeComponent();
            DataContext = DatabaseSetupMainControlModel.Instance;
        }


    }
}
