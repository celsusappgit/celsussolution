using Celsus.Client.Wpf.Types;
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
using Celsus.Client.Shared.Lex;

namespace Celsus.Client.Wpf.Controls.Management.Setup.Licensing
{
    /// <summary>
    /// Interaction logic for RequestTrial.xaml
    /// </summary>
    public partial class RequestTrial : UserControl
    {
        public RequestTrial()
        {
            InitializeComponent();
            TxtError.Visibility = Visibility.Collapsed;
            TxtOk.Visibility = Visibility.Hidden;
            TxtFirstNameError.Visibility = Visibility.Hidden;
            TxtLastNameError.Visibility = Visibility.Hidden;
            TxtEMailError.Visibility = Visibility.Hidden;
            TxtOrganizationError.Visibility = Visibility.Hidden;
        }

        private void Save(object sender, RoutedEventArgs e)
        {



            TxtFirstNameError.Visibility = string.IsNullOrWhiteSpace(TxtFirstName.Value) ? Visibility.Visible : Visibility.Collapsed;
            TxtLastNameError.Visibility = string.IsNullOrWhiteSpace(TxtLastName.Value) ? Visibility.Visible : Visibility.Collapsed;
            TxtEMailError.Visibility = string.IsNullOrWhiteSpace(TxtEMail.Value) ? Visibility.Visible : Visibility.Collapsed;
            TxtOrganizationError.Visibility = string.IsNullOrWhiteSpace(TxtOrganization.Value) ? Visibility.Visible : Visibility.Collapsed;

            RegexUtilities util = new RegexUtilities();
            TxtEMailError.Visibility = util.IsValidEmail(TxtEMail.Value) == false ? Visibility.Visible : Visibility.Collapsed;

            if (TxtFirstNameError.Visibility == Visibility.Visible ||
                TxtLastNameError.Visibility == Visibility.Visible ||
                TxtEMailError.Visibility == Visibility.Visible ||
                TxtOrganizationError.Visibility == Visibility.Visible
                )
            {
                return;
            }

            var backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            int lest;
            lest = LexActivator.SetTrialActivationMetadata("FirstName", TxtFirstName.Value);
            if (lest != LexActivator.StatusCodes.LA_OK) { return; }
            lest = LexActivator.SetTrialActivationMetadata("LastName", TxtLastName.Value);
            if (lest != LexActivator.StatusCodes.LA_OK) { return; }
            lest = LexActivator.SetTrialActivationMetadata("eMail", TxtEMail.Value);
            if (lest != LexActivator.StatusCodes.LA_OK) { return; }
            lest = LexActivator.SetTrialActivationMetadata("Organization", TxtOrganization.Value);
            if (lest != LexActivator.StatusCodes.LA_OK) { return; }

            RadBusyIndicator.IsBusy = true;

            backgroundWorker.RunWorkerAsync();


        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RadBusyIndicator.IsBusy = false;
            var sta = (int)e.Result;
            if (sta == LexActivator.StatusCodes.LA_OK)
            {
                TxtOk.Visibility = Visibility.Visible;
                RadWindow window = (sender as RadButton).GetVisualParent<RadWindow>();
                window.CanClose = true;
            }
            else if (sta == LexActivator.StatusCodes.LA_TRIAL_EXPIRED)
            {
                RadWindow window = (sender as RadButton).GetVisualParent<RadWindow>();
                ErrorCode.Text = "Product trial has expired.";
            }
            else
            {
                RadWindow window = (sender as RadButton).GetVisualParent<RadWindow>();
                ErrorCode.Text = sta.ToString();
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            var sra = LexActivator.ActivateTrial();
            e.Result = sra;
            return;



        }


    }
}
