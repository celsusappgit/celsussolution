using Celsus.Client.Controls.Management.Sources;
using Celsus.Client.Shared.Types;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Celsus.Client.Types.Models
{
    public class WorkflowModel
    {
        public WorkflowDto WorkflowDto { get; set; }

        ICommand editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (editCommand == null)
                    editCommand = new RelayCommand(param => Edit(param), param => { return true; });
                return editCommand;
            }
        }

        private void Edit(object param)
        {
            var newWorkflowItemControl = new WorkflowItemControl();
            newWorkflowItemControl.PrepareForExisting(WorkflowDto.Id);
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(newWorkflowItemControl);
        }
    }
}
