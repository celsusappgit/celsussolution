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
    /// Interaction logic for WizardEnterConnection.xaml
    /// </summary>
    public partial class WizardEnterConnection : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public WizardSQL Main { get; set; }
        public string NextParam { get; set; }
        public WizardEnterConnection()
        {
            InitializeComponent();

            Loaded += WizardEnterConnection_Loaded;
        }

        private void WizardEnterConnection_Loaded(object sender, RoutedEventArgs e)
        {
            if (Main == null)
            {
                Main = new WizardSQL();
            }
            Main.ConnectionInfo.Server = @".\SQLEXPRESS";
            Main.ConnectionInfo.InitialCatalog = "Master";
            DataContext = this;

        }

        private async void CheckSQLServer_Click(object sender, RoutedEventArgs e)
        {
            RadBusyIndicator.IsBusy = true;

            try
            {
                using (var sqlConnection = new SqlConnection(Main.ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"Connection opened to SQL server.");

                TxtOk.Visibility = Visibility.Visible;
                TxtError.Visibility = Visibility.Collapsed;
                Main.BtnNext.IsEnabled = true;
                NextParam = "WizardCheckDatabase";
            }
            catch (SqlException sqlException)
            {
                RunException.Text = sqlException.Message;
                if (sqlException.Number == 4060)
                {
                    //Model.AddMode("NeedDatabaseInstall");
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                }
                else
                {
                    //Model.AddMode("NeedDatabaseInstall");
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                }
                TxtOk.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Visible;
                Main.BtnNext.IsEnabled = true;
                NextParam = "WizardInstallSQL";
            }
            catch (Exception ex)
            {
                RunException.Text = ex.Message;
                //Model.AddMode("NeedDatabaseInstall");
                logger.Error(ex, $"Error occured connecting Celsus database.");

                TxtOk.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Visible;
                Main.BtnNext.IsEnabled = true;
                NextParam = "WizardInstallSQL";
            }
            finally
            {
                RadBusyIndicator.IsBusy = false;
            }
        }

        public async Task Do()
        {
            return;
        }
    }
}
