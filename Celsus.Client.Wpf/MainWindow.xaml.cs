using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Wpf.Types;
using DbUp;
using Microsoft.SqlServer.Dac;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using Telerik.Windows.Controls;

namespace Celsus.Client.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RadDesktopAlertManager radDesktopAlertManager = null;

        private LicenseHelper licenseHelper = LicenseHelper.Instance;
        List<object> _previousControls = new List<object>();
        private List<string> setupItems = new List<string>();

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            InitializeComponent();
            radDesktopAlertManager = new RadDesktopAlertManager(AlertScreenPosition.BottomCenter, new Point(0, 200), 5);

            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;
            Loaded += MainWindow_Loaded;
            //var t = System.IO.Directory.GetFiles(@"C:\Work", "*", System.IO.SearchOption.AllDirectories);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //var licenseHelperInit = licenseHelper.Init(@"Resources\Lex", out bool hasErrorInit, out int statusCodeInit);
            //if (licenseHelperInit == false)
            //{
            //    return;
            //}
            //if (licenseHelper.CheckLicense(out int statusCode) == false)
            //{
            //    if (licenseHelper.CheckTrialLicense(out int statusCodeTrial, out TimeSpan? expireDate) == false)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        // OK
            //    }
            //}
            //else
            //{
            //    // OK
            //}


            AlertDatabaseSetup.Visibility = Visibility.Hidden;
            AlertOCRSetup.Visibility = Visibility.Hidden;
            AlertLicensing.Visibility = Visibility.Hidden;

            SettingsManager.Instance.LoadSettings();

            //await CheckDatabaseAsync();

            //CheckOCR();

            //CheckService();

            //CheckSerial();

            LoadContent("Topology");


        }

        private void CheckService()
        {
            if (SetupManager.CheckService() == null)
            {
                AlertServiceSetup.Visibility = Visibility.Visible;
            }
            else
            {
                AlertServiceSetup.Visibility = Visibility.Hidden;
            }
        }

        //public void CheckSerial()
        //{
        //    AlertLicensing.Visibility = Visibility.Hidden;

        //    var result = SetupManager.Instance.CheckSerial(SettingsManager.Instance.GetSetting<string>(SettingName.Serial), out Celsus.Types.NonDatabase.LicenseData licenseInformation);
        //    if (result == "LicenseOk")
        //    {
        //        AlertLicensing.Visibility = Visibility.Hidden;

        //        if (setupItems.Contains("License") == false)
        //            setupItems.Add("License");
        //    }
        //    else
        //    {
        //        AlertLicensing.Visibility = Visibility.Visible;

        //        if (setupItems.Contains("License") == false)
        //            setupItems.Remove("License");
        //    }

        //    CheckSetup();
        //}

        public async Task CheckDatabaseAsync()
        {
            AlertDatabaseSetup.Visibility = Visibility.Hidden;
            if (SetupManager.Instance.CanReachDatabase)
            {
                AlertDatabaseSetup.Visibility = Visibility.Hidden;
                if (setupItems.Contains("Database") == false)
                    setupItems.Add("Database");
            }
            else
            {
                if (setupItems.Contains("Database") == false)
                    setupItems.Remove("Database");
                AlertDatabaseSetup.Visibility = Visibility.Visible;

            }
            CheckSetup();
        }

        private void CheckSetup()
        {
            if (setupItems.Count == 3)
            {
                TriDashboard.IsEnabled = true;
                TriSearch.IsEnabled = true;
                TriSourceManagement.IsEnabled = true;
            }
            else
            {
                TriDashboard.IsEnabled = false;
                TriSearch.IsEnabled = false;
                TriSourceManagement.IsEnabled = false;
            }
        }

        public void CheckOCR()
        {
            if (SetupManager.Instance.IsOCRInstalled == false)
            {
                AlertOCRSetup.Visibility = Visibility.Visible;
                if (setupItems.Contains("OCR") == false)
                    setupItems.Remove("OCR");
            }
            else
            {
                AlertOCRSetup.Visibility = Visibility.Hidden;
                if (setupItems.Contains("OCR") == false)
                    setupItems.Add("OCR");

            }
            CheckSetup();
        }

        private void Main()
        {
            //DacServices ds = new DacServices(@"Data Source=localhost;Initial Catalog=Celsus;Integrated Security=true");
            //using (DacPackage dp = DacPackage.Load(@"C:\temp\mydb.dacpac"))
            //{
            //    ds.Deploy(dp, @"DATABASENAME", upgradeExisting: false, options: null, cancellationToken: null);
            //}
        }


        private void RadTreeViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var radTreeView = sender as RadTreeView;
            if (radTreeView.SelectedItem != null)
            {
                var radTreeViewItem = radTreeView.SelectedItem as RadTreeViewItem;
                if (radTreeViewItem.Tag != null)
                {
                    LoadContent(radTreeViewItem.Tag.ToString());
                }
            }
        }

        internal void Back()
        {
            if (_previousControls.Count > 0)
            {
                ContentPresenter.Content = _previousControls.Last();
                _previousControls.RemoveAt(_previousControls.Count - 1);
            }
        }

        internal void LoadContent(string typeName)
        {
            var targetType = Assembly.GetExecutingAssembly().GetTypes().AsEnumerable().FirstOrDefault(x => x.Name.Contains(typeName));

            UserControl instance = null;

            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(targetType, typeof(DescriptionAttribute));
            if (descriptionAttribute != null)
            {
                if (descriptionAttribute.Description.IndexOf("SingleInstance") >= 0)
                {
                    var staticMethodInfo = targetType.GetProperty("Instance");
                    if (staticMethodInfo == null)
                    {
                        throw new Exception("ShowContentAsTabItemSingleInstance02");
                    }
                    instance = (UserControl)staticMethodInfo.GetValue(null, null);
                    (instance as dynamic).Init();
                }
            }
            if (instance == null)
            {
                instance = (UserControl)Activator.CreateInstance(targetType);
            }


            if (ContentPresenter.Content != null)
            {
                ContentPresenter.Content = null;
            }
            ContentPresenter.Content = instance;
        }

        internal void LoadContentCanGoToBack(string typeName)
        {
            if (ContentPresenter.Content != null)
            {
                _previousControls.Add(ContentPresenter.Content);
            }
            LoadContent(typeName);
        }

        internal void LoadContentCanGoToBack(object objectToLoad)
        {
            if (ContentPresenter.Content != null)
            {
                _previousControls.Add(ContentPresenter.Content);
            }
            ContentPresenter.Content = objectToLoad;
        }

        public void ShowAlert(RadDesktopAlert radDesktopAlert)
        {
            radDesktopAlert.Background = Brushes.Black;
            radDesktopAlert.Foreground = Brushes.White;
            radDesktopAlertManager.ShowAlert(radDesktopAlert);
        }

        internal void ShowAlertError(string content)
        {
            var radDesktopAlert = new RadDesktopAlert
            {
                Header = "Error",
                Content = content,
                Width = 400,
                ShowDuration = 3000
            };
            radDesktopAlertManager.ShowAlert(radDesktopAlert);
        }

        internal void ShowAlertError(string content, string detail)
        {
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock() { Text = content });
            sp.Children.Add(new TextBlock() { Text = detail });
            var radDesktopAlert = new RadDesktopAlert
            {
                Header = "Error",
                Content = sp,
                Width = 400,
                ShowDuration = 3000
            };
            radDesktopAlertManager.ShowAlert(radDesktopAlert);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlertPositioning();
        }
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            AlertPositioning();
        }

        private void AlertPositioning()
        {

            if (radDesktopAlertManager != null)
            {
                radDesktopAlertManager.CloseAllAlerts();
            }


            if (WindowState != System.Windows.WindowState.Maximized)
            {
                var workingArea = System.Windows.SystemParameters.WorkArea;
                radDesktopAlertManager =
                    new RadDesktopAlertManager(
                        AlertScreenPosition.BottomLeft,
                        new Point(
                            Left + ActualWidth / 2 - 200,
                            -(workingArea.Height - Top - Height + 10)
                            ),
                        5
                        );
            }
            else
            {
                var rect = SystemParameters.WorkArea;

                radDesktopAlertManager =
                    new RadDesktopAlertManager(
                        AlertScreenPosition.BottomCenter, 5);
            }
        }


    }
}
