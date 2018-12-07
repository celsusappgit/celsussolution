using Celsus.Client.Controls.Licensing;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Telerik.Windows.Controls;

namespace Celsus.Client.Controls.Setup
{

    public class SetupMainControlModel : BaseModel<SetupMainControlModel>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;

        #endregion

        #region Properties

        public SettingsHelper SettingsHelper { get { return SettingsHelper.Instance; } }
        public SetupHelper SetupHelper { get { return SetupHelper.Instance; } }
        public LicenseHelper LicenseHelper { get { return LicenseHelper.Instance; } }
        public RolesHelper RolesHelper { get { return RolesHelper.Instance; } }

        public IEnumerable<LanguageInfo> Languages
        {
            get
            {
                return LocManager.Instance.Languages;
            }
        }

        public string SelectedLanguageKey
        {
            get
            {
                return TranslationSource.Instance.CurrentCulture.TwoLetterISOLanguageName;
            }
            set
            {
                TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(value);
                Properties.Settings.Default.Language = value;
                Properties.Settings.Default.Save();
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

        //public object Message
        //{
        //    get
        //    {
        //        return LicenseHelper.Instance.Status.ToString().ConvertToBindableText();
        //    }
        //}

        bool isBusyLicensing;
        public bool IsBusyLicensing
        {
            get
            {
                return isBusyLicensing;
            }
            set
            {
                if (Equals(value, isBusyLicensing)) return;
                isBusyLicensing = value;
                NotifyPropertyChanged(() => IsBusyLicensing);
            }
        }
        public string TopologyMessage
        {
            get
            {
                return TranslationSource.Instance[SetupHelper.Instance.Status.ToString()];
            }
        }

        //public string TopologySubMessage
        //{
        //    get
        //    {
        //        var databaseHelper = TranslationSource.Instance[DatabaseHelper.Instance.Status.ToString()];
        //        var rolesHelper = TranslationSource.Instance[RolesHelper.Instance.Status.ToString()];
        //        return $"Database:{databaseHelper}, Roles:{rolesHelper}";
        //    }
        //}

        public object SubMessage
        {
            get
            {
                switch (LicenseHelper.Instance.Status)
                {
                    case LicenseHelperStatusEnum.DontHaveLicense:
                        return "DontHaveLicenseSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.GotError:
                        return "GotErrorSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.HaveLicense:
                        return "HaveLicenseSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.HaveTrialLicense:
                        return "HaveTrialLicenseSubMessage".ConvertToBindableText(LicenseHelper.Instance.TrialLicenseInfo.TrialDueDate);
                    case LicenseHelperStatusEnum.LicenseSuspended:
                        return "LicenseSuspendedSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.LicenseGracePeriodOver:
                        return "LicenseGracePeriodOverSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.WrongProductKey:
                        return "WrongProductKeySubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.WrongProductId:
                        return "WrongProductIdSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.ComputerClockError:
                        return "ComputerClockErrorSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.ComputerClockCracked:
                        return "ComputerClockCrackedSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.LicenseExpired:
                        return "LicenseExpiredSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.LicenseCracked:
                        return "LicenseCrackedSubMessage".ConvertToBindableText();
                    case LicenseHelperStatusEnum.TrialLicenseExpired:
                        return "TrialLicenseExpiredSubMessage".ConvertToBindableText();
                    default:
                        return "GotErrorSubMessage".ConvertToBindableText();
                }
            }
        }

        ICommand openLicensingCommand;
        public ICommand OpenLicensingCommand
        {
            get
            {
                if (openLicensingCommand == null)
                    openLicensingCommand = new RelayCommand(param => OpenLicensing(param), param => { return true; });
                return openLicensingCommand;
            }
        }

        private void OpenLicensing(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(LicenseCheckControl));
        }

        ICommand openTopologyCommand;
        public ICommand OpenTopologyCommand
        {
            get
            {
                if (openTopologyCommand == null)
                    openTopologyCommand = new RelayCommand(param => OpenTopology(param), param => { return true; });
                return openTopologyCommand;
            }
        }

        private void OpenTopology(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(ManageRolesControl));
        }

        ICommand getTrialLicenseCommand;
        public ICommand GetTrialLicenseCommand
        {
            get
            {
                if (getTrialLicenseCommand == null)
                    getTrialLicenseCommand = new RelayCommand(param => GetTrialLicense(param), param => { return true; });
                return getTrialLicenseCommand;
            }
        }

        private void GetTrialLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(RequestTrialLicenseControl));
        }

        ICommand enterLicenseKeyCommand;
        public ICommand EnterLicenseKeyCommand
        {
            get
            {
                if (enterLicenseKeyCommand == null)
                    enterLicenseKeyCommand = new RelayCommand(param => EnterLicenseKey(param), param => { return true; });
                return enterLicenseKeyCommand;
            }
        }

        private void EnterLicenseKey(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(EnterSerialControl));
        }

        ICommand purchaseNewLicenseCommand;
        public ICommand PurchaseNewLicenseCommand
        {
            get
            {
                if (purchaseNewLicenseCommand == null)
                    purchaseNewLicenseCommand = new RelayCommand(param => PurchaseNewLicense(param), param => CanPurchaseNewLicense());
                return purchaseNewLicenseCommand;
            }
        }

        private bool CanPurchaseNewLicense()
        {
            return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense || LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense;
        }

        private void PurchaseNewLicense(object obj)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.celsusapp.com/shop");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259) ;
            }
            catch (Exception other)
            {
            }
        }

        ICommand entendYourLicenseCommand;
        public ICommand EntendYourLicenseCommand
        {
            get
            {
                if (entendYourLicenseCommand == null)
                    entendYourLicenseCommand = new RelayCommand(param => EntendYourLicense(param), param => { return true; });
                return entendYourLicenseCommand;
            }
        }

        private void EntendYourLicense(object obj)
        {
            if (LicenseHelper.Status == LicenseHelperStatusEnum.TrialLicenseExpired)
            {
                //IsBusyLicensing = true;
                //RadWindow.Prompt(new DialogParameters
                //{
                //    Content = LocHelper.GetWord("PleaseEnterReasonToExtendYourTrial"),
                //    Closed = new EventHandler<WindowClosedEventArgs>(OnPromptClosed),
                //    Owner = Application.Current.MainWindow
                //});
            }
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(ExtendLicenseInfoControl));
        }

        //private async void OnPromptClosed(object sender, WindowClosedEventArgs e)
        //{
        //    if (e.DialogResult.GetValueOrDefault() == true)
        //    {
        //        if (string.IsNullOrWhiteSpace(e.PromptResult) == false)
        //        {
        //            if (LicenseHelper.TrialLicenseInfo != null)
        //            {
        //                var body = LicenseHelper.TrialLicenseInfo.GetAsString() + Environment.NewLine + "Reason: " + e.PromptResult;
        //                var subject = "CELSUS | Extend Trial Request";
        //                var toAddresses = new List<string>() { "efeo@seneka.com.tr" };
        //                var smtpHelper = new SmtpHelper();
        //                var sendEMailResult = await smtpHelper.SendEMail(subject, body, toAddresses);
        //                if (sendEMailResult == false)
        //                {
        //                    RadWindow.Alert(new DialogParameters() { Content = LocHelper.GetWord("CannotRequestTrialExtend"), DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Error", Owner = App.Current.MainWindow });
        //                    IsBusyLicensing = false;
        //                }
        //                else
        //                {
        //                    RadWindow.Alert(new DialogParameters() { Content = LocHelper.GetWord("SendRequestTrialExtend"), DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Success", Owner = App.Current.MainWindow });
        //                    IsBusyLicensing = false;
        //                }
        //            }
        //            else
        //            {
        //                RadWindow.Alert(new DialogParameters() { Content = LocHelper.GetWord("CannotGetTrialLicenseInfo"), DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Error", Owner = App.Current.MainWindow });
        //                IsBusyLicensing = false;
        //            }
        //        }
        //        else
        //        {
        //            RadWindow.Alert(new DialogParameters() { Content = LocHelper.GetWord("PleaseEnterReason"), DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Warning", Owner = App.Current.MainWindow });
        //            IsBusyLicensing = false;
        //        }
        //    }
        //    else
        //    {
        //        IsBusyLicensing = false;
        //    }
        //}

        ICommand viewLicenseCommand;
        public ICommand ViewLicenseCommand
        {
            get
            {
                if (viewLicenseCommand == null)
                    viewLicenseCommand = new RelayCommand(param => ViewLicense(param), param => { return true; });
                return viewLicenseCommand;
            }
        }

        private void ViewLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(LicenseInfoControl));
        }

        ICommand resetLicenseCommand;
        public ICommand ResetLicenseCommand
        {
            get
            {
                if (resetLicenseCommand == null)
                    resetLicenseCommand = new RelayCommand(param => ResetLicense(param), param => { return true; });
                return resetLicenseCommand;
            }
        }

        private void ResetLicense(object obj)
        {
            LicenseHelper.Instance.Reset(out int st);

            var textBlock = "YouHaveSuccessfullyResettedLicenseInform".ConvertToBindableText();
            textBlock.TextWrapping = TextWrapping.WrapWithOverflow;
            textBlock.Height = 200;
            textBlock.Width = 400;
            RadWindow.Alert(new DialogParameters() { Content = textBlock, DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = TranslationSource.Instance["Success"], Owner = App.Current.MainWindow, Closed = AlertClosed });
            
        }

        private void AlertClosed(object sender, WindowClosedEventArgs e)
        {
            App.Current.Shutdown();
        }


        //ICommand closeWindowCommand;
        //public ICommand CloseWindowCommand
        //{
        //    get
        //    {
        //        if (closeWindowCommand == null)
        //            closeWindowCommand = new RelayCommand(param => CloseWindow(param), param => { return true; });
        //        return closeWindowCommand;
        //    }
        //}

        //private void CloseWindow(object param)
        //{
        //    ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).CloseTabItem(typeof(LicenseCheckControl));
        //}

        #endregion
        public void Init()
        {
        }
    }

    [Help("Topology", "")]
    public partial class SetupMainControl : UserControl
    {
        public SetupMainControl()
        {
            InitializeComponent();
            DataContext = SetupMainControlModel.Instance;
        }
    }
}
