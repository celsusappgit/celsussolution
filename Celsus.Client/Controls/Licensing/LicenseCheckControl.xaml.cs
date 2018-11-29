using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.Collections.Generic;
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

namespace Celsus.Client.Controls.Licensing
{
    public class LicenseCheckControlModel : BaseModel<LicenseCheckControlModel>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;

        #endregion

        #region Properties

        public string Message
        {
            get
            {
                return TranslationSource.Instance[LicenseHelper.Instance.Status.ToString()];
            }
        }

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

        public Visibility ViewLicenseVisibility
        {
            get
            {
                return (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveLicense ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.TrialLicenseExpired ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseExpired ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseGracePeriodOver ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseSuspended
                        ) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility TrialLicenseInfoVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public Visibility ActivateTrialLicenseVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility EnterLicenseKeyVisibility
        {
            get
            {
                return (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense ||
                        LicenseHelper.Instance.Status == LicenseHelperStatusEnum.TrialLicenseExpired) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility PurchaseNewLicenseVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SendErrorLogVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.GotError ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility EntendYourLicenseVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseExpired ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ContactSupportVisibility
        {
            get
            {
                return (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseSuspended
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseGracePeriodOver
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.WrongProductId
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.WrongProductKey
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.ComputerClockError
                    ||
                    LicenseHelper.Instance.Status == LicenseHelperStatusEnum.ComputerClockCracked) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ResetLicenseRecordVisibility
        {
            get
            {
                return LicenseHelper.Instance.Status == LicenseHelperStatusEnum.LicenseCracked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        ICommand activateTrialLicense;
        public ICommand ActivateTrialLicenseCommand
        {
            get
            {
                if (activateTrialLicense == null)
                    activateTrialLicense = new RelayCommand(param => ActivateTrialLicense(param), param => { return true; });
                return activateTrialLicense;
            }
        }

        private void ActivateTrialLicense(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(RequestTrialLicenseControl));
        }

        ICommand activateSerial;
        public ICommand ActivateSerialCommand
        {
            get
            {
                if (activateSerial == null)
                    activateSerial = new RelayCommand(param => ActivateSerial(param), param => { return true; });
                return activateSerial;
            }
        }

        private void ActivateSerial(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(EnterSerialControl));
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

        public LicenseCheckControlModel()
        {
        }
        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }
                LicenseHelper.Instance.PropertyChanged += LicenseHelper_PropertyChanged;
                isInitted = true;
            }

        }
        private void LicenseHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("");
        }

        private void CheckTrial()
        {
            //if (licenseHelper.CheckTrialLicense(out int statusCodeTrial, out TimeSpan? expireDate) == false)
            //{
            //    var statusCodesEnumTrial = (StatusCodesEnum)statusCodeTrial;
            //    if (statusCodesEnumTrial == StatusCodesEnum.LA_TRIAL_EXPIRED)
            //    {
            //        Run(() => Status = LocHelper.GetWord("Trial license is expired."));
            //        ContactSupportVisibility = Visibility.Visible;
            //    }
            //    else if (statusCodesEnumTrial == StatusCodesEnum.LA_SUSPENDED)
            //    {
            //        Run(() => Status = LocHelper.GetWord("Trial license is suspended."));
            //        ContactSupportVisibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        Run(() => Status = LocHelper.GetWord("This computer is not licensed."));
            //    }
            //}
            //else
            //{
            //    var readableString = expireDate.Value.ToReadableString();
            //    Run(() => Status = LocHelper.GetWord($"Trial license is ok. You have {readableString} before trial expires."));
            //    IsLicensed = true;
            //    //CheckVersion();
            //}
        }
    }
    public partial class LicenseCheckControl : UserControl
    {
        public LicenseCheckControl()
        {
            InitializeComponent();
            DataContext = LicenseCheckControlModel.Instance;
            //LicenseCheckControlModel.Instance.Init();
        }

        public void Init()
        {

        }
    }
}
