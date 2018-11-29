using Celsus.Client.Controls.Setup.Database;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Telerik.Windows.Controls.Navigation;

namespace Celsus.Client.Controls.Setup
{
    public class ManageRolesControlModel : BaseModel<ManageRolesControlModel>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;

        #endregion
        public DatabaseHelper DatabaseHelper { get { return DatabaseHelper.Instance; } }

        public RolesHelper RolesHelper { get { return RolesHelper.Instance; } }

        public Visibility CollapsedButVisibleInDesign
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(new Button()))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        ICommand addDatabaseRoleCommand;
        public ICommand AddDatabaseRoleCommand
        {
            get
            {
                if (addDatabaseRoleCommand == null)
                    addDatabaseRoleCommand = new RelayCommand(param => AddDatabaseRole(param), param => { return true; });
                return addDatabaseRoleCommand;
            }
        }

        private void AddDatabaseRole(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(DatabaseSetupMainControl));
        }

        ICommand addIndexerRoleCommand;
        public ICommand AddIndexerRoleCommand
        {
            get
            {
                if (addIndexerRoleCommand == null)
                    addIndexerRoleCommand = new RelayCommand(param => AddIndexerRole(param), param => { return true; });
                return addIndexerRoleCommand;
            }
        }

        private void AddIndexerRole(object obj)
        {
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(typeof(IndexerSetupMainControl));
        }

        ICommand enterConnectionParamatersCommand;
        public ICommand EnterConnectionParamatersCommand
        {
            get
            {
                if (enterConnectionParamatersCommand == null)
                    enterConnectionParamatersCommand = new RelayCommand(param => EnterConnectionParamaters(param), param => { return true; });
                return enterConnectionParamatersCommand;
            }
        }

        private void EnterConnectionParamaters(object obj)
        {
            var connectionStringControl = new ConnectionStringControl();
            RadWindow newWindow = new RadWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = (App.Current.MainWindow as FirstWindow),
                Content = connectionStringControl,
                SizeToContent = false,
                Width = (App.Current.MainWindow as FirstWindow).Width *0.8,
                Height = (App.Current.MainWindow as FirstWindow).Height * 0.8,
                Header = "ConnectionStringControl".ConvertToBindableText()
            };
            RadWindowInteropHelper.SetAllowTransparency(newWindow, false);
            newWindow.ShowDialog();

        }

        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }
                isInitted = true;
            }
        }
    }

    public partial class ManageRolesControl : UserControl
    {
        public ManageRolesControl()
        {
            InitializeComponent();
            DataContext = ManageRolesControlModel.Instance;
        }
    }
}
