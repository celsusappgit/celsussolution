using DbUp;
using DbUp.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum DatabaseHelperStatusEnum
    {
        DontHaveConnectionInfo = 0,
        GotError = 1,
        MasterDatabaseReachable = 2,
        MasterDatabaseLoginFailed = 3,
        MasterDatabaseIsNotReachable = 4,
        CelsusDatabaseExists = 5,
        CelsusDatabaseNotExists = 6,
        CelsusDatabaseReachable = 7,
        CelsusDatabaseLoginFailed = 8,
        CelsusDatabaseIsNotReachable = 9,
        CelsusDatabaseVersionOld = 10,
        CelsusDatabaseVersionOk = 11
    }
    public class DatabaseHelper : BaseModel<DatabaseHelper>, MustInit
    {


        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private bool isInitted = false;

        private UpgradeEngine upgradeEngine = null;



        ConnectionInfo connectionInfo;
        public ConnectionInfo ConnectionInfo
        {
            get
            {
                return connectionInfo;
            }
            private set
            {
                if (Equals(value, connectionInfo)) return;
                if (connectionInfo != null)
                {
                    connectionInfo.PropertyChanged -= ConnectionInfo_PropertyChanged;
                }
                connectionInfo = value;
                if (connectionInfo != null)
                {
                    connectionInfo.PropertyChanged += ConnectionInfo_PropertyChanged;
                }
                NotifyPropertyChanged(() => ConnectionInfo);
            }
        }

        private void ConnectionInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("");
        }

        DatabaseHelperStatusEnum status;
        public DatabaseHelperStatusEnum Status
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
                RolesHelper.Instance.Invalidate();
                SetupHelper.Instance.Invalidate();
            }
        }

        bool? isUpgradeRequired;
        public bool? IsUpgradeRequired
        {
            get
            {
                return isUpgradeRequired;
            }
            set
            {
                if (Equals(value, isUpgradeRequired)) return;
                isUpgradeRequired = value;
                NotifyPropertyChanged(() => IsUpgradeRequired);
            }
        }

        public DatabaseHelper()
        {
            ConnectionInfo = new ConnectionInfo
            {
                ConnectionString = SettingsHelper.Instance.ConnectionString
            };
            //SettingsHelper.Instance.PropertyChanged += SettingsHelper_PropertyChanged;
        }

        internal async void Invalidate()
        {
            ConnectionInfo.ConnectionString = SettingsHelper.Instance.ConnectionString;
            await CheckAll();
        }

        //private async void SettingsHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "ConnectionString")
        //    {
        //        ConnectionInfo.ConnectionString = SettingsHelper.Instance.ConnectionString;
        //        await CheckAll();
        //    }
        //}

        public async void Init()
        {
            if (isInitted)
            {
                return;
            }
            await semaphoreSlim.WaitAsync();
            try
            {
                await CheckAll();
                isInitted = true;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }


        }

        public void SetConnectionString(string connectionString)
        {
            ConnectionInfo.ConnectionString = connectionString;
        }
        private async Task CheckAll()
        {
            CheckPort();
            await CheckMasterDatabase();
            if (Status == DatabaseHelperStatusEnum.MasterDatabaseReachable)
            {
                await CheckCelsusDatabaseExists();
                if (Status == DatabaseHelperStatusEnum.CelsusDatabaseExists)
                {
                    await CheckDatabase();
                    if (Status == DatabaseHelperStatusEnum.CelsusDatabaseReachable)
                    {
                        CheckDatabaseVersion();
                    }
                }
            }
        }

        public async Task CheckMasterDatabase()
        {
            if (string.IsNullOrWhiteSpace(ConnectionInfo.ConnectionStringForMaster))
            {
                Status = DatabaseHelperStatusEnum.DontHaveConnectionInfo;
                return;
            }
            try
            {

                using (var sqlConnection = new SqlConnection(ConnectionInfo.ConnectionStringForMaster))
                {
                    await sqlConnection.OpenAsync();
                    Status = DatabaseHelperStatusEnum.MasterDatabaseReachable;
                }
                logger.Trace($"Connection opened to Celsus database.");
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 4060)
                {
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                    Status = DatabaseHelperStatusEnum.MasterDatabaseLoginFailed;
                }
                else
                {
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                    Status = DatabaseHelperStatusEnum.MasterDatabaseIsNotReachable;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting Celsus database.");
                Status = DatabaseHelperStatusEnum.MasterDatabaseIsNotReachable;
            }
        }

        public async Task CheckCelsusDatabaseExists()
        {
            var celsusDatabaseExists = false;
            using (SqlConnection con = new SqlConnection(ConnectionInfo.ConnectionStringForMaster))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var databaseName = reader[0].ToString();
                            if (string.Equals(databaseName, "Celsus", StringComparison.InvariantCultureIgnoreCase))
                            {
                                celsusDatabaseExists = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (celsusDatabaseExists)
            {
                Status = DatabaseHelperStatusEnum.CelsusDatabaseExists;
            }
            else
            {
                Status = DatabaseHelperStatusEnum.CelsusDatabaseNotExists;
            }

        }

        public bool CheckPort()
        {
            var result = false;

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult asyncResult = socket.BeginConnect(ConnectionInfo.Server, 1433, null, null);
                    result = asyncResult.AsyncWaitHandle.WaitOne(500, true);
                    socket.Close();
                }
            }
            catch
            {
            }
            return result;
        }
        public async Task CheckDatabase()
        {
            if (string.IsNullOrWhiteSpace(ConnectionInfo.ConnectionStringForMaster))
            {
                Status = DatabaseHelperStatusEnum.DontHaveConnectionInfo;
                return;
            }
            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionInfo.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    Status = DatabaseHelperStatusEnum.CelsusDatabaseReachable;
                }
                logger.Trace($"Connection opened to Celsus database.");

            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 4060)
                {
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                    Status = DatabaseHelperStatusEnum.CelsusDatabaseLoginFailed;
                }
                else
                {
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                    Status = DatabaseHelperStatusEnum.CelsusDatabaseIsNotReachable;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error occured connecting Celsus database.");
                Status = DatabaseHelperStatusEnum.CelsusDatabaseIsNotReachable;
            }
        }

        public async Task<bool> Upgrade()
        {
            upgradeEngine = DeployChanges.To
                            .SqlDatabase(ConnectionInfo.ConnectionString)
                            .WithScriptsEmbeddedInAssembly(this.GetType().Assembly)
                            .LogToConsole()
                            .Build();
            EnsureDatabase.For.SqlDatabase(ConnectionInfo.ConnectionString);
            var result = upgradeEngine.PerformUpgrade();
            await CheckAll();
            if (result.Successful == false)
            {
                throw result.Error;
            }
            return result.Successful;
        }
        public void CheckDatabaseVersion()
        {
            upgradeEngine = DeployChanges.To
                            .SqlDatabase(ConnectionInfo.ConnectionString)
                            .WithScriptsEmbeddedInAssembly(this.GetType().Assembly)
                            .LogToConsole()
                            .Build();

            IsUpgradeRequired = upgradeEngine.IsUpgradeRequired();
            if (IsUpgradeRequired.HasValue && IsUpgradeRequired.Value == false)
            {
                Status = DatabaseHelperStatusEnum.CelsusDatabaseVersionOk;
            }
            if (IsUpgradeRequired.HasValue && IsUpgradeRequired.Value == true)
            {
                Status = DatabaseHelperStatusEnum.CelsusDatabaseVersionOld;
            }
            //var tr=upgradeEngine.GetScriptsToExecute();
        }

    }
}
