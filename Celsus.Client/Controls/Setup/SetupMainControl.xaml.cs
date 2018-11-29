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
                    purchaseNewLicenseCommand = new RelayCommand(param => PurchaseNewLicense(param), param => { return false; });
                return purchaseNewLicenseCommand;
            }
        }

        private void PurchaseNewLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(EnterSerialControl));
        }

        ICommand entendYourLicenseCommand;
        public ICommand EntendYourLicenseCommand
        {
            get
            {
                if (entendYourLicenseCommand == null)
                    entendYourLicenseCommand = new RelayCommand(param => EntendYourLicense(param), param => { return false; });
                return entendYourLicenseCommand;
            }
        }

        private void EntendYourLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(EnterSerialControl));
        }

        ICommand viewLicenseCommand;
        public ICommand ViewLicenseCommand
        {
            get
            {
                if (viewLicenseCommand == null)
                    viewLicenseCommand = new RelayCommand(param => ViewLicense(param), param => { return false; });
                return viewLicenseCommand;
            }
        }

        private void ViewLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(EnterSerialControl));
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
    public partial class SetupMainControl : UserControl
    {
        public SetupMainControl()
        {
            InitializeComponent();
            DataContext = SetupMainControlModel.Instance;
        }
    }
}
