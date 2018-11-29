using Celsus.Client.Wpf.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;

namespace Celsus.Client.Wpf.Controls.Management
{
        public class ModelObject : INotifyPropertyChanged
        {
            ConnectionInfo connectionInfo = null;

            public ModelObject()
            {
                connectionInfo = new ConnectionInfo();
            }
            public ConnectionInfo ConnectionInfo
            {
                get
                {
                    return connectionInfo;
                }
                set
                {
                    if (Equals(value, connectionInfo)) return;
                    connectionInfo = value;
                    NotifyPropertyChanged(() => ConnectionInfo);
                }
            }

            public void AddMode(string newMode)
            {
                if (Mode.Contains(newMode) == false)
                {
                    Mode.Add(newMode);
                    NotifyPropertyChanged(() => CheckSQLServerEnabled);
                    NotifyPropertyChanged(() => InstallSQLServerEnabled);
                    NotifyPropertyChanged(() => InstallCelsusDatabaseEnabled);
                    NotifyPropertyChanged(() => CheckCelsusDatabaseEnabled);
                    NotifyPropertyChanged(() => CheckSQLServerVisibility);
                    NotifyPropertyChanged(() => CheckSQLServerVisibilityOk);
                    NotifyPropertyChanged(() => CheckSQLServerVisibilityError);
                    NotifyPropertyChanged(() => InstallSQLServerVisibility);
                    NotifyPropertyChanged(() => InstallCelsusDatabaseVisibility);
                    NotifyPropertyChanged(() => CheckCelsusDatabaseVisibility);
                }
            }

            internal void RemoveMode(string newMode)
            {
                if (Mode.Contains(newMode) == true)
                {
                    Mode.Remove(newMode);
                    NotifyPropertyChanged(() => CheckSQLServerEnabled);
                    NotifyPropertyChanged(() => InstallSQLServerEnabled);
                    NotifyPropertyChanged(() => InstallCelsusDatabaseEnabled);
                    NotifyPropertyChanged(() => CheckCelsusDatabaseEnabled);
                    NotifyPropertyChanged(() => CheckSQLServerVisibility);
                    NotifyPropertyChanged(() => CheckSQLServerVisibilityOk);
                    NotifyPropertyChanged(() => CheckSQLServerVisibilityError);
                    NotifyPropertyChanged(() => InstallSQLServerVisibility);
                    NotifyPropertyChanged(() => InstallCelsusDatabaseVisibility);
                    NotifyPropertyChanged(() => CheckCelsusDatabaseVisibility);

                }
            }

            List<string> mode = new List<string>() { "Null" };
            public List<string> Mode
            {
                get
                {
                    return mode;
                }
                private set
                {
                    if (Equals(value, mode)) return;
                    mode = value;
                    NotifyPropertyChanged(() => Mode);

                }
            }

            public bool CheckCelsusDatabaseEnabled
            {
                get
                {
                    return (Mode.Contains("SQLOK") && !Mode.Contains("Working"));
                }
            }

            public Visibility CheckCelsusDatabaseVisibility
            {
                get
                {
                    return CheckCelsusDatabaseEnabled ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            public bool CheckSQLServerEnabled
            {
                get
                {
                    return (Mode.Contains("Null") && !Mode.Contains("Working"));
                }
            }

            public Visibility CheckSQLServerVisibility
            {
                get
                {
                    return (Mode.Contains("Null")) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public Visibility CheckSQLServerVisibilityOk
            {
                get
                {
                    return Mode.Contains("SQLOK") ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public Visibility CheckSQLServerVisibilityError
            {
                get
                {
                    return Mode.Contains("NeedSqlInstall") ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public bool InstallSQLServerEnabled
            {
                get
                {
                    return (Mode.Contains("NeedSqlInstall") && !Mode.Contains("Working"));
                }
            }

            public Visibility InstallSQLServerVisibility
            {
                get
                {
                    return InstallSQLServerEnabled ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public bool InstallCelsusDatabaseEnabled
            {
                get
                {
                    return (Mode.Contains("NeedDatabaseInstall") && !Mode.Contains("Working"));
                }
            }

            public Visibility InstallCelsusDatabaseVisibility
            {
                get
                {
                    return InstallCelsusDatabaseEnabled ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public Visibility InstallCelsusDatabase
            {
                get
                {
                    return InstallCelsusDatabaseEnabled ? Visibility.Visible : Visibility.Collapsed;
                }
            }


            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;
            protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
            {
                var memberExpression = (MemberExpression)exp.Body;
                string propertyName = memberExpression.Member.Name;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }



            #endregion
        }


}
