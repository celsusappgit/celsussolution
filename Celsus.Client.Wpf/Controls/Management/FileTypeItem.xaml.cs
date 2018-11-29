using Celsus.Activities;
using Celsus.DataLayer;
using Celsus.Types;
using Microsoft.Win32;
using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
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

namespace Celsus.Client.Wpf.Controls.Management
{
    /// <summary>
    /// Interaction logic for FileTypeItem.xaml
    /// </summary>
    public partial class FileTypeItem : UserControl
    {
        private WorkflowDesigner _workflowDesigner;

        private FileTypeDto _fileTypeDto;
        private int _originalRecordIndex = -1;
        ObservableCollection<FileTypeDto> _mainCollection;
        public FileTypeItem()
        {
            InitializeComponent();
            Loaded += FileTypeItem_Loaded;
        }

        public void InitForNew(ObservableCollection<FileTypeDto> mainCollection, SourceDto parentSource)
        {
            _fileTypeDto = new FileTypeDto();
            _fileTypeDto.SourceId = parentSource.Id;
            _mainCollection = mainCollection;
            DataContext = _fileTypeDto;
        }

        internal void InitForEdit(FileTypeDto selectedFileType, ObservableCollection<FileTypeDto> mainCollection)
        {
            _fileTypeDto = selectedFileType;
            _mainCollection = mainCollection;
            _originalRecordIndex = _mainCollection.IndexOf(_fileTypeDto);
            DataContext = _fileTypeDto;
        }

        private void FileTypeItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (_fileTypeDto.Id == 0)
            {
                DesignerMetadata designerMetadata = new DesignerMetadata();
                designerMetadata.Register();
                _workflowDesigner = new WorkflowDesigner();
                Optin45();
                BrdDesigner.Child = _workflowDesigner.View;
                BrdProperties.Child = _workflowDesigner.PropertyInspectorView;
                BrdActivities.Child = GetToolboxControl();
                var activityBuilder = new ActivityBuilder();
                _workflowDesigner.Load(activityBuilder);
                AddDefaultArguments();
            }
            else
            {
                DesignerMetadata designerMetadata = new DesignerMetadata();
                designerMetadata.Register();
                _workflowDesigner = new WorkflowDesigner();
                Optin45();
                BrdDesigner.Child = _workflowDesigner.View;
                BrdProperties.Child = _workflowDesigner.PropertyInspectorView;
                BrdActivities.Child = GetToolboxControl();

                string tempFile = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllBytes(tempFile, _fileTypeDto.Workflow);
                _workflowDesigner.Load(tempFile);

            }


        }

        private void AddDefaultArguments()
        {
            ModelTreeManager mtm = _workflowDesigner.Context.Services.GetService<ModelTreeManager>();
            ModelItem ab = mtm.Root;
            ModelItemCollection argsAndProperties = ab.Properties["Properties"].Collection;
            argsAndProperties.Add(new DynamicActivityProperty
            {
                Name = "ArgFileSystemItemId",
                Type = typeof(InArgument<int>),
            });
            argsAndProperties.Add(new DynamicActivityProperty
            {
                Name = "ArgSessionId",
                Type = typeof(InArgument<string>),
            });
        }

        private ToolboxControl GetToolboxControl()
        {
            ToolboxControl toolboxControl = new ToolboxControl();

            ToolboxCategory toolboxCategoryEbys = new ToolboxCategory("Celsus");
            List<Type> activities = typeof(Tessaract).Assembly.GetTypes().Where(x => x.FullName.StartsWith("Celsus.Activities") && x.BaseType.IsGenericType && (x.BaseType.GetGenericTypeDefinition().FullName.IndexOf("NativeActivity") >= 0 || x.BaseType.GetGenericTypeDefinition().FullName.IndexOf("CodeActivity") >= 0)).ToList();
            activities.ForEach(x => toolboxCategoryEbys.Add(new ToolboxItemWrapper(x)));
            toolboxControl.Categories.Add(toolboxCategoryEbys);

            var catWs = new ToolboxCategory("Common");
            var assemblies = new List<Assembly>();
            assemblies.Add(typeof(Delay).Assembly);
            //assemblies.Add(typeof(Point.Workflow.Designer.PointManagement.ChangeRoleRightCompletedEventArgs).Assembly);
            var query = from asm in assemblies
                        from type in asm.GetTypes()
                        where
                        type.IsPublic &&
                        !type.IsNested &&
                        !type.IsAbstract &&
                        !type.ContainsGenericParameters &&
                        (
                        typeof(Activity).IsAssignableFrom(type) ||
                        typeof(IActivityTemplateFactory).IsAssignableFrom(type)
                        )
                        orderby type.Name
                        select new ToolboxItemWrapper(type);

            query.ToList().ForEach(ti => catWs.Add(ti));

            catWs.Add(new ToolboxItemWrapper(typeof(FlowDecision), "FlowDecision"));
            catWs.Add(new ToolboxItemWrapper(typeof(FlowSwitch<>), "FlowSwitch"));

            toolboxControl.Categories.Add(catWs);

            toolboxControl.BorderThickness = new Thickness(0);

            return toolboxControl;
        }

        private void Optin45()
        {
            DesignerConfigurationService configService = _workflowDesigner.Context.Services.GetRequiredService<DesignerConfigurationService>();
            configService.AnnotationEnabled = true; /* maybe, see explanation of TargetFrameworkName*/
            configService.AutoConnectEnabled = true;
            configService.AutoSplitEnabled = true;
            configService.AutoSurroundWithSequenceEnabled = true;
            configService.BackgroundValidationEnabled = true;
            configService.MultipleItemsContextMenuEnabled = true;
            configService.MultipleItemsDragDropEnabled = true;
            configService.NamespaceConversionEnabled = true;
            configService.PanModeEnabled = true;
            configService.RubberBandSelectionEnabled = true;
            configService.LoadingFromUntrustedSourceEnabled = false;
            configService.TargetFrameworkName = new FrameworkName(".NETFramework,Version=v4.5");
        }


        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_fileTypeDto.Id == 0)
            {

            }
            else
            {
                try
                {
                    using (var context = new SqlDbContext())
                    {
                        if (_originalRecordIndex >= 0)
                        {
                            _mainCollection.Remove(_fileTypeDto);
                        }
                        _fileTypeDto = await context.FileTypes.SingleOrDefaultAsync(x => x.Id == _fileTypeDto.Id);
                        _mainCollection.Insert(_originalRecordIndex, _fileTypeDto);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            (Application.Current.MainWindow as MainWindow).Back();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string tempFile = System.IO.Path.GetTempFileName();
            _workflowDesigner.Save(tempFile);
            _fileTypeDto.Workflow = System.IO.File.ReadAllBytes(tempFile);

            try
            {
                using (var context = new SqlDbContext())
                {
                    if (_fileTypeDto.Id == 0)
                    {
                        context.FileTypes.Add(_fileTypeDto);
                        var saveResult = await context.SaveChangesAsync();
                        if (saveResult == 1)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Save operation completed.", ShowDuration = 3000 });
                        }
                        _mainCollection.Add(_fileTypeDto);
                    }
                    else
                    {
                        context.Entry(_fileTypeDto).State = EntityState.Modified;
                        var saveResult = await context.SaveChangesAsync();
                        if (saveResult == 1)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Save operation completed.", ShowDuration = 3000 });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        private void ExportToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog myDialog = new SaveFileDialog
            {
                Filter = "XAML Files" + " (*.xaml)|*.xaml",
                CheckFileExists = false
            };
            if (myDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(myDialog.FileName))
                {
                    _workflowDesigner.Save(myDialog.FileName);
                }
            }
            
        }

        private void ImportFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myDialog = new OpenFileDialog
            {
                Filter = "XAML Files" + " (*.xaml)|*.xaml",
                CheckFileExists = true,
                Multiselect = false
            };
            if (myDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(myDialog.FileName))
                {
                    DesignerMetadata designerMetadata = new DesignerMetadata();
                    designerMetadata.Register();
                    _workflowDesigner = new WorkflowDesigner();
                    Optin45();
                    BrdDesigner.Child = _workflowDesigner.View;
                    BrdProperties.Child = _workflowDesigner.PropertyInspectorView;
                    BrdActivities.Child = GetToolboxControl();

                    _workflowDesigner.Load(myDialog.FileName);
                }
            }
            
        }
    }
}
