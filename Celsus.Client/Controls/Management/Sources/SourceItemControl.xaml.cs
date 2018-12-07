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
    public class SourceItemControlModel : BaseModel
    {
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
                NotifyPropertyChanged(() => SelectFolderCommand);
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
            }
        }

        public Visibility NameErrorVisibility
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        string path;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (Equals(value, path)) return;
                path = value;
                NotifyPropertyChanged(() => Path);
                NotifyPropertyChanged(() => PathErrorVisibility);
            }
        }

        public Visibility PathErrorVisibility
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Path))
                {
                    return Visibility.Visible;
                }
                try
                {
                    if (System.IO.Directory.Exists(Path))
                    {

                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
                catch (Exception)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
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

        ICommand selectFolderCommand;
        public ICommand SelectFolderCommand
        {
            get
            {
                if (selectFolderCommand == null)
                    selectFolderCommand = new RelayCommand(param => SelectFolder(param), param => { return CanSelectFolder(param); });
                return selectFolderCommand;
            }
        }

        private bool CanSelectFolder(object obj)
        {
            return IsNew;
        }

        private void SelectFolder(object obj)
        {
            System.Windows.Forms.FolderBrowserDialog myDialog = new System.Windows.Forms.FolderBrowserDialog();
            myDialog.RootFolder = Environment.SpecialFolder.Desktop;
            myDialog.Description = "Select Source Folder";
            if (myDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(myDialog.SelectedPath))
                {
                    Path = myDialog.SelectedPath;
                }
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

        private bool CanSave(object obj)
        {
            return true;
        }

        private async void Save(object obj)
        {
            if (IsNew)
            {
                var result = await Repo.Instance.AddSource(new Celsus.Types.SourceDto() { Name = Name, IsActive = IsActive, Path = Path, ServerId= ComputerHelper.Instance.ServerId });
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
                var result = await Repo.Instance.UpdateSource(new Celsus.Types.SourceDto() { Id = Id, Name = Name, IsActive = IsActive, Path = Path, ServerId  = ComputerHelper.Instance.ServerId });
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
            var sourceModel = Repo.Instance.Sources.SingleOrDefault(x => x.SourceDto.Id == Id);
            Name = sourceModel.SourceDto.Name;
            Title = Name;
            IsActive = sourceModel.SourceDto.IsActive;
            Path = sourceModel.SourceDto.Path;
        }

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
    }
    public partial class SourceItemControl : UserControl, IItemEditControl
    {
        public int? ItemId
        {
            get
            {
                if ((DataContext as SourceItemControlModel).IsNew)
                {
                    return null;
                }
                return (DataContext as SourceItemControlModel).Id;
            }
        }

        public SourceItemControl()
        {
            InitializeComponent();

        }

        public void PrepareForNew()
        {
            DataContext = new SourceItemControlModel() { IsNew = true };
        }

        public void PrepareForExisting(int sourceId)
        {
            DataContext = new SourceItemControlModel() { IsNew = false, Id = sourceId };
        }
    }
}
