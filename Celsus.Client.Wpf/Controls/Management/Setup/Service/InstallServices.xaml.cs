using Celsus.Client.Wpf.Types;
using Celsus.Types.NonDatabase;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
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
using System.Windows.Shapes;

namespace Celsus.Client.Wpf.Controls.Management.Setup.Service
{
    /// <summary>
    /// Interaction logic for InstallServices.xaml
    /// </summary>
    [Description("SingleInstance")]
    public partial class InstallServices : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool initDone;
        public InstallServices()
        {
            InitializeComponent();

            MethodCallTarget target = new MethodCallTarget("MyTarget", (logEvent, parms) => InstallServicesLoggerClass.Logs.Add(new LogItem() { Level = logEvent.Level.ToString(), Message = logEvent.Message }));
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            logger.Info("log message");


            LbLogs.ItemsSource = InstallServicesLoggerClass.Logs;
        }

        private void InstallServices_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private static readonly Lazy<InstallServices> _lazyInstance = new Lazy<InstallServices>(() => new InstallServices());

        public static InstallServices Instance
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

            CheckService();

            initDone = true;
        }

        public void CheckService()
        {
            SpServiceOk.Visibility = Visibility.Collapsed;
            SpServiceOld.Visibility = Visibility.Collapsed;
            SpServiceNo.Visibility = Visibility.Collapsed;

            var checkResult = SetupManager.CheckService();

            logger.Trace("Service is running");

            if (checkResult != null)
            {
                logger.Info("Service is running");
                if (CheckVersion())
                {

                    SpServiceOk.Visibility = Visibility.Visible;
                }
                else
                {
                    SpServiceOld.Visibility = Visibility.Visible;
                }

            }
            else if (checkResult == null)
            {
                SpServiceNo.Visibility = Visibility.Visible;
            }
        }

        private void Run_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Install(object sender, MouseButtonEventArgs e)
        {

        }

        private bool CheckVersion()
        {
            string zipFolder = UnzipWorkerZip(FileHelper.GetUnusedFolderName(System.IO.Path.GetTempPath(), $"WorkerUnzipped"));
            var workerPath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            var compareResult = FileHelper.CompareFolders(zipFolder, workerPath);
            return compareResult;
        }

        private static string UnzipWorkerZip(string targetDir)
        {
            byte[] zipData = null;
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (MemoryStream _mem = new MemoryStream())
            {
                _assembly.GetManifestResourceStream("Celsus.Client.Wpf.Resources.Worker.worker.zip").CopyTo(_mem);
                zipData = _mem.ToArray();
            }
            var zipFile = FileHelper.GetUnusedFileName(System.IO.Path.GetTempPath(), $"worker.zip");
            var zipFolder = targetDir;
            File.WriteAllBytes(zipFile, zipData);
            ZipFile.ExtractToDirectory(zipFile, zipFolder);
            return zipFolder;
        }

        private void Upgrade(object sender, MouseButtonEventArgs e)
        {
            UninstallService();
            DeleteFolder();
            UnzipWorkerZip(System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker"));
            InstallService();
        }

        private void InstallService()
        {
            //var workerPath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            //Process process = new Process();
            //ProcessStartInfo processStartInfo = new ProcessStartInfo
            //{
            //    FileName = $"{System.IO.Path.Combine(workerPath, "Celsus.Worker.exe")}",
            //    Arguments = $"install –autostart",
            //    RedirectStandardOutput = true,
            //    UseShellExecute = false
            //};
            //process.StartInfo = processStartInfo;
            //try
            //{
            //    process.Start();
            //    process.WaitForExit();
            //    while (!process.StandardOutput.EndOfStream)
            //    {
            //        string line = process.StandardOutput.ReadLine();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex, $"Process error.");
            //    return;
            //}

            List<string> errorDatas = new List<string>();
            List<string> outputDatas = new List<string>();

            var workerPath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = $"{System.IO.Path.Combine(workerPath, "Celsus.Worker.exe")}",
                Arguments = $"install –autostart",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorDatas.Add(e.Data);
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputDatas.Add(e.Data);
                }
            };
            process.EnableRaisingEvents = true;
            process.StartInfo = processStartInfo;
            var cmd = processStartInfo.FileName + " " + processStartInfo.Arguments;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Process error.");
            }

            if (errorDatas.Count > 0)
            {
                Debug.WriteLine("TTTT " + string.Join(",", errorDatas.ToArray()));
                return;
            }
            if (outputDatas.Count > 0)
            {
                Debug.WriteLine("OOOO " + string.Join(",", outputDatas.ToArray()));
            }
        }

        private void DeleteFolder()
        {
            var workerPath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Worker");
            System.IO.Directory.Delete(workerPath, true);
        }

        private void UninstallServiceViaServiceInstaller()
        {
            ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
            InstallContext Context = new InstallContext(@"c:\AkisLog\ulog.txt", null);
            ServiceInstallerObj.Context = Context;
            ServiceInstallerObj.DisplayName = "CelsusService";
            ServiceInstallerObj.Uninstall(null);

            //ServiceInstaller si = new ServiceInstaller();
            //ServiceProcessInstaller spi = new ServiceProcessInstaller();
            //si.Parent = spi;
            //si.ServiceName = "CelsusService";

            //si.Context = new InstallContext(Assembly.GetExecutingAssembly().GetName().Name + ".uninstall.log", null);
            //si.Uninstall(null);
        }
        private void UninstallService()
        {
            var t = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string zipFolder = UnzipWorkerZip(FileHelper.GetUnusedFolderName(System.IO.Path.GetTempPath(), $"WorkerUnzipped"));
            var c = $"installutil /u {System.IO.Path.Combine(zipFolder, "Celsus.Worker.exe")}";

            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = $"{System.IO.Path.Combine(t, "installutil.exe")}",
                Arguments = $"/u {System.IO.Path.Combine(zipFolder, "Celsus.Worker.exe")}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            process.StartInfo = processStartInfo;
            try
            {
                process.Start();
                process.WaitForExit();
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Process error.");
                return;
            }
        }
    }

    public static class InstallServicesLoggerClass
    {
        public static ObservableCollection<LogItem> Logs = new ObservableCollection<LogItem>();
        public static void LogMethod(string level, string message)
        {
            //if (exception != null)
            {
                //Logs.Insert(0, new LogItem() { Level = level, Message = message, Exception = exception });
            }
            //else
            {
                Logs.Insert(0, new LogItem() { Level = level, Message = message });
            }
        }
    }
}
