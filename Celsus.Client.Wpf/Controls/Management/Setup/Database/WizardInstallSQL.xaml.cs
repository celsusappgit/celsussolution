using Celsus.Client.Wpf.Types;
using Celsus.Types.NonDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
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

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    /// <summary>
    /// Interaction logic for WizardInstallSQL.xaml
    /// </summary>
    public partial class WizardInstallSQL : UserControl
    {
        public WizardSQL Main { get; set; }
        public string NextParam { get; set; }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public WizardInstallSQL()
        {
            InitializeComponent();
            RadProgressBar1.Visibility = Visibility.Hidden;
        }

        public async Task Do()
        {
            InstallSQLServer_Click();
        }

        private void InstallSQLServer_Click()
        {
            logger.Trace($"Install SQL Server started.");

            var sqlSetupPath = FileHelper.GetUnusedFileName(System.IO.Path.GetTempPath(), "SqlExe.zip");

            logger.Trace($"SQL Server setup file path is {sqlSetupPath}");

            var url = DownloadUris.SqlExpress;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Starting download."));


            try
            {
                logger.Trace($"SQL Server setup file will be downloaded from {url}");

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressChanged;
                    webClient.DownloadFileCompleted += DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(url), sqlSetupPath, sqlSetupPath);
                }

                RadProgressBar1.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Error in starting download."));
                logger.Error(ex, $"Exception has been thrown when downloading SQL Server setup file.");
            }

        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            RadProgressBar1.Visibility = Visibility.Hidden;

            var exceptionString = string.Empty;
            if (e.Error != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Error downlading setup file."));
                exceptionString = e.Error.ToString();
                logger.Error(e.Error, $"SQL Server setup file downloaded with error. IsCancelled: {e.Cancelled}. Error: {exceptionString}");
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "SQL Server setup file downloaded."));
                logger.Trace($"SQL Server setup file downloaded. IsCancelled: {e.Cancelled}");
            }

            if (e.Cancelled == false && e.Error == null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Extracting setup file."));

                var extractPath = FileHelper.GetUnusedFolderName(System.IO.Path.GetTempPath(), "SqlExtract");

                var setupExePath = System.IO.Path.Combine(System.IO.Path.Combine(extractPath, "SQLEX"), "Setup.exe");

                logger.Trace($"SQL Server setup file downloaded. ExtractPath: {extractPath}, SetupExePath: {setupExePath}");

                try
                {
                    ZipFile.ExtractToDirectory(e.UserState.ToString(), extractPath);
                    logger.Trace($"SQL Server setup file extracted. ExtractPath: {extractPath}");

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "SQL Server setup file extracted."));
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = $"SQL Server setup file extract error."));
                    logger.Error(ex, $"SQL Server setup file extract error.");
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Creating config file."));

                var uri = new Uri("/Resources/Setup/Database/Config.ini", UriKind.Relative);

                StreamResourceInfo resourceStream = null;
                try
                {
                    resourceStream = Application.GetResourceStream(uri);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Config file resource error."));
                    logger.Error(ex, $"Config file resource error.");
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
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Config file saved."));
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Config file write error."));
                    logger.Error(ex, $"Config file write error.");
                    return;
                }

                var options = $" /ConfigurationFile=\"{configFilePath}\"";

                logger.Trace($"Process start options: {options}, FileName: {setupExePath}");

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Setup will begin shortly. Do not close black window. It will take long time to install. Please be patient."));

                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = setupExePath,
                    Arguments = options,
                    WindowStyle = ProcessWindowStyle.Minimized,
                };
                process.StartInfo = processStartInfo;
                try
                {
                    process.Start();
                    process.WaitForExit();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Setup completed successfully."));
                    Main.BtnNext.IsEnabled = true;
                    NextParam = "WizardCheckDatabase";
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status.Text = "Process error."));
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
    }


}
