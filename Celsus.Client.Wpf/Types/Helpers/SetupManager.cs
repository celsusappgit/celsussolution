using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Wpf.Types
{
    public class SetupManagerLocator
    {
        public SetupManager Main
        {
            get { return SetupManager.Instance; }
        }

    }
    public class SetupManager : ModelBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static Lazy<SetupManager> _lazyInstance = new Lazy<SetupManager>(() => new SetupManager());
        public static SetupManager Instance
        {
            get
            {
                if (_lazyInstance == null)
                {
                    _lazyInstance = new Lazy<SetupManager>(() => new SetupManager());
                }
                return _lazyInstance.Value;
            }
        }

        //public static async Task<string> CheckRoles()
        //{
        //    if (ConnectionSettingExists() == false)
        //    {
        //        return "NoConnectionSetting";
        //    }
        //    if (await CheckDatabase() == false)
        //    {
        //        return "CannotReachDatabase";
        //    }
        //    var serverRoles = await GetRoles();
        //    if (serverRoles == null || serverRoles.Count == 0)
        //    {
        //        return "NoRoles";
        //    }
        //    var thisServerRoles = serverRoles.Where(x => x.IsActive && x.ServerId == GetServerId());
        //    if (thisServerRoles.Count() == 0)
        //    {
        //        return "NoRolesForThisServer";
        //    }
        //    return string.Join(",", thisServerRoles.Select(x => x.ServerRoleEnum.ToString()).ToArray());
        //}

        //public static async Task<List<ServerRoleDto>> GetRoles()
        //{
        //    var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
        //    try
        //    {
        //        using (var context = new SqlDbContext(connectionString))
        //        {
        //            return await context.ServerRoles.Where(x => x.IsActive == true).ToListAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, $"Error occured connecting SQL Server.");
        //        return null;
        //    }
        //}

        public bool IsConnectionSettingOk
        {
            get
            {
                var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return false;
                }
                try
                {
                    new SqlConnectionStringBuilder(connectionString);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsImageMagickInstalled
        {
            get
            {
                int count = 0;

                var ImageMagickPath = SettingsManager.Instance.GetSetting<string>(SettingName.ImageMagickPath);
                if (string.IsNullOrWhiteSpace(ImageMagickPath) == false)
                {
                    if (Directory.Exists(ImageMagickPath))
                    {
                        count++;
                    }
                }

                return (count == 1);
            }
        }

        public bool IsTesseractInstalled
        {
            get
            {
                int count = 0;

                var tesseractPath = SettingsManager.Instance.GetSetting<string>(SettingName.TesseractPath);
                if (string.IsNullOrWhiteSpace(tesseractPath) == false)
                {
                    if (Directory.Exists(tesseractPath))
                    {
                        count++;
                    }
                }

                return (count == 1);
            }
        }

        public bool IsXPdfToolsInstalled
        {
            get
            {
                int count = 0;

                var xPdfToolsPath = SettingsManager.Instance.GetSetting<string>(SettingName.XPdfToolsPath);
                if (string.IsNullOrWhiteSpace(xPdfToolsPath) == false)
                {
                    if (Directory.Exists(xPdfToolsPath))
                    {
                        count++;
                    }
                }

                return (count == 1);
            }
        }
        public bool IsOCRInstalled
        {
            get
            {
                return (IsImageMagickInstalled && IsTesseractInstalled && IsXPdfToolsInstalled);
            }
        }

        internal void InvalidateConnectionString()
        {
            NotifyPropertyChanged(() => IsConnectionSettingOk);
            CheckDatabase();
            InvalidateAllRoles();
            
        }

        List<ServerRoleDto> allRoles;
        public List<ServerRoleDto> AllRoles
        {
            get
            {
                return allRoles;
            }
            set
            {
                if (Equals(value, allRoles)) return;
                allRoles = value;
                NotifyPropertyChanged(() => AllRoles);
                NotifyPropertyChanged(() => IsDatabaseRoleExistsMoreThanOne);
                NotifyPropertyChanged(() => IsIndexerRoleExistsMoreThanOne);
                NotifyPropertyChanged(() => IsDatabaseRoleExists);
                NotifyPropertyChanged(() => IsIndexerRoleExists);
                NotifyPropertyChanged(() => ThisServerRoles);
                NotifyPropertyChanged(() => DatabaseRole);
                NotifyPropertyChanged(() => IndexerRole);
                NotifyPropertyChanged(() => IsOneDatabaseRoleExists);
                NotifyPropertyChanged(() => IsOneIndexerRoleExists);
            }
        }

        public List<ServerRoleDto> ThisServerRoles
        {
            get
            {
                if (allRoles == null)
                {
                    return null;
                }
                return AllRoles.Where(x => x.IsActive && x.ServerId == GetServerId()).ToList();
            }
        }

        public bool IsDatabaseRoleExists
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Database) > 0;
            }
        }
        public bool IsIndexerRoleExists
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Indexer) > 0;
            }
        }

        public bool IsOneDatabaseRoleExists
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Database) == 1;
            }
        }
        public bool IsOneIndexerRoleExists
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Indexer) == 1;
            }
        }

        public ServerRoleDto DatabaseRole
        {
            get
            {
                if (AllRoles == null)
                {
                    return null;
                }
                return AllRoles.OrderByDescending(x=>x.Id).FirstOrDefault(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Database);
            }
        }

        public ServerRoleDto IndexerRole
        {
            get
            {
                if (AllRoles == null)
                {
                    return null;
                }
                return AllRoles.OrderByDescending(x => x.Id).FirstOrDefault(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Indexer);
            }
        }

        public bool IsDatabaseRoleExistsMoreThanOne
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Database) > 1;
            }
        }

        public bool IsIndexerRoleExistsMoreThanOne
        {
            get
            {
                if (AllRoles == null)
                {
                    return false;
                }
                return AllRoles.Count(x => x.IsActive && x.ServerRoleEnum == ServerRoleEnum.Indexer) > 1;
            }
        }

        internal async void InvalidateAllRoles()
        {
            if (IsConnectionSettingOk == false)
            {
                AllRoles = null;
                NotifyPropertyChanged(() => AllRoles);
                NotifyPropertyChanged(() => ThisServerRoles);
            }
            else
            {
                var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
                try
                {
                    using (var context = new SqlDbContext(connectionString))
                    {
                        AllRoles = await context.ServerRoles.Where(x => x.IsActive == true).ToListAsync();
                    }
                    CanGetRoles = true;
                }

                catch (Exception ex)
                {
                    CanGetRoles = false;
                    logger.Error(ex, $"Error occured connecting SQL Server.");
                }
                NotifyPropertyChanged(() => AllRoles);
                NotifyPropertyChanged(() => ThisServerRoles);
            }
        }

        internal void InvalidateIsOCRInstalled()
        {
            NotifyPropertyChanged(() => IsOCRInstalled);
        }

        //public static bool ConnectionSettingExists()
        //{
        //    var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
        //    return !string.IsNullOrWhiteSpace(connectionString);
        //}

        //public static bool CheckOcrComponent(SettingName settingName)
        //{
        //    var path = SettingsManager.Instance.GetSetting<string>(settingName);
        //    if (string.IsNullOrWhiteSpace(path) == false)
        //    {
        //        if (Directory.Exists(path))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}


        bool canReachDatabase;
        public bool CanReachDatabase
        {
            get
            {
                return canReachDatabase;
            }
            set
            {
                if (Equals(value, canReachDatabase)) return;
                canReachDatabase = value;
                NotifyPropertyChanged(() => CanReachDatabase);
            }
        }
        private async void CheckDatabase()
        {
            logger.Trace($"Check Database started.");
            if (IsConnectionSettingOk == false)
            {
                CanReachDatabase = false;
                return;
            }

            var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                }
                logger.Trace($"Connection opened to Celsus database.");
                CanReachDatabase = true;
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 4060)
                {
                    logger.Trace(sqlException, $"Login failed to Celsus database.");
                }
                else
                {
                    logger.Error(sqlException, $"Celsus database is not reachable.");
                }
                CanReachDatabase = false;
            }
            catch (Exception ex)
            {

                logger.Error(ex, $"Error occured connecting Celsus database.");
                CanReachDatabase = false;
            }
            finally
            {
            }

        }

        //public static async Task<bool> CheckDatabase()
        //{
        //    var connectionString = SettingsManager.Instance.GetSetting<string>(SettingName.ConnectionString);
        //    return await CheckDatabase(connectionString);
        //}

        public string CheckSerial(string newSerial, out Celsus.Types.NonDatabase.LicenseData licenseInformation)
        {

            if (string.IsNullOrWhiteSpace(newSerial))
            {
                licenseInformation = null;
                return "NoLicense";
            }

            byte[] certPubKeyData = null;
            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (MemoryStream _mem = new MemoryStream())
            {
                _assembly.GetManifestResourceStream("Celsus.Client.Wpf.Resources.Certificates.EbdysCertPublic.cer").CopyTo(_mem);
                certPubKeyData = _mem.ToArray();
            }

            try
            {
                licenseInformation = SignHelper.VerifyLicense(certPubKeyData, newSerial);
                if (licenseInformation != null)
                {
                    if (licenseInformation.ExpireDate < DateTime.UtcNow || ServerId != licenseInformation.ServerId)
                    {
                        return "LicenseError";
                    }
                    else
                    {
                        return "LicenseOk";
                    }
                }
                else
                {
                    return "LicenseError";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"LicenseError");
                licenseInformation = null;
                return "LicenseError";
            }
        }

        public static ServiceController CheckService()
        {
            ServiceController[] services = null;
            try
            {
                services = ServiceController.GetServices();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"CheckService");
                return null;
            }
            var service = services.FirstOrDefault(s => s.DisplayName == "CelsusService");
            return service;
        }

        private static string GetProcessorId()
        {
            try
            {
                var managementObjectSearcher = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                var managementObjectCollection = managementObjectSearcher.Get();
                string id = string.Empty;
                foreach (var managementObject in managementObjectCollection)
                {
                    id = managementObject["ProcessorId"].ToString();
                    break;
                }
                return id;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetProcessorId");
                return string.Empty;
            }

        }

        public string ServerId
        {
            get
            {
                return GetServerId();
            }
        }
        private string GetMotherboardId()
        {
            try
            {
                ManagementObjectSearcher _mbs = new ManagementObjectSearcher("Select SerialNumber From Win32_BaseBoard");
                ManagementObjectCollection _mbsList = _mbs.Get();
                string _id = string.Empty;
                foreach (ManagementObject _mo in _mbsList)
                {
                    _id = _mo["SerialNumber"].ToString();
                    break;
                }

                return _id;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetMotherboardId");
                return string.Empty;
            }
        }
        private string GetServerName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetServerName");
                return string.Empty;
            }
        }
        public string IPAddress
        {
            get
            {
                try
                {
                    IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                    if (localIPs == null || localIPs.Count() == 0)
                    {
                        return null;
                    }
                    return string.Join(", ", localIPs.Select(x => x.MapToIPv4().ToString()));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"GetIPAddress");
                    return string.Empty;
                }
                return null;
            }
        }

        bool canGetRoles;
        public bool CanGetRoles
        {
            get
            {
                return canGetRoles;
            }
            private set
            {
                if (Equals(value, canGetRoles)) return;
                canGetRoles = value;
                NotifyPropertyChanged(() => CanGetRoles);
            }
        }

        private string GetServerId()
        {
            var processorId = GetProcessorId();
            var motherboardId = GetMotherboardId();
            var servername = GetServerName();

            if (string.IsNullOrWhiteSpace(processorId) || string.IsNullOrWhiteSpace(motherboardId) || string.IsNullOrWhiteSpace(servername))
            {
                return null;
            }

            string id = string.Concat(processorId, motherboardId, servername);
            byte[] idAsByte = Encoding.UTF8.GetBytes(id);
            byte[] _checksum = null;
            using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                _checksum = md5CryptoServiceProvider.ComputeHash(idAsByte);
            }
            string _part1Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 0));
            string _part2Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 4));
            string _part3Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 8));
            string _part4Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 12));

            return $"{_part1Id}-{_part2Id}-{_part3Id}-{_part4Id}";
        }
    }

}
