using Celsus.Client.Wpf.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    /// <summary>
    /// Interaction logic for WizardCheckSqlServer.xaml
    /// </summary>
    public partial class WizardCheckSqlServer : UserControl
    {
        public WizardSQL Main { get; set; }
        public string NextParam { get; set; }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public WizardCheckSqlServer()
        {
            InitializeComponent();
            TxtChecking.Visibility = Visibility.Visible;
            TxtOk.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
            //RadBusyIndicator.Visibility = Visibility.Visible;
            SpErrorOptions.Visibility = Visibility.Collapsed;
        }

        public async Task Do()
        {
            RadBusyIndicator.IsBusy = true;
            if (await One(@".\SQLEXPRESS") == false)
            {
                if (await One(@".") == false)
                {
                    TxtChecking.Visibility = Visibility.Collapsed;
                    //RadBusyIndicator.Visibility = Visibility.Collapsed;
                    TxtOk.Visibility = Visibility.Collapsed;
                    TxtError.Visibility = Visibility.Visible;
                    SpErrorOptions.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtChecking.Visibility = Visibility.Collapsed;
                    //RadBusyIndicator.Visibility = Visibility.Collapsed;
                    TxtOk.Visibility = Visibility.Visible;
                    TxtError.Visibility = Visibility.Collapsed;
                    SpErrorOptions.Visibility = Visibility.Collapsed;
                    Main.BtnNext.IsEnabled = true;
                    NextParam = "WizardCheckDatabase";
                }
            }
            else
            {
                TxtChecking.Visibility = Visibility.Collapsed;
                //RadBusyIndicator.Visibility = Visibility.Collapsed;
                TxtOk.Visibility = Visibility.Visible;
                TxtError.Visibility = Visibility.Collapsed;
                SpErrorOptions.Visibility = Visibility.Collapsed;
                Main.BtnNext.IsEnabled = true;
                NextParam = "WizardCheckDatabase";
            }

            RadBusyIndicator.IsBusy = false;
        }

        public async Task<bool> One(string server)
        {
            try
            {
                Main.ConnectionInfo.InitialCatalog = "Master";
                Main.ConnectionInfo.IntegratedSecurity = true;
                Main.ConnectionInfo.ConnectTimeout = 5;
                Main.ConnectionInfo.Server = server;

                using (var sqlConnection = new SqlConnection(Main.ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"SQL Server found.");
                
                return true;
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 18456)
                {
                    logger.Trace(sqlException, $"Login failed.");
                }
                else
                {
                    logger.Trace(sqlException, $"SQL Server is not reachable.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting SQL Server.");
            }
            finally
            {
            }
            return false;
        }

        private void RadRadioButton_Click(object sender, RoutedEventArgs e)
        {
            NextParam = "WizardEnterConnection";
            Main.BtnNext.IsEnabled = true;
        }

        private void RadRadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            NextParam = "WizardInstallSQL";
            Main.BtnNext.IsEnabled = true;
        }
    }
}
