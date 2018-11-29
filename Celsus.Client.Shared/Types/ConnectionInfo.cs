﻿using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Windows;

namespace Celsus.Client.Shared.Types
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
                NotifyPropertyChanged(() => ServerErrorVisibility);
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
                NotifyPropertyChanged(() => ConnectionString);
                NotifyPropertyChanged(() => InitialCatalog);
                NotifyPropertyChanged(() => Password);
                NotifyPropertyChanged(() => UserID);
                NotifyPropertyChanged(() => IntegratedSecurity);
                NotifyPropertyChanged(() => IsOK);
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


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}