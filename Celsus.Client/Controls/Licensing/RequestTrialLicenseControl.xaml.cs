using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.Client.Types.CryptlexApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.Collections.Generic;
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
    public class RequestTrialLicenseControlModel : BaseModel<RequestTrialLicenseControlModel>
    {
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

        bool eMailAlreadyExists;
        public bool EMailAlreadyExists
        {
            get
            {
                return eMailAlreadyExists;
            }
            set
            {
                if (Equals(value, eMailAlreadyExists)) return;
                eMailAlreadyExists = value;
                NotifyPropertyChanged(() => EMailAlreadyExists);
                NotifyPropertyChanged(() => EMailErrorVisibility);
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

        bool organizationAlreadyExists;
        public bool OrganizationAlreadyExists
        {
            get
            {
                return organizationAlreadyExists;
            }
            set
            {
                if (Equals(value, organizationAlreadyExists)) return;
                organizationAlreadyExists = value;
                NotifyPropertyChanged(() => OrganizationAlreadyExists);
                NotifyPropertyChanged(() => OrganizationErrorVisibility);
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
                if (EMailAlreadyExists)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility OrganizationErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(Organization) || OrganizationAlreadyExists ? Visibility.Visible : Visibility.Collapsed;
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
        //    ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).CloseTabItem(typeof(RequestTrialLicenseControl));
        //}

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
                    //if (LicenseHelper.Instance.CheckAnyLicense()==false)
                    if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.DontHaveLicense)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private string CreateRandomPassword(int length)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }

        private async void ActivateTrialLicense(object obj)
        {
            IsBusy = true;

            var hasErrors = false;

            var getAllUsersResult = await CryptlexHelper.GetAllUsersAsync();
            if (getAllUsersResult.Error != null)
            {
                Status = "CannotConnectLicenseServer".ConvertToBindableText();
                IsBusy = false;
                return;
            }

            var users = getAllUsersResult.Result;

            if (users.Count(x => x.Company != null && x.Company.Equals(organization, StringComparison.InvariantCultureIgnoreCase)) > 0)
            {
                Status = "OrganizationAlreadyExists".ConvertToBindableText();
                OrganizationAlreadyExists = true;
                hasErrors = true;
            }
            else
            {
                OrganizationAlreadyExists = false;
            }

            if (users.Count(x => x.Email.Equals(EMail, StringComparison.InvariantCultureIgnoreCase)) > 0)
            {
                Status = "EMailAlreadyExists".ConvertToBindableText();
                EMailAlreadyExists = true;
                hasErrors = true;
            }
            else
            {
                EMailAlreadyExists = false;
            }

            if (hasErrors)
            {
                IsBusy = false;
                return;
            }

            var password = CreateRandomPassword(8);

            var createUserResult = await CryptlexHelper.CreateUserAsync(FirstName, LastName, EMail, Organization, password);
            if (createUserResult.Error != null || createUserResult.Result == null)
            {
                Status = "CannotConnectLicenseServer".ConvertToBindableText();
                IsBusy = false;
                return;
            }

            var userId = createUserResult.Result.Id;

            var activateTrialLicense = LicenseHelper.Instance.ActivateTrialLicense(out bool hasError, out int status, firstName, lastName, EMail, organization, userId);

            if (activateTrialLicense == false)
            {
                if (hasError)
                {
                    await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                     {
                         IsBusy = false;
                         SendErrorLogVisibility = Visibility.Visible;
                         Status = "ErrorSettingTrialActivationData".ConvertToBindableText();
                     }));

                    var statusEnum = (StatusCodesEnum)status;

                    logger.Trace($"Error setting trial activation data {statusEnum}");

                    IsBusy = false;
                }
                else
                {
                    var statusEnum = (StatusCodesEnum)status;
                    if (statusEnum == StatusCodesEnum.LA_TRIAL_EXPIRED)
                    {
                        Status = "ProductTrialHasExpired".ConvertToBindableText();
                        CloseWindowVisibility = Visibility.Visible;
                    }
                    else
                    {
                        Status = "ErrorOccuredErrorCode".ConvertToBindableText(statusEnum);
                        CloseWindowVisibility = Visibility.Visible;
                    }

                    IsBusy = false;
                }
            }
            else
            {



                Status = "YouHaveSuccessfullyStartedYourTrialLicen".ConvertToBindableText();
                IsBusy = false;

                var textBlock = "YouHaveSuccessfullyStartedYourTrialLicen".ConvertToBindableText();
                textBlock.TextWrapping = TextWrapping.WrapWithOverflow;
                textBlock.Height = 200;
                textBlock.Width = 400;

                RadWindow.Alert(new DialogParameters() { Content = textBlock, DialogStartupLocation = WindowStartupLocation.CenterOwner, Header = TranslationSource.Instance["Success"], Owner = App.Current.MainWindow, Closed = AlertClosed });
            }
            NotifyPropertyChanged(() => ActivateTrialLicenseCommand);
        }



        private void AlertClosed(object sender, WindowClosedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
    public partial class RequestTrialLicenseControl : UserControl
    {
        public RequestTrialLicenseControl()
        {
            InitializeComponent();
            DataContext = RequestTrialLicenseControlModel.Instance;
        }
    }

    // <auto-generated />
    //
    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    //
    //    using QuickType;
    //
    //    var httpsApiCryptlexComV3Users = HttpsApiCryptlexComV3Users.FromJson(jsonString);



}

