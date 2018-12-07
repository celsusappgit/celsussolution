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
    public class SourceModel
    {
        public SourceDto SourceDto { get; set; }

        ICommand editWorkflowdCommand;
        public ICommand EditWorkflowsCommand
        {
            get
            {
                if (editWorkflowdCommand == null)
                    editWorkflowdCommand = new RelayCommand(param => EditWorkflows(param), param => { return CanEditWorkflows(param); });
                return editWorkflowdCommand;
            }
        }

        private bool CanEditWorkflows(object obj)
        {
            return true;
        }

        private void EditWorkflows(object obj)
        {
            var newWorkflowManagementControl = new WorkflowManagementControl();
            newWorkflowManagementControl.PrepareForExisting(SourceDto.Id);
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(newWorkflowManagementControl);
        }

        ICommand editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (editCommand == null)
                    editCommand = new RelayCommand(param => Edit(param), param => { return CanEdit(param); });
                return editCommand;
            }
        }

        private bool CanEdit(object obj)
        {
            return SourceDto.ServerId == ComputerHelper.Instance.ServerId;
        }

        private void Edit(object obj)
        {
            var newSourceItemControl = new SourceItemControl();
            newSourceItemControl.PrepareForExisting(SourceDto.Id);
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(newSourceItemControl);
        }

    }
}
