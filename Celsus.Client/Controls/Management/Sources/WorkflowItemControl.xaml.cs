using Celsus.Client.Shared.Lex;
using Celsus.Client.Shared.Types;
using Celsus.Client.Shared.Types.Workflow;
using Celsus.Client.Types;
using Celsus.Client.Types.Models;
using Newtonsoft.Json;
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
    public class WorkflowItemControlModel : BaseModel
    {
        string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (Equals(value, title)) return;
                title = value;
                NotifyPropertyChanged(() => Title);
            }
        }

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

        bool isNew;
        public bool IsNew
        {
            get
            {
                return isNew;
            }
            set
            {
                if (Equals(value, isNew)) return;
                isNew = value;
                NotifyPropertyChanged(() => IsNew);
            }
        }
        int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                if (Equals(value, id)) return;

                id = value;
                Get();
                NotifyPropertyChanged(() => Id);
            }
        }

        public int OwnerSourceId { get; internal set; }

        string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (Equals(value, name)) return;
                name = value;
                NotifyPropertyChanged(() => Name);
                NotifyPropertyChanged(() => NameErrorVisibility);
                NotifyPropertyChanged(() => SaveCommand);
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

        public Visibility SelectedWorkflowErrorVisibility
        {
            get
            {
                return SelectedWorkflowInternalTypeName == null ? Visibility.Visible : Visibility.Hidden;
                //return SelectedWorkflowName == null ? Visibility.Visible : Visibility.Hidden;
                //return SelectedWorkflow == null ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility NameErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public string FinalFileType
        {
            get
            {

                if (SelectedFileType == "Other")
                {
                    return CustomFileType;
                }
                else
                {
                    return SelectedFileType;
                }

            }
        }

        public List<MyWorkflowModel> Workflows
        {
            get
            {
                var iCodeWorkflow = typeof(ICodeWorkflow);
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => iCodeWorkflow.IsAssignableFrom(type));
                var allTypes = types.Where(type => type.GetAttributeValue((WorksonAttribute attribute) => attribute.FileType) == FinalFileType);
                var result = allTypes.Select(x => new MyWorkflowModel() { InternalType = x, Name = x.GetAttributeValue((LocKeyAttribute attribute) => attribute.LocKey) }).ToList();
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                //{
                //    NotifyPropertyChanged(() => SelectedWorkflow);
                //}));
                return result;

            }
        }

        //string selectedWorkflowName;
        //public string SelectedWorkflowName
        //{
        //    get
        //    {
        //        return selectedWorkflowName;
        //    }
        //    set
        //    {
        //        if (Equals(value, selectedWorkflowName)) return;
        //        selectedWorkflowName = value;
        //        NotifyPropertyChanged(() => SelectedWorkflowName);
        //    }
        //}

        string selectedWorkflowInternalTypeName;
        public string SelectedWorkflowInternalTypeName
        {
            get
            {
                return selectedWorkflowInternalTypeName;
            }
            set
            {
                if (Equals(value, selectedWorkflowInternalTypeName)) return;
                selectedWorkflowInternalTypeName = value;
                NotifyPropertyChanged(() => SelectedWorkflowInternalTypeName);
            }
        }

        string internalTypeParameters;
        public string InternalTypeParameters
        {
            get
            {
                return internalTypeParameters;
            }
            set
            {
                if (Equals(value, internalTypeParameters)) return;
                internalTypeParameters = value;
                NotifyPropertyChanged(() => InternalTypeParameters);
            }
        }

        //MyWorkflowModel selectedWorkflow;
        //public MyWorkflowModel SelectedWorkflow
        //{
        //    get
        //    {
        //        return selectedWorkflow;
        //    }
        //    set
        //    {
        //        if (Equals(value, selectedWorkflow)) return;
        //        selectedWorkflow = value;
        //        NotifyPropertyChanged(() => SelectedWorkflow);
        //        NotifyPropertyChanged(() => SelectedWorkflowErrorVisibility);
        //        NotifyPropertyChanged(() => SaveCommand);
        //        NotifyPropertyChanged(() => WorkflowOptions);
        //        NotifyPropertyChanged(() => SaveCommand);
        //    }
        //}

        public bool HasErrorsForOptions
        {
            get
            {
                return WorkflowOptions.Count(x => x.ErrorVisibility == Visibility.Visible) > 0;
            }
        }
        private List<MyOptionModel> workflowOptions = null;
        public List<MyOptionModel> WorkflowOptions
        {
            get
            {
                if (workflowOptions != null)
                {
                    return workflowOptions;
                }
                if (IsNew)
                {
                    if (SelectedWorkflowInternalTypeName != null)
                    {
                        var workflowSelected = Workflows.SingleOrDefault(x => x.InternalType.FullName == SelectedWorkflowInternalTypeName);
                        var workflowToRun = Activator.CreateInstance(workflowSelected.InternalType) as ICodeWorkflow;
                        var options = workflowToRun.GetOptionsList().Select(x => new MyOptionModel() { MyOption = x }).ToList();
                        workflowOptions = options;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(InternalTypeParameters) == false)
                    {
                        var myOptions = JsonConvert.DeserializeObject<List<MyOption>>(InternalTypeParameters);
                        var options = myOptions.Select(x => new MyOptionModel() { MyOption = x }).ToList();
                        workflowOptions = options;
                    }
                    else
                    {
                        var workflowSelected = Workflows.SingleOrDefault(x => x.InternalType.FullName == SelectedWorkflowInternalTypeName);
                        var workflowToRun = Activator.CreateInstance(workflowSelected.InternalType) as ICodeWorkflow;
                        var options = workflowToRun.GetOptionsList().Select(x => new MyOptionModel() { MyOption = x }).ToList();
                        workflowOptions = options;
                    }
                }

                return workflowOptions;
            }
        }

        public List<string> FileTypes
        {
            get
            {
                // TODO
                return new List<string>() { "PDF", "XML", "Other" };
            }
        }

        string selectedFileType;
        public string SelectedFileType
        {
            get
            {
                return selectedFileType;
            }
            set
            {
                if (Equals(value, selectedFileType)) return;
                selectedFileType = value;
                NotifyPropertyChanged(() => SelectedFileType);
                NotifyPropertyChanged(() => CustomFileTypeVisibility);
                NotifyPropertyChanged(() => CustomFileTypeErrorVisibility);
                NotifyPropertyChanged(() => SelectedFileTypeErrorVisibility);
                NotifyPropertyChanged(() => FinalFileType);
                NotifyPropertyChanged(() => Workflows);
                NotifyPropertyChanged(() => SaveCommand);
            }
        }

        public Visibility SelectedFileTypeErrorVisibility
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedFileType))
                {
                    return Visibility.Visible;
                }

                return Visibility.Hidden;
            }
        }


        public Visibility CustomFileTypeVisibility
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(new Button()))
                {
                    return Visibility.Visible;
                }
                if (selectedFileType == "Other")
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

        }

        string customFileType;
        public string CustomFileType
        {
            get
            {
                return customFileType;
            }
            set
            {
                if (Equals(value, customFileType)) return;
                customFileType = value;
                NotifyPropertyChanged(() => CustomFileType);
                NotifyPropertyChanged(() => CustomFileTypeErrorVisibility);
                NotifyPropertyChanged(() => SaveCommand);
                NotifyPropertyChanged(() => FinalFileType);
                NotifyPropertyChanged(() => Workflows);
                NotifyPropertyChanged(() => SaveCommand);
            }
        }

        public Visibility CustomFileTypeErrorVisibility
        {
            get
            {
                if (CustomFileTypeVisibility == Visibility.Visible)
                {
                    return string.IsNullOrWhiteSpace(CustomFileType) ? Visibility.Visible : Visibility.Hidden;
                }
                return Visibility.Hidden;
            }
        }

        string fileType;
        public string FileType
        {
            get
            {
                return fileType;
            }
            set
            {
                if (Equals(value, fileType)) return;
                fileType = value;
                NotifyPropertyChanged(() => FileType);
            }
        }

        bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (Equals(value, isActive)) return;
                isActive = value;
                NotifyPropertyChanged(() => IsActive);
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


        ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new RelayCommand(param => Save(param), param => { return CanSave(param); });
                return saveCommand;
            }
        }

        public int OrderNo { get; set; }

        private bool CanSave(object obj)
        {
            if (CustomFileTypeErrorVisibility == Visibility.Hidden &&
                NameErrorVisibility == Visibility.Hidden &&
                SelectedFileTypeErrorVisibility == Visibility.Hidden &&
                SelectedWorkflowErrorVisibility == Visibility.Hidden &&
                HasErrorsForOptions == false
                )
            {
                return true;
            }
            return false;
        }

        private async void Save(object obj)
        {
            if (IsNew)
            {
                int orderNo = 0;
                var oldItems = Repo.Instance.Workflows.Where(x => x.WorkflowDto.SourceId == OwnerSourceId);
                if (oldItems.Any())
                {
                    orderNo = oldItems.Max(x => x.WorkflowDto.OrderNo) + 1;
                }
                else
                {
                    orderNo = 1;
                }


                var newWorkflowDto = new Celsus.Types.WorkflowDto() { Name = Name, IsActive = IsActive, OrderNo = orderNo };
                if (SelectedFileType == "Other")
                {
                    newWorkflowDto.FileType = CustomFileType;
                }
                else
                {
                    newWorkflowDto.FileType = SelectedFileType;
                }
                newWorkflowDto.SourceId = OwnerSourceId;
                if (SelectedWorkflowInternalTypeName != null)
                {
                    var workflowSelected = Workflows.SingleOrDefault(x => x.InternalType.FullName == SelectedWorkflowInternalTypeName);
                    newWorkflowDto.InternalTypeName = workflowSelected.InternalType.FullName;
                    newWorkflowDto.InternalTypeParameters = JsonConvert.SerializeObject(WorkflowOptions.Select(x => x.MyOption).ToList());
                }
                else
                {
                    newWorkflowDto.InternalTypeName = null;
                    newWorkflowDto.InternalTypeParameters = null;
                }
                var result = await Repo.Instance.AddWorkflow(newWorkflowDto);
                if (result == false)
                {
                    Status = "ErrorSaving".ConvertToBindableText();
                }
                else
                {
                    FirstWindowModel.Instance.CloseTabItem(this);
                }
            }
            else
            {
                var newWorkflowDto = new Celsus.Types.WorkflowDto() { Name = Name, IsActive = IsActive };
                newWorkflowDto.Id = Id;
                newWorkflowDto.OrderNo = OrderNo;
                if (SelectedFileType == "Other")
                {
                    newWorkflowDto.FileType = CustomFileType;
                }
                else
                {
                    newWorkflowDto.FileType = SelectedFileType;
                }
                newWorkflowDto.SourceId = OwnerSourceId;
                //if (SelectedWorkflow != null)
                //{
                //    newWorkflowDto.InternalTypeName = SelectedWorkflow.InternalType.FullName;
                //}
                //else
                //{
                //    newWorkflowDto.InternalTypeName = null;
                //}
                //if (SelectedWorkflowName != null)
                //{
                //    var workflowSelected = Workflows.SingleOrDefault(x => x.Name == SelectedWorkflowName);
                //    newWorkflowDto.InternalTypeName = workflowSelected.InternalType.FullName;
                //}
                //else
                //{
                //    newWorkflowDto.InternalTypeName = null;
                //}
                if (SelectedWorkflowInternalTypeName != null)
                {
                    var workflowSelected = Workflows.SingleOrDefault(x => x.InternalType.FullName == SelectedWorkflowInternalTypeName);
                    newWorkflowDto.InternalTypeName = workflowSelected.InternalType.FullName;
                    newWorkflowDto.InternalTypeParameters = JsonConvert.SerializeObject(WorkflowOptions.Select(x => x.MyOption).ToList());
                }
                else
                {
                    newWorkflowDto.InternalTypeName = null;
                    newWorkflowDto.InternalTypeParameters = null;
                }
                var result = await Repo.Instance.UpdateWorkflow(newWorkflowDto);
                if (result == false)
                {
                    Status = "ErrorSaving".ConvertToBindableText();
                }
                else
                {
                    FirstWindowModel.Instance.CloseTabItem(this);
                }
            }
        }

        private void Get()
        {
            var sourceModel = Repo.Instance.Workflows.SingleOrDefault(x => x.WorkflowDto.Id == Id);
            Id = sourceModel.WorkflowDto.Id;
            Name = sourceModel.WorkflowDto.Name;
            IsActive = sourceModel.WorkflowDto.IsActive;
            OrderNo = sourceModel.WorkflowDto.OrderNo;
            Title = Name;
            SelectedFileType = sourceModel.WorkflowDto.FileType;
            //SelectedWorkflow = Workflows.SingleOrDefault(x => x.InternalType.FullName == sourceModel.WorkflowDto.InternalTypeName);
            SelectedWorkflowInternalTypeName = sourceModel.WorkflowDto.InternalTypeName;
            InternalTypeParameters = sourceModel.WorkflowDto.InternalTypeParameters;
        }
    }

    public class MyOptionModel : BaseModel
    {
        MyOption myOption;
        public MyOption MyOption
        {
            get
            {
                return myOption;
            }
            set
            {
                if (Equals(value, myOption)) return;
                myOption = value;
                NotifyPropertyChanged(() => MyOption);
                NotifyPropertyChanged(() => Name);
                NotifyPropertyChanged(() => TypeName);
                NotifyPropertyChanged(() => Value);
                NotifyPropertyChanged(() => ErrorVisibility);
            }
        }

        public Visibility ErrorVisibility
        {
            get
            {
                if (myOption == null)
                {
                    return Visibility.Collapsed;
                }
                if (myOption.IsOptional == false && string.IsNullOrWhiteSpace(myOption.JSonValue))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        public object Value
        {
            get
            {
                if (myOption == null)
                {
                    return null;
                }
                if (string.IsNullOrWhiteSpace(myOption.JSonValue))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject(myOption.JSonValue, myOption.OptionType);
            }
            set
            {
                myOption.JSonValue = JsonConvert.SerializeObject(value);
            }
        }

        public string Name
        {
            get
            {
                return TranslationSource.Instance[myOption?.Name];
            }
        }
        public string TypeName
        {
            get
            {
                if (MyOption == null)
                {
                    return "";
                }
                if (MyOption.OptionType == typeof(string))
                {
                    return "String";
                }
                if (MyOption.OptionType == typeof(bool?))
                {
                    return "NullableBool";
                }
                return "";
            }
        }



    }
    public partial class WorkflowItemControl : UserControl, IItemEditControl
    {
        public int? ItemId
        {
            get
            {
                if ((DataContext as WorkflowItemControlModel).IsNew)
                {
                    return null;
                }
                return (DataContext as WorkflowItemControlModel).Id;
            }
        }

        public WorkflowItemControl()
        {
            InitializeComponent();

        }

        public void PrepareForNew(int ownerSourceId)
        {
            DataContext = new WorkflowItemControlModel() { IsNew = true, OwnerSourceId = ownerSourceId };
        }

        public void PrepareForExisting(int workflowId)
        {
            DataContext = new WorkflowItemControlModel() { Id = workflowId };
        }
    }
}
