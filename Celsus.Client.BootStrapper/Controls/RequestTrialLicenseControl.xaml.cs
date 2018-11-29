using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
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
using Telerik.Windows.Controls;

namespace Celsus.Client.BootStrapper.Controls
{
    /// <summary>
    /// Interaction logic for RequestTrialLicenseControl.xaml
    /// </summary>
    public partial class RequestTrialLicenseControl : UserControl
    {
        private RequestTrialLicenseControlModel model = new RequestTrialLicenseControlModel();
        public RequestTrialLicenseControl()
        {
            InitializeComponent();
            DataContext = model;
        }
    }

    public class RequestTrialLicenseControlModel : BaseModel
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public RequestTrialLicenseControlModel()
        {
            IsBusy = false;
            SendErrorLogVisibility = Visibility.Collapsed;
            CloseWindowVisibility = Visibility.Collapsed;
        }

        string status;
        public object Status
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

        string firstName;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                if (Equals(value, firstName)) return;
                firstName = value;
                NotifyPropertyChanged(() => FirstName);
                NotifyPropertyChanged(() => FirstNameErrorVisibility);
                NotifyPropertyChanged(() => ActivateTrialLicenseCommand);
            }
        }
        string lastName;
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                if (Equals(value, lastName)) return;
                lastName = value;
                NotifyPropertyChanged(() => LastName);
                NotifyPropertyChanged(() => LastNameErrorVisibility);
                NotifyPropertyChanged(() => ActivateTrialLicenseCommand);
            }
        }
        string eMail;
        public string EMail
        {
            get
            {
                return eMail;
            }
            set
            {
                if (Equals(value, eMail)) return;
                eMail = value;
                NotifyPropertyChanged(() => EMail);
                NotifyPropertyChanged(() => EMailErrorVisibility);
                NotifyPropertyChanged(() => ActivateTrialLicenseCommand);
            }
        }
        string organization;
        public string Organization
        {
            get
            {
                return organization;
            }
            set
            {
                if (Equals(value, organization)) return;
                organization = value;
                NotifyPropertyChanged(() => Organization);
                NotifyPropertyChanged(() => OrganizationErrorVisibility);
                NotifyPropertyChanged(() => ActivateTrialLicenseCommand);
            }
        }

        public Visibility FirstNameErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(FirstName) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility LastNameErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(LastName) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility EMailErrorVisibility
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Organization))
                {
                    return Visibility.Visible;
                }
                else
                {
                    RegexUtilities util = new RegexUtilities();
                    if (util.IsValidEmail(EMail) == false)
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility OrganizationErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(Organization) ? Visibility.Visible : Visibility.Collapsed;
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

        Visibility closeWindowVisibility;
        public Visibility CloseWindowVisibility
        {
            get
            {
                return closeWindowVisibility;
            }
            set
            {
                if (Equals(value, closeWindowVisibility)) return;
                closeWindowVisibility = value;
                NotifyPropertyChanged(() => CloseWindowVisibility);
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (Equals(value, isBusy)) return;
                isBusy = value;
                NotifyPropertyChanged(() => IsBusy);
            }
        }

        ICommand closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (closeWindowCommand == null)
                    closeWindowCommand = new RelayCommand(param => CloseWindow(param), param => { return true; });
                return closeWindowCommand;
            }
        }

        private void CloseWindow(object param)
        {
            throw new NotImplementedException();
        }

        ICommand activateTrialLicense;
        public ICommand ActivateTrialLicenseCommand
        {
            get
            {
                if (activateTrialLicense == null)
                    activateTrialLicense = new RelayCommand(param => ActivateTrialLicense(param), param => { return CanActivateTrialLicense(param); });
                return activateTrialLicense;
            }
        }

        private bool CanActivateTrialLicense(object obj)
        {
            if (string.IsNullOrWhiteSpace(FirstName) == false && string.IsNullOrWhiteSpace(LastName) == false && string.IsNullOrWhiteSpace(EMail) == false && string.IsNullOrWhiteSpace(Organization) == false)
            {
                RegexUtilities util = new RegexUtilities();
                if (util.IsValidEmail(EMail))
                {
                    return true;
                }
            }
            return false;
        }

        private void ActivateTrialLicense(object obj)
        {
            IsBusy = true;

            int status;
            status = LexActivator.SetTrialActivationMetadata("FirstName", FirstName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata FirstName Code {status}");
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    IsBusy = false;
                    SendErrorLogVisibility = Visibility.Visible;
                    Status = LocHelper.GetWord("Error setting activation data.");
                }));
                return;
            }
            status = LexActivator.SetTrialActivationMetadata("LastName", LastName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata LastName Code {status}");
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    IsBusy = false;
                    SendErrorLogVisibility = Visibility.Visible;
                    Status = LocHelper.GetWord("Error setting activation data.");
                }));
                return;
            }
            status = LexActivator.SetTrialActivationMetadata("eMail", EMail);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata eMail Code {status}");
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    IsBusy = false;
                    SendErrorLogVisibility = Visibility.Visible;
                    Status = LocHelper.GetWord("Error setting activation data.");
                }));
                return;
            }
            status = LexActivator.SetTrialActivationMetadata("Organization", Organization);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata Organization Code {status}");
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    IsBusy = false;
                    SendErrorLogVisibility = Visibility.Visible;
                    Status = LocHelper.GetWord("Error setting activation data.");
                }));
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                
                var activateTrial = LexActivator.ActivateTrial();
                var statusEnum = (StatusCodesEnum)activateTrial;
                IsBusy = false;
                if (statusEnum == StatusCodesEnum.LA_OK)
                {
                    Status = LocHelper.GetWord("You have successfully started your trial license. You can close this window and start using Celsus.");
                    CloseWindowVisibility = Visibility.Visible;
                }
                else if (status == LexActivator.StatusCodes.LA_TRIAL_EXPIRED)
                {
                    Status = LocHelper.GetWord("Product trial has expired.");
                    CloseWindowVisibility = Visibility.Visible;
                }
                else
                {
                    Status = LocHelper.GetWord($"Error occured. Error code {statusEnum.ToString()}");
                    CloseWindowVisibility = Visibility.Visible;
                }
            }));
            return;
        }
    }


}
