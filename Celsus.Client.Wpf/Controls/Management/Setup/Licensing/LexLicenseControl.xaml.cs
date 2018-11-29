using Celsus.Client.Wpf.Types;
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
using Celsus.Client.Shared.Lex;

namespace Celsus.Client.Wpf.Controls.Management.Setup.Licensing
{
    /// <summary>
    /// Interaction logic for LexLicenseControl.xaml
    /// </summary>
    public partial class LexLicenseControl : UserControl
    {
        public LexLicenseControl()
        {
            InitializeComponent();
            Loaded += LexLicenseControl_Loaded;
        }

        private void LexLicenseControl_Loaded(object sender, RoutedEventArgs e)
        {
            SpLicenseOk.Visibility = Visibility.Collapsed;
            SpLicenseNoLicense.Visibility = Visibility.Collapsed;
            SpLicenseError.Visibility = Visibility.Collapsed;
            SpLicenseIsSuspended.Visibility = Visibility.Collapsed;
            SpLicenseExpired.Visibility = Visibility.Collapsed;
            SpTrialLicenseOk.Visibility = Visibility.Collapsed;
            SpTrialLicenseIsSuspended.Visibility = Visibility.Collapsed;
            SpTrialLicenseExpired.Visibility = Visibility.Collapsed;
            
            var isLicenseGenuine = LexActivator.IsLicenseGenuine();
            if (isLicenseGenuine == LexActivator.StatusCodes.LA_OK)
            {
                SpLicenseOk.Visibility = Visibility.Visible;
            }
            else if(isLicenseGenuine == LexActivator.StatusCodes.LA_EXPIRED)
            {
                SpLicenseExpired.Visibility = Visibility.Visible;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_SUSPENDED)
            {
                SpLicenseIsSuspended.Visibility = Visibility.Visible;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_GRACE_PERIOD_OVER)
            {
                SpLicenseExpired.Visibility = Visibility.Visible;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_FAIL || isLicenseGenuine == LexActivator.StatusCodes.LA_E_PRODUCT_ID || isLicenseGenuine == LexActivator.StatusCodes.LA_E_LICENSE_KEY || isLicenseGenuine == LexActivator.StatusCodes.LA_E_TIME || isLicenseGenuine == LexActivator.StatusCodes.LA_E_TIME_MODIFIED)
            {
                SpLicenseError.Visibility = Visibility.Visible;
                ErrorCode.Text = ((StatusCodesEnum)isLicenseGenuine).ToString();
            }
            else
            {
                var isTrialGenuine = LexActivator.IsTrialGenuine();
                if (isTrialGenuine == LexActivator.StatusCodes.LA_OK)
                {
                    uint trialExpiryDate = 0;
                    var getTrialExpiryDate = LexActivator.GetTrialExpiryDate(ref trialExpiryDate);
                    var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(trialExpiryDate).ToLocalTime();
                    //LexActivator.Reset();
                    SpTrialLicenseOk.Visibility = Visibility.Visible;

                }
                else if (isTrialGenuine == LexActivator.StatusCodes.LA_TRIAL_EXPIRED)
                {
                    SpTrialLicenseExpired.Visibility = Visibility.Visible;
                }
                else if (isTrialGenuine == LexActivator.StatusCodes.LA_SUSPENDED)
                {
                    SpTrialLicenseIsSuspended.Visibility = Visibility.Visible;
                }
                else
                {
                    SpLicenseNoLicense.Visibility = Visibility.Visible;
                }
                //SpLicenseError.Visibility = Visibility.Visible;
            }
        }

        private void CheckNewSerial(object sender, RoutedEventArgs e)
        {

        }

        private void SeeLicense(object sender, MouseButtonEventArgs e)
        {
            RadWindow window = new RadWindow();

            window.Height = 600;
            window.Width = 600;
            window.SizeToContent = false;
            window.Content = new LicenseInfoControl();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Header = "Enter Connection";
            window.CanClose = false;
            window.ShowDialog();
        }


        private void GetTrial(object sender, MouseButtonEventArgs e)
        {
            RadWindow window = new RadWindow();

            window.Height = 600;
            window.Width = 600;
            window.SizeToContent = false;
            window.Content = new RequestTrial();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Header = "Enter Connection";
            window.CanClose = false;
            window.ShowDialog();


        }

       
    }
}
