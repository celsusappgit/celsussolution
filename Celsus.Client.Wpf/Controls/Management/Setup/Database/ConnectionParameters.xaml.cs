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
using Telerik.Windows.Controls;

namespace Celsus.Client.Wpf.Controls.Management.Setup.Database
{
    /// <summary>
    /// Interaction logic for ConnectionParameters.xaml
    /// </summary>
    public partial class ConnectionParameters : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ConnectionInfo ConnectionInfo { get; private set; } = new ConnectionInfo();
        public bool CanConnect { get; private set; }
        public ConnectionParameters()
        {
            InitializeComponent();
            DataContext = ConnectionInfo;
            TxtOk.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
        }
        private async void CheckSQLServer_Click(object sender, RoutedEventArgs e)
        {
            RadBusyIndicator.IsBusy = true;

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"Connection opened to SQL server.");

                TxtOk.Visibility = Visibility.Visible;
                TxtError.Visibility = Visibility.Collapsed;
                CanConnect = true;

            }
            catch (SqlException sqlException)
            {
                RunException.Text = sqlException.Message;
                if (sqlException.Number == 4060)
                {
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                }
                else
                {
                    //Model.AddMode("NeedDatabaseInstall");
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                }
                TxtOk.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Visible;
                CanConnect = false;
            }
            catch (Exception ex)
            {
                RunException.Text = ex.Message;
                logger.Error(ex, $"Error occured connecting Celsus database.");

                TxtOk.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Visible;
                CanConnect = false;
            }
            finally
            {
                RadBusyIndicator.IsBusy = false;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = (sender as RadButton).GetVisualParent<RadWindow>();
            window.Close();
        }
    }
}
