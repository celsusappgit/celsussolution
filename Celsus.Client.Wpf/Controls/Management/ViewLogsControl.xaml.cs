using Newtonsoft.Json;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Celsus.Client.Wpf.Controls.Management
{
    /// <summary>
    /// Interaction logic for ViewLogsControl.xaml
    /// </summary>
    public partial class ViewLogsControl : UserControl
    {
        private string[] allLogFiles = null;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public ViewLogsControl()
        {
            InitializeComponent();
            Loaded += ViewLogsControl_Loaded;
        }

        private void ViewLogsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("FileTarget");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            string fileName = fileTarget.FileName.Render(new LogEventInfo { TimeStamp = DateTime.Now });
            if (string.IsNullOrWhiteSpace(fileName) == false)
            {
                string dirPath = "";
                try
                {
                    dirPath = System.IO.Path.GetDirectoryName(fileName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured GetDirectoryName for log files.");
                    (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Error occured accesing directory for log files.", ShowDuration = 3000 });
                    return;
                }
                try
                {
                    allLogFiles = Directory.GetFiles(dirPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error occured GetFiles for log files.");
                    (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Error occured accesing log files.", ShowDuration = 3000 });
                    return;
                }
                var sortedFileNames = allLogFiles.Select(x => System.IO.Path.GetFileNameWithoutExtension(x)).OrderByDescending(x => x);
                Files.ItemsSource = sortedFileNames;
            }
            else
            {
                (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Cannot get file name for log files.", ShowDuration = 3000 });
            }

        }

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RadGridViewLogs.ItemsSource = (null);

            if (Files.SelectedItem != null)
            {
                var selectedLogFile = allLogFiles.SingleOrDefault(x => x.Contains(Files.SelectedItem.ToString()));
                if (selectedLogFile != null && File.Exists(selectedLogFile))
                {
                    string[] lines = null;
                    try
                    {
                        lines = File.ReadAllLines(selectedLogFile);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error occured reading log file.");
                        (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Error occured reading log file.", ShowDuration = 3000 });
                        return;
                    }
                    if (lines == null || lines.Count() == 0)
                    {
                        (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "There is no information in log file.", ShowDuration = 3000 });
                        return;
                    }
                    var linesString = "[" + string.Join(",", lines) + "]";
                    object logItems = null;
                    try
                    {
                        logItems = JsonConvert.DeserializeObject(linesString);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error occured analyzing log file.");
                        (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Error occured analyzing log file.", ShowDuration = 3000 });
                        return;
                    }
                    RadGridViewLogs.ItemsSource = logItems;
                }
                else
                {
                    (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Error", Content = "Please select a vaild log file.", ShowDuration = 3000 });
                }
            }
        }
    }
}
