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
using Telerik.Windows.Controls;

namespace Celsus.Client.Controls.Licensing
{
    public class ExtendLicenseInfoControlModel : BaseModel
    {
        public TrialLicenseInfo TrialLicenseInfo
        {
            get
            {
                return LicenseHelper.Instance.TrialLicenseInfo;
            }

        }

        string reason;
        public string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                if (Equals(value, reason)) return;
                reason = value;
                NotifyPropertyChanged(() => Reason);
                NotifyPropertyChanged(() => SendRequestCommand);
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

        bool sentSuccessfully;
        public bool SentSuccessfully
        {
            get
            {
                return sentSuccessfully;
            }
            set
            {
                if (Equals(value, sentSuccessfully)) return;
                sentSuccessfully = value;
                NotifyPropertyChanged(() => SentSuccessfully);
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


        ICommand sendRequestCommand;
        public ICommand SendRequestCommand
        {
            get
            {
                if (sendRequestCommand == null)
                    sendRequestCommand = new RelayCommand(param => SendRequest(param), param => { return CanSendRequest(param); });
                return sendRequestCommand;
            }
        }

        private bool CanSendRequest(object obj)
        {
            if (SentSuccessfully)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(Reason) == false)
            {
                if (LicenseHelper.Instance.Status != LicenseHelperStatusEnum.HaveLicense)
                {
                    return true;
                }
            }
            return false;
        }

        private async void SendRequest(object obj)
        {
            IsBusy = true;

            if (TrialLicenseInfo != null)
            {
                var body = TrialLicenseInfo.GetAsString() + Environment.NewLine + "Reason: " + Reason;
                var subject = "CELSUS | Extend Trial Request";
                var toAddresses = new List<string>() { "efeo@seneka.com.tr" };
                var smtpHelper = new SmtpHelper();
                var sendEMailResult = await smtpHelper.SendEMail(subject, body, toAddresses);
                if (sendEMailResult == false)
                {
                    Status = "CannotRequestTrialExtend".ConvertToBindableText();
                    IsBusy = false;
                }
                else
                {
                    Status = "SendRequestTrialExtend".ConvertToBindableText();
                    IsBusy = false;
                    SentSuccessfully = true;
                }
            }
            else
            {
                Status = "CannotGetTrialLicenseInfo".ConvertToBindableText();
                IsBusy = false;
            }
        }
    }

    public partial class ExtendLicenseInfoControl : UserControl
    {
        public ExtendLicenseInfoControl()
        {
            InitializeComponent();
            DataContext = new ExtendLicenseInfoControlModel();
        }
    }
}
