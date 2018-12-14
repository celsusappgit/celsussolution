using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Windows;

namespace Celsus.Client.Shared.Types
{
    public class ConnectionInfo : BaseModel
    {
        SqlConnectionStringBuilder sqlConnectionStringBuilder = null;

        public ConnectionInfo()
        {
            sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
        }

        public string MachineName
        {
            get
            {
                string nameToCheck = "";
                if (Server.Contains(@"\"))
                {
                    nameToCheck = Server.Split('\\').FirstOrDefault();
                }
                else
                {
                    nameToCheck = Server;
                }
                return nameToCheck;
            }
        }

        public bool MachineNameIsAnIPAddress
        {
            get
            {
                if (IPAddress.TryParse(MachineName, out IPAddress ipAddress))
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsServerCurrentMachine
        {
            get
            {
                string nameToCheck = MachineName;
                if (
                        string.IsNullOrWhiteSpace(nameToCheck) == false &&
                        (
                            nameToCheck.Equals(".", StringComparison.InvariantCultureIgnoreCase) ||
                            nameToCheck.Equals("local", StringComparison.InvariantCultureIgnoreCase) ||
                            nameToCheck.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ||
                            nameToCheck.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public string Server
        {
            get
            {
                return sqlConnectionStringBuilder.DataSource;
            }
            set
            {
                sqlConnectionStringBuilder.DataSource = value;
                NotifyPropertyChanged(() => Server);
                NotifyPropertyChanged(() => IsOK);
                NotifyPropertyChanged(() => ServerErrorVisibility);
                NotifyPropertyChanged(() => IsServerCurrentMachine);
                NotifyPropertyChanged(() => MachineName);
                NotifyPropertyChanged(() => MachineNameIsAnIPAddress);
            }
        }

        public Visibility ServerErrorVisibility
        {
            get
            {
                if (IntegratedSecurity == false)
                {
                    if (string.IsNullOrWhiteSpace(Server))
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }
        public bool IntegratedSecurity
        {
            get
            {
                return sqlConnectionStringBuilder.IntegratedSecurity;
            }
            set
            {
                sqlConnectionStringBuilder.IntegratedSecurity = value;
                NotifyPropertyChanged(() => IntegratedSecurity);
                NotifyPropertyChanged(() => UserIDEnabled);
                NotifyPropertyChanged(() => GrdUsernameVisibility);
                NotifyPropertyChanged(() => GrdPasswordVisibility);
                NotifyPropertyChanged(() => PasswordEnabled);
                NotifyPropertyChanged(() => IsOK);
                NotifyPropertyChanged(() => PasswordErrorVisibility);
                NotifyPropertyChanged(() => UserIDErrorVisibility);
            }
        }

        public int ConnectTimeout
        {
            get
            {
                return sqlConnectionStringBuilder.ConnectTimeout;
            }
            set
            {
                sqlConnectionStringBuilder.ConnectTimeout = value;
                NotifyPropertyChanged(() => ConnectTimeout);
            }
        }

        public bool UserIDEnabled
        {
            get
            {
                return IntegratedSecurity == false;
            }
        }

        public Visibility GrdUsernameVisibility
        {
            get
            {
                return IntegratedSecurity == false ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility GrdPasswordVisibility
        {
            get
            {
                return IntegratedSecurity == false ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool PasswordEnabled
        {
            get
            {
                return IntegratedSecurity == false;
            }
        }

        public string UserID
        {
            get
            {
                return sqlConnectionStringBuilder.UserID;
            }
            set
            {
                sqlConnectionStringBuilder.UserID = value;
                NotifyPropertyChanged(() => UserID);
                NotifyPropertyChanged(() => IsOK);
                NotifyPropertyChanged(() => UserIDErrorVisibility);
            }
        }

        public Visibility UserIDErrorVisibility
        {
            get
            {
                if (IntegratedSecurity == false)
                {
                    if (string.IsNullOrWhiteSpace(UserID))
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }
        public string Password
        {
            get
            {
                return sqlConnectionStringBuilder.Password;
            }
            set
            {
                sqlConnectionStringBuilder.Password = value;
                NotifyPropertyChanged(() => Password);
                NotifyPropertyChanged(() => IsOK);
                NotifyPropertyChanged(() => PasswordErrorVisibility);
            }
        }

        public Visibility PasswordErrorVisibility
        {
            get
            {
                if (IntegratedSecurity == false)
                {
                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }
        public string InitialCatalog
        {
            get
            {
                return sqlConnectionStringBuilder.InitialCatalog;
            }
            set
            {
                sqlConnectionStringBuilder.InitialCatalog = value;
                NotifyPropertyChanged(() => InitialCatalog);
                NotifyPropertyChanged(() => IsOK);
            }
        }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server) || (IntegratedSecurity == false && string.IsNullOrWhiteSpace(UserID) && string.IsNullOrWhiteSpace(Password)))
                {
                    return null;
                }
                var oldInitialCatalog = sqlConnectionStringBuilder.InitialCatalog;
                sqlConnectionStringBuilder.InitialCatalog = "Celsus";
                var csDummy = sqlConnectionStringBuilder.ConnectionString;
                sqlConnectionStringBuilder.InitialCatalog = oldInitialCatalog;
                return csDummy;
            }
            set
            {
                sqlConnectionStringBuilder.ConnectionString = value;
                //NotifyPropertyChanged(() => ConnectionString);
                //NotifyPropertyChanged(() => InitialCatalog);
                //NotifyPropertyChanged(() => Password);
                //NotifyPropertyChanged(() => UserID);
                //NotifyPropertyChanged(() => Server);
                //NotifyPropertyChanged(() => IntegratedSecurity);
                //NotifyPropertyChanged(() => IsOK);
                NotifyPropertyChanged("");
            }
        }

        public string ConnectionStringWithSmallTimeOut
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server) || (IntegratedSecurity == false && string.IsNullOrWhiteSpace(UserID) && string.IsNullOrWhiteSpace(Password)))
                {
                    return null;
                }
                var oldConnectTimeout = sqlConnectionStringBuilder.ConnectTimeout;
                sqlConnectionStringBuilder.ConnectTimeout = 3;
                var csDummy = sqlConnectionStringBuilder.ConnectionString;
                sqlConnectionStringBuilder.ConnectTimeout = oldConnectTimeout;
                return csDummy;
            }
            set
            {
                sqlConnectionStringBuilder.ConnectionString = value;
                NotifyPropertyChanged(() => ConnectionString);
                NotifyPropertyChanged(() => InitialCatalog);
                NotifyPropertyChanged(() => Password);
                NotifyPropertyChanged(() => UserID);
                NotifyPropertyChanged(() => IntegratedSecurity);
                NotifyPropertyChanged(() => IsOK);
            }
        }

        public string ConnectionStringForMaster
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server) || (IntegratedSecurity == false && string.IsNullOrWhiteSpace(UserID) && string.IsNullOrWhiteSpace(Password)))
                {
                    return null;
                }
                var oldInitialCatalog = sqlConnectionStringBuilder.InitialCatalog;
                sqlConnectionStringBuilder.InitialCatalog = "Master";
                var csDummy = sqlConnectionStringBuilder.ConnectionString;
                sqlConnectionStringBuilder.InitialCatalog = oldInitialCatalog;
                return csDummy;
            }
        }

        public string ConnectionStringForMasterWithSmallTimeOut
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server) || (IntegratedSecurity == false && string.IsNullOrWhiteSpace(UserID) && string.IsNullOrWhiteSpace(Password)))
                {
                    return null;
                }
                var oldConnectTimeout = sqlConnectionStringBuilder.ConnectTimeout;
                var oldInitialCatalog = sqlConnectionStringBuilder.InitialCatalog;
                sqlConnectionStringBuilder.InitialCatalog = "Master";
                sqlConnectionStringBuilder.ConnectTimeout = 3;
                var csDummy = sqlConnectionStringBuilder.ConnectionString;
                sqlConnectionStringBuilder.InitialCatalog = oldInitialCatalog;
                sqlConnectionStringBuilder.ConnectTimeout = oldConnectTimeout;
                return csDummy;
            }
        }

        public bool IsOK
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server) == false)
                {
                    if (IntegratedSecurity)
                    {
                        return true;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(UserID) == false)
                        {
                            if (string.IsNullOrWhiteSpace(Password) == false)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

    }
}
