using Celsus.Client.BootStrapper.Controls;
using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace Celsus.Client.BootStrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowModel model = new MainWindowModel();
        public MainWindow()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            InitializeComponent();
            DataContext = model;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => model.Init()));
        }
    }

    public class MainWindowModel : BaseModel
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainWindowModel()
        {
            ActivateTrialLicenseVisibility = Visibility.Collapsed;
            EnterLicenseKeyVisibility = Visibility.Collapsed;
            PurchaseNewLicenseVisibility = Visibility.Collapsed;
            GetHelpVisibility = Visibility.Collapsed;
            SendErrorLogVisibility = Visibility.Collapsed;
            EntendYourLicenseVisibility = Visibility.Collapsed;
            ContactSupportVisibility = Visibility.Collapsed;
            ResetLicenseRecordVisibility = Visibility.Collapsed;

        }

        private LicenseHelper licenseHelper = LicenseHelper.Instance;
        string status;
        public string Status
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
            }
        }

        string updateStatus;
        public string UpdateStatus
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
            }
        }

        Visibility activateTrialLicenseVisibility;
        public Visibility ActivateTrialLicenseVisibility
        {
            get
            {
                return activateTrialLicenseVisibility;
            }
            set
            {
                if (Equals(value, activateTrialLicenseVisibility)) return;
                activateTrialLicenseVisibility = value;
                NotifyPropertyChanged(() => ActivateTrialLicenseVisibility);
            }
        }

        Visibility enterLicenseKeyVisibility;
        public Visibility EnterLicenseKeyVisibility
        {
            get
            {
                return enterLicenseKeyVisibility;
            }
            set
            {
                if (Equals(value, enterLicenseKeyVisibility)) return;
                enterLicenseKeyVisibility = value;
                NotifyPropertyChanged(() => EnterLicenseKeyVisibility);
            }
        }

        Visibility purchaseNewLicenseVisibility;
        public Visibility PurchaseNewLicenseVisibility
        {
            get
            {
                return purchaseNewLicenseVisibility;
            }
            set
            {
                if (Equals(value, purchaseNewLicenseVisibility)) return;
                purchaseNewLicenseVisibility = value;
                NotifyPropertyChanged(() => PurchaseNewLicenseVisibility);
            }
        }

        Visibility getHelpVisibility;
        public Visibility GetHelpVisibility
        {
            get
            {
                return getHelpVisibility;
            }
            set
            {
                if (Equals(value, getHelpVisibility)) return;
                getHelpVisibility = value;
                NotifyPropertyChanged(() => GetHelpVisibility);
            }
        }

        Visibility sendErrorLogVisibility;
        public Visibility SendErrorLogVisibility
        {
            get
            {
                return sendErrorLogVisibility;
            }
            set
            {
                if (Equals(value, sendErrorLogVisibility)) return;
                sendErrorLogVisibility = value;
                NotifyPropertyChanged(() => SendErrorLogVisibility);
            }
        }

        Visibility entendYourLicenseVisibility;
        public Visibility EntendYourLicenseVisibility
        {
            get
            {
                return entendYourLicenseVisibility;
            }
            set
            {
                if (Equals(value, entendYourLicenseVisibility)) return;
                entendYourLicenseVisibility = value;
                NotifyPropertyChanged(() => EntendYourLicenseVisibility);
            }
        }

        Visibility contactSupportVisibility;
        public Visibility ContactSupportVisibility
        {
            get
            {
                return contactSupportVisibility;
            }
            set
            {
                if (Equals(value, contactSupportVisibility)) return;
                contactSupportVisibility = value;
                NotifyPropertyChanged(() => ContactSupportVisibility);
            }
        }

        Visibility resetLicenseRecordVisibility;
        public Visibility ResetLicenseRecordVisibility
        {
            get
            {
                return resetLicenseRecordVisibility;
            }
            set
            {
                if (Equals(value, resetLicenseRecordVisibility)) return;
                resetLicenseRecordVisibility = value;
                NotifyPropertyChanged(() => ResetLicenseRecordVisibility);
            }
        }

        ICommand activateTrialLicense;
        public ICommand ActivateTrialLicenseCommand
        {
            get
            {
                if (activateTrialLicense == null)
                {
                    activateTrialLicense = new DelegateCommand(ActivateTrialLicense);
                }
                return activateTrialLicense;
            }
        }

        private void ActivateTrialLicense(object obj)
        {
            RadWindow window = new RadWindow
            {
                Height = 600,
                Width = 600,
                SizeToContent = false,
                Content = new RequestTrialLicenseControl(),
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Header = LocHelper.GetWord("Activate Trial License"),
                CanClose = false,
                HideMinimizeButton = true,
                HideMaximizeButton = true
            };
            window.ShowDialog();
        }

        public void Init()
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("Checking license...")));
            //var licenseHelperInit = licenseHelper.Init(@"Resources\Lex", out bool hasErrorInit, out int statusCodeInit);
            //if (licenseHelperInit == false)
            //{
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("Error generating license files.")));
            //    SendErrorLogVisibility = Visibility.Visible;
            //    GetHelpVisibility = Visibility.Visible;
            //    return;
            //}

            ////if (licenseHelper.CheckLicense(out  statusCode) == false)
            //int statusCode = 0; if (false)
            //{
            //    var statusCodesEnum = (StatusCodesEnum)statusCode;
            //    if (statusCodesEnum == StatusCodesEnum.LA_EXPIRED)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License is expired.")));
            //        EntendYourLicenseVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_SUSPENDED)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License is suspended.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_GRACE_PERIOD_OVER)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License grace period is over.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_FAIL)
            //    {
            //        CheckTrial();
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_E_PRODUCT_ID)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License is corrupted.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        ResetLicenseRecordVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_E_LICENSE_KEY)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License is corrupted.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        ResetLicenseRecordVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_E_TIME)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("Your system time is wrong. Please check and fix your system time.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnum == StatusCodesEnum.LA_E_TIME_MODIFIED)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("You have changed your system time. Please check and fix your system time.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        CheckTrial();
            //    }
            //}
            //else
            //{
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("License is ok.")));
            //}
        }

        private void CheckTrial()
        {
            //if (licenseHelper.CheckTrialLicense(out int statusCodeTrial, out TimeSpan? expireDate) == false)
            //{
            //    var statusCodesEnumTrial = (StatusCodesEnum)statusCodeTrial;
            //    if (statusCodesEnumTrial == StatusCodesEnum.LA_TRIAL_EXPIRED)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("Trial license is expired.")));
            //        EntendYourLicenseVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnumTrial == StatusCodesEnum.LA_SUSPENDED)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("Trial license is suspended.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord("This computer is not licensed.")));
            //        ContactSupportVisibility = Visibility.Visible;
            //        ActivateTrialLicenseVisibility = Visibility.Visible;
            //        GetHelpVisibility = Visibility.Visible;
            //        EnterLicenseKeyVisibility = Visibility.Visible;
            //        PurchaseNewLicenseVisibility = Visibility.Visible;
            //    }
            //}
            //else
            //{
            //    var readableString = expireDate.Value.ToReadableString();
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Status = LocHelper.GetWord($"Trial license is ok. You have {readableString} before trial expires.")));
            //    CheckVersion();
            //}
        }

        private void CheckVersion()
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            if (isDebug)
            {
                //var path = @"..\..\..\Celsus.Client.Wpf\Package.zip";
                //if (File.Exists(path))
                //{
                //    var zipTargetPath = FileHelper.GetUnusedFileName(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
                //    File.Copy(path, zipTargetPath, true);
                //    var zipExtractPath = FileHelper.GetUnusedFolderName(Path.GetTempPath(), Guid.NewGuid().ToString());
                //    ZipFile.ExtractToDirectory(zipTargetPath, zipExtractPath);

                //}
                var path = @"..\..\..\Celsus.Client.Wpf\bin\Debug\Celsus.Client.Wpf.exe";
                Process.Start(path);
            }
        }




        //public bool CopyFiles()
        //{
        //    try
        //    {
        //        if (File.Exists("LexActivator.dll") == false)
        //        {
        //            File.Copy(@"Resources\Lex\LexActivator.dll", "LexActivator.dll");
        //        }

        //        if (File.Exists("LexActivator64.dll") == false)
        //        {
        //            File.Copy(@"Resources\Lex\LexActivator64.dll", "LexActivator64.dll");
        //        }

        //        if (File.Exists("Product.dat") == false)
        //        {
        //            File.Copy(@"Resources\Lex\Product.dat", "Product.dat");
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);
        //    }

        //    return false;
        //}
    }
}
