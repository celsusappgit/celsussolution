using Celsus.Client.Shared.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
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

namespace Celsus.Client.Wpf.Controls.Management
{
    /// <summary>
    /// Interaction logic for SourceItem.xaml
    /// </summary>
    public partial class SourceItem : UserControl
    {
        public SourceDto SourceDto { get; set; }
        private int _originalRecordIndex = -1;
        ObservableCollection<SourceDto> _mainCollection;
        public FileTypeDto SelectedFileType { get; set; }
        public ICollectionViewLiveShaping FileTypes { get; private set; }

        ObservableCollection<FileTypeDto> _fileTypes = new ObservableCollection<FileTypeDto>();

        public SourceItem()
        {
            InitializeComponent();
        }

        public void InitForNew(ObservableCollection<SourceDto> mainCollection)
        {
            SourceDto = new SourceDto();
            _mainCollection = mainCollection;
            DataContext = this;
            BorderFileTypes.IsEnabled = false;
        }

        internal void InitForEdit(SourceDto selectedSource, ObservableCollection<SourceDto> mainCollection)
        {
            SourceDto = selectedSource;
            _mainCollection = mainCollection;
            _originalRecordIndex = _mainCollection.IndexOf(SourceDto);
            DataContext = this;
            GetFileTypes();
        }

        private async void GetFileTypes()
        {
            try
            {
                List<FileTypeDto> sources = null;
                using (var context = new SqlDbContext())
                {
                    sources = await context.FileTypes.ToListAsync();
                }
                _fileTypes = new ObservableCollection<FileTypeDto>(sources);
                var itemsView = new CollectionViewSource() { Source = _fileTypes }.View;
                itemsView.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Ascending, PropertyName = "Name" });
                FileTypes = itemsView as ICollectionViewLiveShaping;
                if (FileTypes.CanChangeLiveSorting)
                {
                    FileTypes.LiveSortingProperties.Add("Name");
                    FileTypes.IsLiveSorting = true;
                }
                RadGridViewFileTypes.ItemsSource = (ICollectionView)FileTypes;
            }
            catch (Exception ex)
            {
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                using (var context = new SqlDbContext())
                {
                    if (SourceDto.Id == 0)
                    {
                        if (context.Sources.Count(x => x.Name == SourceDto.Name) > 0)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Another source has same name.", ShowDuration = 3000 });
                            BorderFileTypes.IsEnabled = true;
                            return;
                        }
                        if (context.Sources.Count(x => x.Path == SourceDto.Path) > 0)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Another source has same path.", ShowDuration = 3000 });
                            BorderFileTypes.IsEnabled = true;
                            return;
                        }
                        if (context.Sources.Count(x => x.Path.Contains(SourceDto.Path)) > 0)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Another source path covers this source.", ShowDuration = 3000 });
                            BorderFileTypes.IsEnabled = true;
                            return;
                        }
                        if (context.Sources.Count(x => SourceDto.Path.Contains(x.Path)) > 0)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "This source path covers another source.", ShowDuration = 3000 });
                            BorderFileTypes.IsEnabled = true;
                            return;
                        }
                        SourceDto.ServerId = ComputerHelper.Instance.ServerId;
                        context.Sources.Add(SourceDto);
                        var saveResult = await context.SaveChangesAsync();
                        if (saveResult == 1)
                        {
                            (Application.Current.MainWindow as MainWindow).ShowAlert(new Telerik.Windows.Controls.RadDesktopAlert() { Width = 400, Header = "Success", Content = "Save operation completed.", ShowDuration = 3000 });
                            BorderFileTypes.IsEnabled = true;
                            _mainCollection.Add(SourceDto);
                        }
                    }
                    else
                    {
                        context.Entry(SourceDto).State = System.Data.Entity.EntityState.Modified;
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

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            if (SourceDto.Id == 0)
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
                            _mainCollection.Remove(SourceDto);
                        }
                        SourceDto = await context.Sources.SingleOrDefaultAsync(x => x.Id == SourceDto.Id);
                        _mainCollection.Insert(_originalRecordIndex, SourceDto);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            (Application.Current.MainWindow as MainWindow).Back();
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog myDialog = new System.Windows.Forms.FolderBrowserDialog();
            myDialog.RootFolder = Environment.SpecialFolder.Desktop;
            myDialog.Description = "Select Source Folder";
            if (myDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(myDialog.SelectedPath))
                {
                    SourceDto.Path = myDialog.SelectedPath;
                }
            }
        }

        private void AddFileType_Click(object sender, RoutedEventArgs e)
        {
            var fileTypeItem = new FileTypeItem();
            fileTypeItem.InitForNew(_fileTypes, SourceDto);
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack(fileTypeItem);
        }

        private void EditSelectedFileType_Click(object sender, RoutedEventArgs e)
        {
            var fileTypeItem = new FileTypeItem();
            fileTypeItem.InitForEdit(SelectedFileType, _fileTypes);
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack(fileTypeItem);
        }

        private void OpenCronHelper_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("IExplore.exe", "https://crontab.guru/");
        }
    }
}
