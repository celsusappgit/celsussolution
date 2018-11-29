using Celsus.Client.Types;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;

namespace Celsus.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var logFilePath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), $"Celsus.Client.{DateTime.Now.ToString("yyyy MMMM dd HH mm")}.txt");

            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
            };

            config.AddRuleForAllLevels(logfile);

            LogManager.Configuration = config;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                FirstWindowModel.Instance.AnalitycsMonitor.EndSession();
            }
            catch (Exception)
            {
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                FirstWindowModel.Instance.AnalitycsMonitor.TrackError(sender.GetType().ToString(), e.Exception);
            }
            catch (Exception)
            {
            }
        }
    }
}
