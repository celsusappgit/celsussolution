using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Celsus.Client.Wpf.Types
{
    public class ConnectionInfo : INotifyPropertyChanged
    {
        SqlConnectionStringBuilder sqlConnectionStringBuilder = null;

        public ConnectionInfo()
        {
            sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
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
                NotifyPropertyChanged(() => PasswordEnabled);
                NotifyPropertyChanged(() => IsOK);
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
                return sqlConnectionStringBuilder.ConnectionString;
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


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
