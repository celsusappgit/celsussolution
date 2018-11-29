using Celsus.Client.Wpf.Types;
using Celsus.DataLayer;
using Celsus.Types;
using DbUp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for WizardCheckDatabase.xaml
    /// </summary>
    public partial class WizardCheckDatabase : UserControl
    {
        public WizardSQL Main { get; set; }
        public string NextParam { get; set; }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public WizardCheckDatabase()
        {
            InitializeComponent();
            //TxtChecking.Visibility = Visibility.Visible;
            TxtOk.Visibility = Visibility.Collapsed;
            TxtErrorAdded.Visibility = Visibility.Collapsed;
            TxtVersionOK.Visibility = Visibility.Collapsed;
            TxtVersionUpdated.Visibility = Visibility.Collapsed;
            TxtErrorNotAdded.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
            SpErrorOptions.Visibility = Visibility.Collapsed;
        }

        public async Task Do()
        {
            RadBusyIndicator.IsBusy = true;
            if (await One() == false)
            {

            }
            else
            {
                //TxtChecking.Visibility = Visibility.Collapsed;
                //TxtOk.Visibility = Visibility.Visible;
                //TxtError.Visibility = Visibility.Collapsed;
                //SpErrorOptions.Visibility = Visibility.Collapsed;
                //Main.BtnNext.IsEnabled = true;
                //NextParam = "WizardCheckDatabase";
            }

            RadBusyIndicator.IsBusy = false;
        }

        public async Task<bool> One()
        {
            try
            {

                Main.ConnectionInfo.InitialCatalog = "Celsus";


                var connectionString = Main.ConnectionInfo.ConnectionString;

                var upgradeEngine = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .LogToConsole()
                            .Build();

                bool canContinue = false;
                if (upgradeEngine.TryConnect(out string mes))
                {
                    TxtOk.Visibility = Visibility.Visible;
                    canContinue = true;
                }
                else
                {
                    try
                    {
                        EnsureDatabase.For.SqlDatabase(connectionString);
                        TxtErrorAdded.Visibility = Visibility.Visible;
                        canContinue = true;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error occured EnsureDatabase.");
                        TxtErrorNotAdded.Visibility = Visibility.Visible;
                    }
                }

                if (canContinue)
                {
                    var isUpgradeRequired = upgradeEngine.IsUpgradeRequired();
                    if (isUpgradeRequired)
                    {
                        var upgradeResult = upgradeEngine.PerformUpgrade();
                        if (upgradeResult.Successful == false)
                        {
                            logger.Error(upgradeResult.Error);
                            TxtError.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            TxtVersionUpdated.Visibility = Visibility.Visible;
                            SettingsManager.Instance.AddOrUpdateSetting(SettingName.ConnectionString, Main.ConnectionInfo.ConnectionString);
                            AddServerRole();
                            await (App.Current.MainWindow as MainWindow).CheckDatabaseAsync();
                        }
                    }
                    else
                    {
                        TxtVersionOK.Visibility = Visibility.Visible;
                        SettingsManager.Instance.AddOrUpdateSetting(SettingName.ConnectionString, Main.ConnectionInfo.ConnectionString);
                        AddServerRole();
                        await (App.Current.MainWindow as MainWindow).CheckDatabaseAsync();

                        
                    }
                }

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

        public async void AddServerRole()
        {
            var serverRoleDto = new ServerRoleDto
            {
                ServerId = SetupManager.Instance.ServerId,
                ServerIP = SetupManager.Instance.IPAddress,
                ServerName = Environment.MachineName,
                ServerRoleEnum = ServerRoleEnum.Database,
                IsActive = true
            };

            try
            {
                using (var context = new SqlDbContext(Main.ConnectionInfo.ConnectionString))
                {
                    var oldRoles = context.ServerRoles.Where(x => x.ServerRoleEnum == ServerRoleEnum.Database && x.IsActive == true).ToList();
                    if (oldRoles.Count == 0)
                    {
                        context.ServerRoles.Add(serverRoleDto);
                        await context.SaveChangesAsync();
                    }
                    else if (oldRoles.Count == 1)
                    {
                        oldRoles.First().IsActive = false;
                        context.Entry(oldRoles.First()).State = System.Data.Entity.EntityState.Modified;

                        context.ServerRoles.Add(serverRoleDto);

                        await context.SaveChangesAsync();
                    }
                    else if (oldRoles.Count > 1)
                    {
                        var toDelete = oldRoles.Take(oldRoles.Count - 1).ToList();
                        foreach (var item in toDelete)
                        {
                            item.IsActive = false;
                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }

                        oldRoles.Last().ServerId = serverRoleDto.ServerId;
                        oldRoles.Last().ServerIP = serverRoleDto.ServerIP;
                        oldRoles.Last().ServerName = serverRoleDto.ServerName;
                        oldRoles.Last().ServerRoleEnum = serverRoleDto.ServerRoleEnum;
                        context.Entry(oldRoles.Last()).State = System.Data.Entity.EntityState.Modified;

                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured EnsureDatabase.");
                TxtErrorNotAdded.Visibility = Visibility.Visible;
            }
        }
    }
}
