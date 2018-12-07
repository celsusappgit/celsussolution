using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace Celsus.Client.Controls.Setup.Database
{
    public class ConnectionStringControlModel : BaseModel
    {
        ConnectionInfo connectionInfo;
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

        object status;
        public object Status
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
                NotifyPropertyChanged(() => StatusVisibility);
            }
        }

        public Visibility StatusVisibility
        {
            get
            {
                return Status == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (Equals(value, isBusy)) return;
                isBusy = value;
                NotifyPropertyChanged(() => IsBusy);
            }
        }

        ICommand closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (closeWindowCommand == null)
                    closeWindowCommand = new RelayCommand(param => CloseWindow(param), param => { return true; });
                return closeWindowCommand;
            }
        }

        private void CloseWindow(object param)
        {
            RadWindowManager.Current.GetWindows().Last().Close();
        }

        ICommand importSettingsFileCommand;
        public ICommand ImportSettingsFileCommand
        {
            get
            {
                if (importSettingsFileCommand == null)
                    importSettingsFileCommand = new RelayCommand(param => ImportSettingsFile(param), param => { return true; });
                return importSettingsFileCommand;
            }
        }

        private void ImportSettingsFile(object param)
        {
            var encryptedInfo = string.Empty;
            OpenFileDialog myDialog = new OpenFileDialog
            {
                Filter = LocHelper.GetWord("CelsusInfoFiles") + " (*.clsinfo)|*.clsinfo",
                CheckFileExists = true,
                Multiselect = false
            };
            if (myDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(myDialog.FileName))
                {
                    encryptedInfo= System.IO.File.ReadAllText(myDialog.FileName);
                }
            }
            if (string.IsNullOrWhiteSpace(encryptedInfo) == false)
            {
                var decryptedInfo = EncryptionHelper.Decrypt(encryptedInfo);
                if (string.IsNullOrWhiteSpace(decryptedInfo) == false)
                {
                    ConnectionInfo.ConnectionString = decryptedInfo;
                    CheckConnection(null);
                }
            }
        }

        ICommand checkConnectionCommand;
        public ICommand CheckConnectionCommand
        {
            get
            {
                if (checkConnectionCommand == null)
                    checkConnectionCommand = new RelayCommand(param => CheckConnection(param), param => { return true; });
                return checkConnectionCommand;
            }
        }

        private async void CheckConnection(object param)
        {

            IsBusy = true;

            try
            {
                using (var sqlConnection = new SqlConnection(ConnectionInfo.ConnectionStringForMasterWithSmallTimeOut))
                {
                    await sqlConnection.OpenAsync();
                    Status = $"Successfully connected to SQL Server.".ConvertToBindableText();
                    SettingsHelper.Instance.ConnectionString = ConnectionInfo.ConnectionString;
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 4060)
                {
                    Status = $"Login failed to SQL Server.".ConvertToBindableText();
                }
                else
                {
                    Status = $"SQL Server. is not reachable.".ConvertToBindableText();
                }
            }
            catch (Exception ex)
            {
                Status = $"Error occured connecting SQL Server.".ConvertToBindableText();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ConnectionStringControlModel()
        {
            ConnectionInfo = new ConnectionInfo
            {
                ConnectionString = DatabaseHelper.Instance.ConnectionInfo.ConnectionStringForMaster
            };
        }

    }
    public partial class ConnectionStringControl : UserControl
    {
        public ConnectionStringControl()
        {
            InitializeComponent();
            DataContext = new ConnectionStringControlModel();
        }
    }
}
