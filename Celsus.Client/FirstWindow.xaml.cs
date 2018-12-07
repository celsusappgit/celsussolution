using Celsus.Client.Controls.Common;
using Celsus.Client.Controls.Licensing;
using Celsus.Client.Controls.Management;
using Celsus.Client.Controls.Management.Sources;
using Celsus.Client.Controls.Setup;
using Celsus.Client.Controls.Setup.Database;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
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
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace Celsus.Client
{

    public partial class FirstWindow : Window
    {
        private FirstWindowModel model = new FirstWindowModel();
        public FirstWindow()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            InitializeComponent();

            //LicenseHelper.Instance.Init();

            DataContext = FirstWindowModel.Instance;
            //FirstWindowModel.Instance.Init();

#if DEBUG
            Width = 1280;
            Height = 1024;
#endif

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LicenseHelper.Instance.Reset(out int st);
        }



        //private LicenseHelper licenseHelper = new LicenseHelper();

        //private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    licenseHelper.Init(@"Resources\Lex");

        //    var licenseHelperInit = licenseHelper.Init(@"Resources\Lex");
        //    if (licenseHelperInit == false)
        //    {
        //        RadWindow.Alert("Licensing error.", ErrorClosed);
        //        return;
        //    }
        //    if (licenseHelper.CheckLicense(out bool hasError, out int statusCode) == false)
        //    {
        //        if (hasError)
        //        {
        //            RadWindow.Alert("Licensing error.");
        //            return;
        //        }
        //        else
        //        {
        //            if (licenseHelper.CheckTrialLicense(out bool hasErrorInTrial, out int statusCodeTrial, out TimeSpan? expireDate) == false)
        //            {
        //                if (hasErrorInTrial)
        //                {
        //                    RadWindow.Alert("Licensing error.");
        //                    return;
        //                }
        //                else
        //                {
        //                    RadWindow.Alert("Licensing error.");
        //                    return;
        //                }
        //            }
        //            else
        //            {
        //                // OK
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // OK
        //    }
        //}

        //private void ErrorClosed(object sender, WindowClosedEventArgs e)
        //{
        //    App.Current.Shutdown();
        //}
    }

    public class FirstWindowModel : BaseModel<FirstWindowModel>, MustInit
    {
        #region Properties
        public LicenseHelper LicenseHelper { get { return LicenseHelper.Instance; } }

        public SetupHelper SetupHelper { get { return SetupHelper.Instance; } }

        public DatabaseHelper DatabaseHelper { get { return DatabaseHelper.Instance; } }

        public IndexerHelper IndexerHelper { get { return IndexerHelper.Instance; } }

        ObservableCollection<TabViewModel> tabItems;
        public ObservableCollection<TabViewModel> TabItems
        {
            get
            {
                return tabItems;
            }
            set
            {
                if (Equals(value, tabItems)) return;
                tabItems = value;
                NotifyPropertyChanged(() => TabItems);
            }
        }

        TabViewModel previousSelectedTabItem = null;

        TabViewModel selectedTabItem;
        public TabViewModel SelectedTabItem
        {
            get
            {
                return selectedTabItem;
            }
            set
            {
                if (Equals(value, selectedTabItem)) return;
                if (selectedTabItem != null)
                {
                    previousSelectedTabItem = selectedTabItem;
                }
                selectedTabItem = value;
                NotifyPropertyChanged(() => SelectedTabItem);
            }
        }

        public string Title
        {
            get
            {
                string AppVersion = string.Empty;
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    try
                    {
                        AppVersion = " | " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    }
                    catch (InvalidDeploymentException)
                    {
                        AppVersion = " | " + "Local Run";
                    }
                }
                return $"Celsus{AppVersion}";
            }
        }

        #endregion

        ICommand searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                    searchCommand = new RelayCommand(param => Search(param), param => { return true; });
                return searchCommand;
            }
        }

        private void Search(object obj)
        {
            OpenTabItem(typeof(SearchControl));
        }

        ICommand setupCommand;
        public ICommand SetupCommand
        {
            get
            {
                if (setupCommand == null)
                    setupCommand = new RelayCommand(param => Setup(param), param => { return true; });
                return setupCommand;
            }
        }

        private void Setup(object obj)
        {
            OpenTabItem(typeof(SetupMainControl));
        }

        ICommand systemMonitorCommand;
        public ICommand SystemMonitorCommand
        {
            get
            {
                if (systemMonitorCommand == null)
                    systemMonitorCommand = new RelayCommand(param => SystemMonitor(param), param => { return true; });
                return systemMonitorCommand;
            }
        }



        ICommand helpCommand;
        public ICommand HelpCommand
        {
            get
            {
                if (helpCommand == null)
                    helpCommand = new RelayCommand(param => Help(param), param => { return true; });
                return helpCommand;
            }
        }

        private void Help(object param)
        {
            string target = "https://celsus.gitbook.io/project/";
            if (SelectedTabItem != null && SelectedTabItem.Content != null)
            {
                var helpKeywords = SelectedTabItem.Content.GetType().GetAttributeValue((HelpAttribute attribute) => attribute.HelpKeywords);
                var url = SelectedTabItem.Content.GetType().GetAttributeValue((HelpAttribute attribute) => attribute.Url);
                if (string.IsNullOrWhiteSpace(url) == false)
                {
                    target = url;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(helpKeywords) == false)
                    {
                        target = target + "?q=" + helpKeywords;
                    }
                }
            }

            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259) ;
            }
            catch (Exception other)
            {
            }

        }

        private void SystemMonitor(object obj)
        {
            OpenTabItem(typeof(SystemMonitorControl));
        }

        ICommand sourcesCommand;
        public ICommand SourcesCommand
        {
            get
            {
                if (sourcesCommand == null)
                    sourcesCommand = new RelayCommand(param => Sources(param), param => { return true; });
                return sourcesCommand;
            }
        }

        public CustomAnalitycsMonitor AnalitycsMonitor { get; set; }

        private void Sources(object obj)
        {
            OpenTabItem(typeof(SourcesManagementControl));
        }

        public FirstWindowModel()
        {
            tabItems = new ObservableCollection<TabViewModel>();


        }
        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.Language) == false)
            {
                if (Properties.Settings.Default.Language.Length == 2)
                {
                    System.Globalization.CultureInfo cultureInfo = null;
                    try
                    {
                        cultureInfo = new System.Globalization.CultureInfo(Properties.Settings.Default.Language);
                        TranslationSource.Instance.CurrentCulture = cultureInfo;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                TranslationSource.Instance.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture;
            }

            if (SettingsHelper.Instance.Status == SettingsHelperStatusEnum.DontHaveSettingsFile)
            {
                OpenTabItem(typeof(SetupMainControl));
            }
            else if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.ComputerClockCracked
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.ComputerClockError
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.GotError
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseCracked
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseExpired
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseGracePeriodOver
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseSuspended
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.TrialLicenseExpired
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.WrongProductId
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.WrongProductKey)
            {
                OpenTabItem(typeof(SetupMainControl));
            }
            //if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense)
            //{
            //    OpenTabItem(typeof(LicenseCheckControl));
            //}
            //else
            //{
            //    var t = DatabaseHelper.Instance.Status;
            //}

            //OpenTabItem(typeof(IndexerSetupMainControl));

            AnalitycsMonitor = new CustomAnalitycsMonitor();

            AnalitycsMonitor.StartSession();

        }

        internal void OpenTabItem(Type type)
        {
            var oldItem = TabItems.FirstOrDefault(x => x.Content.GetType() == type);
            if (oldItem != null)
            {
                SelectedTabItem = oldItem;
            }
            else
            {
                var control = (UserControl)Activator.CreateInstance(type);
                var headerTextBlock = new TextBlock();
                headerTextBlock.SetBinding(TextBlock.TextProperty, new Binding() { Path = new PropertyPath("[" + type.Name + "]"), Source = TranslationSource.Instance });
                var newTabItem = new TabViewModel() { Header = headerTextBlock, Content = control };
                TabItems.Add(newTabItem);
                SelectedTabItem = newTabItem;
            }
        }

        internal void OpenTabItem(object control)
        {
            if (typeof(IItemEditControl).IsAssignableFrom(control.GetType()))
            {
                var itemEditControl = control as IItemEditControl;
                if (itemEditControl.ItemId.HasValue)
                {
                    var oldItem = TabItems.FirstOrDefault(x =>
                                                          x.Content.GetType() == control.GetType() &&
                                                          typeof(IItemEditControl).IsAssignableFrom(x.Content.GetType()) &&
                                                          (x.Content as IItemEditControl).ItemId == itemEditControl.ItemId);
                    if (oldItem != null)
                    {
                        SelectedTabItem = oldItem;
                        return;
                    }

                }
                else if (itemEditControl.ItemId.HasValue == false)
                {
                    var oldItem = TabItems.FirstOrDefault(x => x.Content.GetType() == control.GetType() && typeof(IItemEditControl).IsAssignableFrom(x.Content.GetType()) && (control as IItemEditControl).ItemId == null);
                    if (oldItem != null)
                    {
                        SelectedTabItem = oldItem;
                        return;
                    }
                }
            }
            var headerTextBlock = new TextBlock();
            headerTextBlock.SetBinding(TextBlock.TextProperty, new Binding() { Path = new PropertyPath("[" + control.GetType().Name + "]"), Source = TranslationSource.Instance });
            var newTabItem = new TabViewModel() { Header = headerTextBlock, Content = control };
            TabItems.Add(newTabItem);
            SelectedTabItem = newTabItem;
        }


        internal void CloseTabItem(TabViewModel tabViewModel)
        {
            var oldItem = TabItems.FirstOrDefault(x => x == tabViewModel);
            if (oldItem != null)
            {
                var index = tabItems.IndexOf(oldItem);

                if (previousSelectedTabItem != null)
                    SelectedTabItem = previousSelectedTabItem;
                tabItems.RemoveAt(index);
                oldItem.Content = null;
                oldItem = null;
            }
        }

        internal void CloseTabItem(object model)
        {
            TabViewModel oldItem = null;
            if (typeof(IItemEditControl).IsAssignableFrom(model.GetType()))
            {
                var itemEditControl = model as IItemEditControl;

                if (itemEditControl.ItemId.HasValue)
                {
                    oldItem = TabItems.FirstOrDefault(x =>
                                                          x.Content.GetType() == model.GetType() &&
                                                          typeof(IItemEditControl).IsAssignableFrom(x.Content.GetType()) &&
                                                          (x.Content as IItemEditControl).ItemId == itemEditControl.ItemId);


                }
                else if (itemEditControl.ItemId.HasValue == false)
                {
                    oldItem = TabItems.FirstOrDefault(x =>
                                                           x.Content.GetType() == model.GetType() &&
                                                           typeof(IItemEditControl).IsAssignableFrom(x.Content.GetType())
                                                           && (model as IItemEditControl).ItemId == null);

                }
            }
            else
            {

            }
            oldItem = TabItems.FirstOrDefault(x => (x.Content as UserControl).DataContext == model);

            if (oldItem != null)
            {
                var index = tabItems.IndexOf(oldItem);

                if (previousSelectedTabItem != null)
                    SelectedTabItem = previousSelectedTabItem;
                tabItems.RemoveAt(index);
                oldItem.Content = null;
                oldItem = null;
            }

        }
    }
}