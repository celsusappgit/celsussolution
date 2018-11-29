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

        ICommand addNewSourceCommand;
        public ICommand AddNewSourceCommand
        {
            get
            {
                if (addNewSourceCommand == null)
                    addNewSourceCommand = new RelayCommand(param => AddNewSource(param), param => { return true; });
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
