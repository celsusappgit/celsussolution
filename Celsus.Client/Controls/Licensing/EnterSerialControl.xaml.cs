using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace Celsus.Client.Controls.Licensing
{
    /// <summary>
    /// Interaction logic for EnterSerialControl.xaml
    /// </summary>
    public partial class EnterSerialControl : UserControl
    {
        public EnterSerialControl()
        {
            InitializeComponent();
            DataContext = EnterSerialControlModel.Instance;
        }
    }

   

    public class EnterSerialControlModel : BaseModel<EnterSerialControlModel>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;

        #endregion

        #region Properties

        

        string serialKey;
        public string SerialKey
        {
            get
            {
                return serialKey;
            }
            set
            {
                if (Equals(value, serialKey)) return;
                serialKey = value;
                NotifyPropertyChanged(() => SerialKey);
                NotifyPropertyChanged(() => SerialErrorVisibility);
                NotifyPropertyChanged(() => ActivateSerialCommand);
            }
        }

        public Visibility SerialErrorVisibility
        {
            get
            {
                //1B3913-E8F81E-4CB59B-F5FDEA-E3762F-B91A15
                if (string.IsNullOrWhiteSpace(SerialKey))
                {
                    return Visibility.Visible;
                }
                if (SerialKey.Length != "xxxxxx-xxxxxx-xxxxxx-xxxxxx-xxxxxx-xxxxxx".Length)
                {
                    return Visibility.Visible;
                }
                if (SerialKey.Count(x => x == '-') != "xxxxxx-xxxxxx-xxxxxx-xxxxxx-xxxxxx-xxxxxx".Count(x => x == '-'))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
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
                NotifyPropertyChanged(() => ActivateSerialCommand);
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
                NotifyPropertyChanged(() => ActivateSerialCommand);
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
                NotifyPropertyChanged(() => ActivateSerialCommand);
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
                NotifyPropertyChanged(() => ActivateSerialCommand);
            }
        }

        object status;
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
                NotifyPropertyChanged(() => StatusVisibility);
            }
        }

        public Visibility StatusVisibility
        {
            get
            {
                return Status == null ? Visibility.Collapsed : Visibility.Visible;
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
                if (string.IsNullOrWhiteSpace(EMail))
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

        ICommand activateSerial;
        public ICommand ActivateSerialCommand
        {
            get
            {
                if (activateSerial == null)
                    activateSerial = new RelayCommand(param => ActivateSerial(param), param => { return CanActivateSerial(param); });
                return activateSerial;
            }
        }

        private bool CanActivateSerial(object obj)
        {
            if (string.IsNullOrWhiteSpace(FirstName) == false && string.IsNullOrWhiteSpace(LastName) == false && string.IsNullOrWhiteSpace(EMail) == false && string.IsNullOrWhiteSpace(Organization) == false)
            {
                if (SerialErrorVisibility == Visibility.Collapsed)
                {
                    if (LicenseHelper.Instance.Status != LicenseHelperStatusEnum.HaveLicense)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ActivateSerial(object obj)
        {
            IsBusy = true;

            var activateSerial = LicenseHelper.Instance.ActivateSerial(out bool hasError, out int status, FirstName, LastName, EMail, Organization, SerialKey);

            if (activateSerial == false)
            {
                if (hasError)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        IsBusy = false;
                        SendErrorLogVisibility = Visibility.Visible;
                        Status = ("Error setting activation data.").ConvertToBindableText();
                    }));
                }
                else
                {
                    var statusEnum = (StatusCodesEnum)status;
                    if (statusEnum == StatusCodesEnum.LA_TRIAL_EXPIRED)
                    {
                        Status = "Product trial has expired.".ConvertToBindableText();
                        CloseWindowVisibility = Visibility.Visible;
                    }
                    else
                    {
                        Status = "ErrorOccuredErrorCode".ConvertToBindableText(statusEnum);
                        CloseWindowVisibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                Status = ("You have successfully activated your license. Please close and re-open Celsus again.").ConvertToBindableText();
                IsBusy = false;
                RadWindow.Alert(new DialogParameters() { Content = Status, DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = "Success", Owner = App.Current.MainWindow, Closed = AlertClosed });
            }
            NotifyPropertyChanged(() => ActivateSerialCommand);
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
        //    ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).CloseTabItem(typeof(EnterSerialControl));
        //}

        private void AlertClosed(object sender, WindowClosedEventArgs e)
        {
            App.Current.Shutdown();
        }

        #endregion
        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }
                LicenseHelper.Instance.PropertyChanged += LicenseHelper_PropertyChanged;
            }
        }

        private void LicenseHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("");
        }
    }



}
