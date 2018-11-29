using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NLog;
using NLog.Targets;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Celsus.Client.Wpf.Controls.Management
{
    /// <summary>
    /// Interaction logic for DatabaseManagement.xaml
    /// </summary>
    public partial class DatabaseManagement : UserControl
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        ModelObject model = null;

        public ModelObject Model
        {
            get
            {
                return model;
            }
            set
            {
                if (Equals(value, model)) return;
                model = value;
                NotifyPropertyChanged(() => Model);
            }
        }
        public DatabaseManagement()
        {
            InitializeComponent();

            model = new ModelObject();
            Loaded += DatabaseManagement_Loaded;


        }

        private void DatabaseManagement_Loaded(object sender, RoutedEventArgs e)
        {
            LbLogs.ItemsSource = MyClass.logs;


            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.CS))
            {
                Model.ConnectionInfo.ConnectionString = @"Server=.\SQLEXPRESS; Integrated Security=SSPI;";
            }
            else
            {
                Model.ConnectionInfo.ConnectionString = Properties.Settings.Default.CS;
            }
            //DataContext = Model;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void CheckSQLServer_Click(object sender, RoutedEventArgs e)
        {
            logger.Trace($"Check SQL Server started.");

            //var target = LogManager.Configuration.FindTargetByName<MemoryTarget>("MemoryTarget");
            //var logs = target.Logs;

            Model.AddMode("Working");
            try
            {
                Model.RemoveMode("SQLOK");
                Model.RemoveMode("NeedSqlInstall");
                Model.ConnectionInfo.InitialCatalog = "Master";
                using (var sqlConnection = new SqlConnection(Model.ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"SQL Server found.");
                Model.AddMode("SQLOK");
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 18456)
                {
                    logger.Trace(sqlException, $"Login failed.");
                }
                else
                {
                    logger.Trace(sqlException, $"SQL Server is not reachable.");
                    Model.AddMode("NeedSqlInstall");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
                Model.AddMode("NeedSqlInstall");
            }
            finally
            {
            }

            Model.RemoveMode("Working");
        }

        private void InstallSQLServer_Click(object sender, RoutedEventArgs e)
        {
            logger.Trace($"Install SQL Server started.");

            var sqlSetupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SqlExe.exe");

            logger.Trace($"SQL Server setup file path is {sqlSetupPath}");

            var url = "https://download.microsoft.com/download/E/F/2/EF23C21D-7860-4F05-88CE-39AA114B014B/SQLEXPRADV_x64_ENU.exe";
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
                logger.Error(ex, $"Exception has been thrown when downloading SQL Server setup file.");
            }

        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var exceptionString = string.Empty;
            if (e.Error != null)
            {
                exceptionString = e.Error.ToString();
                logger.Error(e.Error, $"SQL Server setup file downloaded with error. IsCancelled: {e.Cancelled}. Error: {exceptionString}");
            }
            else
            {
                logger.Trace($"SQL Server setup file downloaded. IsCancelled: {e.Cancelled}");
            }

            if (e.Cancelled == false && e.Error == null)
            {

                var extractPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SqlExtract");

                int index = 0;
                while (true)
                {
                    index++;
                    if (Directory.Exists(extractPath))
                    {
                        extractPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SqlExtract" + index);
                    }
                    else
                    {
                        break;
                    }
                }

                var setupExePath = System.IO.Path.Combine(System.IO.Path.Combine(extractPath, "SQLEXPRADV_x64_ENU"), "Setup.exe");

                logger.Trace($"SQL Server setup file downloaded. ExtractPath: {extractPath}, SetupExePath: {setupExePath}");

                try
                {
                    ZipFile.ExtractToDirectory(e.UserState.ToString(), extractPath);
                    logger.Trace($"SQL Server setup file extracted. ExtractPath: {extractPath}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"SQL Server setup file extract error.");
                    return;
                }


                var uri = new Uri("/Resources/Setup/Database/Config.ini", UriKind.Relative);

                StreamResourceInfo resourceStream = null;
                try
                {
                    resourceStream = Application.GetResourceStream(uri);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Config file resource error.");
                    return;
                }

                var configFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Config.ini");
                index = 0;
                while (true)
                {
                    index++;
                    if (File.Exists(configFilePath))
                    {
                        configFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Config{index}.ini");
                    }
                    else
                    {
                        break;
                    }
                }

                logger.Trace($"Config file path {configFilePath}");

                try
                {
                    using (var reader = new StreamReader(resourceStream.Stream))
                    {
                        var text = reader.ReadToEnd();
                        File.WriteAllText(configFilePath, text);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Config file write error.");
                    return;
                }

                var options = $" /ConfigurationFile=\"{configFilePath}\"";

                logger.Trace($"Process start options: {options}, FileName: {setupExePath}");

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = setupExePath,
                    Arguments = options
                };
                process.StartInfo = processStartInfo;
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Process error.");
                    return;
                }

                logger.Trace($"Process ended.");
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RadProgressBar1.Value = e.ProgressPercentage;
        }

        private async void CheckDatabase_Click(object sender, RoutedEventArgs e)
        {
            logger.Trace($"Check Database started.");

            RadProgressBarCheckDatabase.IsIndeterminate = true;
            try
            {
                Model.ConnectionInfo.InitialCatalog = "Celsus";
                using (var sqlConnection = new SqlConnection(Model.ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"Connection opened to Celsus database.");
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 4060)
                {
                    Model.AddMode("NeedDatabaseInstall");
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                }
                else
                {
                    Model.AddMode("NeedDatabaseInstall");
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                }
            }
            catch (Exception ex)
            {
                Model.AddMode("NeedDatabaseInstall");
                logger.Error(ex, $"Error occured connecting Celsus database.");
            }
            finally
            {
                RadProgressBarCheckDatabase.IsIndeterminate = false;
            }
        }

        private void InstallDatabase_Click(object sender, RoutedEventArgs e)
        {
            DacServices dacServices = new DacServices(Model.ConnectionInfo.ConnectionString);

            var dacOptions = new DacDeployOptions();
            dacOptions.BlockOnPossibleDataLoss = true;
            dacOptions.AllowIncompatiblePlatform = true;
            dacOptions.BlockWhenDriftDetected = true;

            dacServices.Message += DacServices_Message;
            dacServices.ProgressChanged += DacServices_ProgressChanged;

            using (DacPackage dacPackage = DacPackage.Load(@"C:\Users\efeo\source\repos\Celsus\Celsus.Database\bin\Debug\Celsus.Database.dacpac"))
            {
                dacServices.Deploy(dacPackage, @"Celsus", upgradeExisting: true, options: dacOptions, cancellationToken: null);
            }


        }

        private void DacServices_ProgressChanged(object sender, DacProgressEventArgs e)
        {
            var message = $"Status: {e.Status}, Message: {e.Message}";

            Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => TxtInstallDatabase.Text = TxtInstallDatabase.Text + message + Environment.NewLine));
        }

        private void DacServices_Message(object sender, DacMessageEventArgs e)
        {
            var message = $"Message: {e.Message}";

            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() => TxtInstallDatabase.Text = TxtInstallDatabase.Text + message + Environment.NewLine));
        }
    }

    public class MyClass
    {
        public static ObservableCollection<LogItem> logs = new ObservableCollection<LogItem>();
        public static void LogMethod(string level, string message, string exception)
        {
            if (exception != null)
            {
                logs.Insert(0, new LogItem() { Level = level, Message = message, Exception = exception });
            }
            else
            {
                logs.Insert(0, new LogItem() { Level = level, Message = message });
            }
        }
    }

    public class LogItem
    {
        public LogItem()
        {
        }

        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public Visibility ExceptionVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(Exception) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }



}
