using Celsus.Client.Wpf.Controls.Management.Setup.Database;
using Celsus.Client.Wpf.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace Celsus.Client.Wpf.Controls.Management.Setup.FirstLoad
{
    /// <summary>
    /// Interaction logic for Topology.xaml
    /// </summary>
    public partial class Topology : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private List<ServerRoleDto> serverRoles = null;
        public Topology()
        {
            InitializeComponent();
            Loaded += Topology_Loaded;
        }

        private async void Topology_Loaded(object sender, RoutedEventArgs e)
        {
            SetupManager.Instance.InvalidateConnectionString();
            //await Check();
        }

        private async Task Check()
        {
            SpDBDontHaveDatabaseConnectionInfo.Visibility = Visibility.Collapsed;
            SpDBConnectionInfoError.Visibility = Visibility.Collapsed;
            SpDBCannotGetRoles.Visibility = Visibility.Collapsed;
            SpDBDatabaseRoleNotInstalled.Visibility = Visibility.Collapsed;
            SpDBMoreThanOneDatabaseRole.Visibility = Visibility.Collapsed;

            //SpICannotGetRoles
            SpIDontHaveConnectionInfoForIndexerRole.Visibility = Visibility.Collapsed;
            SpIIndexerRoleNotInstalled.Visibility = Visibility.Collapsed;
            SpIMoreThanOneIndexerRole.Visibility = Visibility.Collapsed;
            SpICannotGetRoles.Visibility = Visibility.Collapsed;

            SpVDontHaveConnectionInfoForViewerRole.Visibility = Visibility.Collapsed;
            SpVCannotGetRoles.Visibility = Visibility.Collapsed;
            SpVViewerRoleNotInstalled.Visibility = Visibility.Collapsed;

            BorderDBDatabaseRole.Visibility = Visibility.Collapsed;
            BorderIIndexerRole.Visibility = Visibility.Collapsed;
            BorderVViewerRole.Visibility = Visibility.Collapsed;

            if (SetupManager.Instance.IsConnectionSettingOk)
            {
                if (SetupManager.Instance.CanReachDatabase)
                {
                    if (await GetRoles())
                    {
                        var databaseRoles = serverRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Database).ToList();
                        if (databaseRoles.Count == 0)
                        {
                            SpDBDatabaseRoleNotInstalled.Visibility = Visibility.Visible;
                        }
                        else if (databaseRoles.Count == 1)
                        {
                            BorderDBDatabaseRole.Visibility = Visibility.Visible;
                            BorderDBDatabaseRole.DataContext = databaseRoles.First();
                        }
                        else
                        {
                            SpDBMoreThanOneDatabaseRole.Visibility = Visibility.Visible;
                        }

                        var indexerRoles = serverRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Indexer).ToList();
                        if (indexerRoles.Count == 0)
                        {
                            SpIIndexerRoleNotInstalled.Visibility = Visibility.Visible;
                        }
                        else if (indexerRoles.Count == 1)
                        {
                            BorderIIndexerRole.Visibility = Visibility.Visible;
                            BorderIIndexerRole.DataContext = indexerRoles.First();
                        }
                        else
                        {
                            SpIMoreThanOneIndexerRole.Visibility = Visibility.Visible;
                        }

                        var viewerRoles = serverRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Viewer).ToList();
                        if (viewerRoles.Count == 0)
                        {
                            SpVViewerRoleNotInstalled.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            BorderVViewerRole.Visibility = Visibility.Visible;
                            BorderVViewerRole.DataContext = viewerRoles.First();
                        }
                    }
                    else
                    {
                        SpDBCannotGetRoles.Visibility = Visibility.Visible;
                        SpICannotGetRoles.Visibility = Visibility.Visible;
                        SpVCannotGetRoles.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    SpDBConnectionInfoError.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SpDBDontHaveDatabaseConnectionInfo.Visibility = Visibility.Visible;
            }
        }




        private async Task<bool> GetRoles()
        {
            serverRoles = SetupManager.Instance.AllRoles;
            if (serverRoles!=null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void EnterDBConnectionInfo(object sender, MouseButtonEventArgs e)
        {
            var connectionParameters = new ConnectionParameters();

            RadWindow window = new RadWindow();

            window.Height = 600;
            window.Width = 600;
            window.SizeToContent = false;
            window.Content = connectionParameters;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Header = "Enter Connection";
            window.CanClose = false;
            window.ShowDialog();

            if (connectionParameters.CanConnect)
            {
                SettingsManager.Instance.AddOrUpdateSetting(SettingName.ConnectionString, connectionParameters.ConnectionInfo.ConnectionString);
                await Check();
            }
        }

        private void InstallSQL(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack("WizardSQL");
        }
        private void InstallIndexer(object sender, MouseButtonEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack("InstallTesseract");
        }

        private void ShowConnectionInfo(object sender, MouseButtonEventArgs e)
        {
            var connectionParameters = new ConnectionParameters();
            connectionParameters.Background = Brushes.White;
            var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
            connectionParameters.ConnectionInfo.ConnectionString = connectionString;

            RadWindow window = new RadWindow();
            window.Background = Brushes.Black;
            window.Height = 600;
            window.Width = 600;
            window.SizeToContent = false;
            window.Content = connectionParameters;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Header = "Enter Connection";
            window.CanClose = true;
            window.ShowDialog();
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {

        }

        
    }
}
