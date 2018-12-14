using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.Client.Types.Models;
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
using System.Windows.Threading;

namespace Celsus.Client.Controls.Management.Sources
{
    public class WorkflowManagementControlModel : BaseModel
    {
        public int SourceId { get; internal set; }

        public string SourceName
        {
            get
            {
                var source = Repo.Instance.Sources.SingleOrDefault(x => x.SourceDto.Id == SourceId);
                if (source != null)
                {
                    return $"({source.SourceDto.Name})";
                }
                return string.Empty;
            }

        }

        private ICollectionView workflowsView;
        public ICollectionView Workflows
        {
            get
            {
                if (workflowsView != null)
                {
                    return workflowsView;
                }
                workflowsView = new CollectionViewSource() { Source = Repo.Instance.Workflows }.View;
                workflowsView.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Ascending, PropertyName = "WorkflowDto.OrderNo" });
                workflowsView.Filter = new Predicate<object>(Contains);
                var collectionViewLiveShaping = workflowsView as ICollectionViewLiveShaping;
                if (collectionViewLiveShaping.CanChangeLiveSorting)
                {

                    collectionViewLiveShaping.LiveSortingProperties.Add("WorkflowDto.OrderNo");
                    collectionViewLiveShaping.IsLiveSorting = true;
                }
                return workflowsView;
            }
        }

        public bool Contains(object de)
        {
            WorkflowModel workflowModel = de as WorkflowModel;
            return (workflowModel.WorkflowDto.SourceId == SourceId);
        }

        ICommand addNewWorkflowCommand;
        public ICommand AddNewWorkflowCommand
        {
            get
            {
                if (addNewWorkflowCommand == null)
                    addNewWorkflowCommand = new RelayCommand(param => AddNewWorkflow(param), param => { return true; });
                return addNewWorkflowCommand;
            }
        }

        private void AddNewWorkflow(object obj)
        {
            var newWorkflowItemControl = new WorkflowItemControl();
            newWorkflowItemControl.PrepareForNew(SourceId);
            ((App.Current.MainWindow as FirstWindow).DataContext as FirstWindowModel).OpenTabItem(newWorkflowItemControl);
        }
    }
    public partial class WorkflowManagementControl : UserControl, IItemEditControl
    {

        public int? ItemId
        {
            get
            {
                //if ((DataContext as WorkflowManagementControlModel).IsNew)
                //{
                //    return null;
                //}
                return (DataContext as WorkflowManagementControlModel).SourceId;
            }
        }
        public WorkflowManagementControl()
        {
            InitializeComponent();
            DataContext = new WorkflowManagementControlModel();
        }

        internal void PrepareForExisting(int sourceId)
        {
            (DataContext as WorkflowManagementControlModel).SourceId = sourceId;
        }
    }
}
