using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.Client.Types.Models;
using System;
using System.Collections.Generic;
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

namespace Celsus.Client.Controls.Management.Sources
{
    public class SourcesManagementControlModel : BaseModel<SourcesManagementControlModel>
    {
        public Repo Repo { get { return Repo.Instance; } }



        ICommand refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (refreshCommand == null)
                    refreshCommand = new RelayCommand(param => Refresh(param), param => { return true; });
                return refreshCommand;
            }
        }

        private async void Refresh(object param)
        {
            await Repo.Instance.RefreshSources();
        }

        ICommand helpForDisabledAddNewSourceCommand;
        public ICommand HelpForDisabledAddNewSourceCommand
        {
            get
            {
                if (helpForDisabledAddNewSourceCommand == null)
                    helpForDisabledAddNewSourceCommand = new RelayCommand(param => HelpForDisabledAddNewSource(param), param => { return true; });
                return helpForDisabledAddNewSourceCommand;
            }
        }

        private void HelpForDisabledAddNewSource(object param)
        {
            string target = "https://celsus.gitbook.io/project/administration/sources#creating-new-source";
            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {

                }
                logger.Error(noBrowser, "Error in Help");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in Help");
            }

        }

        public Visibility HelpForDisabledAddNewSourceVisibility
        {
            get
            {
                return RolesHelper.Instance.IsIndexerRoleThisComputer ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        ICommand addNewSourceCommand;
        public ICommand AddNewSourceCommand
        {
            get
            {
                if (addNewSourceCommand == null)
                    addNewSourceCommand = new RelayCommand(param => AddNewSource(param), param => { return RolesHelper.Instance.IsIndexerRoleThisComputer; });
                return addNewSourceCommand;
            }
        }

        private void AddNewSource(object obj)
        {
            var t = new SourceItemControl();
            t.PrepareForNew();
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(t);
        }
    }
    public partial class SourcesManagementControl : UserControl
    {
        public SourcesManagementControl()
        {
            InitializeComponent();
            DataContext = SourcesManagementControlModel.Instance;
        }

    }
}
