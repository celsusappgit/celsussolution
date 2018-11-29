using Celsus.Client.Wpf.Types;
using Celsus.Types.NonDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    /// <summary>
    /// Interaction logic for InstallTesseract.xaml
    /// </summary>
    [Description("SingleInstance")]
    public partial class InstallTesseract : UserControl
    {
        private bool initDone;


        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Lazy<InstallTesseract> _lazyInstance = new Lazy<InstallTesseract>(() => new InstallTesseract());
        public InstallTesseract()
        {
            InitializeComponent();
         
        }
        public static InstallTesseract Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        public void Init()
        {
            if (initDone)
            {
                return;
            }
            //Check(SettingName.TesseractPath);
            //Check(SettingName.ImageMagickPath);
            //Check(SettingName.XPdfToolsPath);
            initDone = true;
        }
        
        private void Check(SettingName settingName)
        {
            var componentName = settingName.ToString().Replace("Path", "");

            (FindName($"Installed{componentName}") as FrameworkElement).Visibility = Visibility.Collapsed;
            (FindName($"NotInstalled{componentName}") as FrameworkElement).Visibility = Visibility.Visible;
            (FindName($"RadProgressBarDownload{componentName}") as FrameworkElement).Visibility = Visibility.Hidden;

            bool isOk = false;
            if (settingName== SettingName.ImageMagickPath)
            {
                isOk = SetupManager.Instance.IsImageMagickInstalled;
            }

            //if (SetupManager.CheckOcrComponent(settingName))
            {
                (FindName($"Installed{componentName}") as FrameworkElement).Visibility = Visibility.Visible;
                (FindName($"NotInstalled{componentName}") as FrameworkElement).Visibility = Visibility.Collapsed;
                (FindName($"BtnInstall{componentName}") as RadButton).Content = "Reinstall";
            }

            (FindName($"BtnInstall{componentName}") as RadButton).IsEnabled = true;
        }
        private void Setup(SettingName settingName, string url)
        {

            var componentName = settingName.ToString().Replace("Path", "");

            logger.Trace($"Install {componentName} started.");

            var downloadPath = FileHelper.GetUnusedFileName(Path.GetTempPath(), $"{componentName}.7z");

            logger.Trace($"{componentName} setup file path is {downloadPath}");

            var urlTesseract = "https://drive.google.com/uc?export=download&id=1tVu45QDZJK_x2jmpHJP9w2zD3DodhJ23";

            urlTesseract = url;

            (FindName($"RadProgressBarDownload{componentName}") as FrameworkElement).Visibility = Visibility.Visible;

            (FindName($"StatusInstall{componentName}") as RadMaskedTextInput).Value = "Downloading";

            (FindName($"BtnInstall{componentName}") as RadButton).IsEnabled = false;

            try
            {
                logger.Trace($"{componentName} setup file will be downloaded from {urlTesseract}");

                (FindName($"RadProgressBarDownload{componentName}") as RadProgressBar).Visibility = Visibility.Visible;

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressChanged;
                    webClient.DownloadFileCompleted += DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(urlTesseract), downloadPath, new DownloadFileAsyncState() { DownloadPath = downloadPath, SettingName = settingName });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when downloading {componentName} setup file.");

                (FindName($"BtnInstall{componentName}") as RadButton).IsEnabled = true;
            }
        }
        private void SetupTesseract(object sender, RoutedEventArgs e)
        {
            Setup(SettingName.TesseractPath, DownloadUris.Tesseract);
        }
        private void SetupImageMagick(object sender, RoutedEventArgs e)
        {
            Setup(SettingName.ImageMagickPath, DownloadUris.ImageMagick);
        }
        private void SetupXPdfTools(object sender, RoutedEventArgs e)
        {
            Setup(SettingName.XPdfToolsPath, DownloadUris.XPdfTools);
        }

        #region Download

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var settingName = (e.UserState as DownloadFileAsyncState).SettingName;
            var componentName = settingName.ToString().Replace("Path", "");
            (FindName($"RadProgressBarDownload{componentName}") as RadProgressBar).Value = e.ProgressPercentage;
            System.Diagnostics.Debug.WriteLine(componentName + " " + e.ProgressPercentage);
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

                (FindName($"StatusInstall{componentName}") as RadMaskedTextInput).Value = "Error occured.";
                (FindName($"BtnInstall{componentName}") as RadButton).IsEnabled = true;
            }
            else
            {
                logger.Trace($"{componentName} setup file downloaded. IsCancelled: {e.Cancelled}");
            }

            if (e.Cancelled == false && e.Error == null)
            {
                ExtractInBackground(state);
            }
        }

        #endregion

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

            var extractPath = FileHelper.GetUnusedFolderName(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), componentName);

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
                (FindName($"BtnInstall{componentName}") as RadButton).IsEnabled = true;
            }

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var result = e.UserState as BackgroundWorkerState;
            var componentName = result.SettingName.ToString().Replace("Path", "");
            (FindName($"StatusInstall{componentName}") as RadMaskedTextInput).Value = "Extracting";
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (e.Result as BackgroundWorkerResult);
            var extractPath = result.ExtractPath;
            var settingName = result.SettingName;
            var componentName = result.SettingName.ToString().Replace("Path", "");

            (FindName($"StatusInstall{componentName}") as RadMaskedTextInput).Value = "Done";

            SettingsManager.Instance.AddOrUpdateSetting(settingName, extractPath);

            //Check(settingName);

            (App.Current.MainWindow as MainWindow).CheckOCR();

        }



        #endregion

        private void SetupService(object sender, RoutedEventArgs e)
        {

        }
    }

    internal class BackgroundWorkerState
    {
        public BackgroundWorkerState()
        {
        }

        public string DownloadPath { get; set; }
        public SettingName SettingName { get; set; }
    }

    internal class BackgroundWorkerResult
    {
        public BackgroundWorkerResult()
        {
        }

        public string ExtractPath { get; set; }
        public SettingName SettingName { get; set; }
    }

    public class DownloadFileAsyncState
    {
        public DownloadFileAsyncState()
        {
        }

        public string DownloadPath { get; set; }
        public SettingName SettingName { get; set; }
    }
}
