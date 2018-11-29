//using Celsus.Types;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace Celsus.Client.Shared.Types
//{
//    public class SourceModel
//    {
//        public SourceDto SourceDto { get; set; }

//        ICommand editWorkflowdCommand;
//        public ICommand EditWorkflowsCommand
//        {
//            get
//            {
//                if (editWorkflowdCommand == null)
//                    editWorkflowdCommand = new RelayCommand(param => EditWorkflows(param), param => { return CanSelectFolder(param); });
//                return editWorkflowdCommand;
//            }
//        }

//        private bool CanSelectFolder(object obj)
//        {
//            return true;
//        }

//        private void EditWorkflows(object obj)
//        {
//            var t = new WorkflowManagementControl();
//            t.PrepareForNew();
//            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(t);
//        }

//    }
//}
