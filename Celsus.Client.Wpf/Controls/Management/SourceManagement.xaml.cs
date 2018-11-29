using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
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
    /// Interaction logic for SourceManagement.xaml
    /// </summary>
    public partial class SourceManagement : UserControl
    {
        public ICollectionViewLiveShaping Sources { get; private set; }

        public SourceDto SelectedSource { get; set; }

        ObservableCollection<SourceDto> _sources = new ObservableCollection<SourceDto>();
        public SourceManagement()
        {
            InitializeComponent();
            this.Loaded += SourceManagement_Loaded;
        }

        private async void SourceManagement_Loaded(object sender, RoutedEventArgs e)
        {
            await GetSources();
        }

        private async Task GetSources()
        {
            try
            {
                List<SourceDto> sources = null;
                using (var context = new SqlDbContext())
                {
                    sources = await context.Sources.ToListAsync();
                }
                _sources = new ObservableCollection<SourceDto>(sources);
                var itemsView = new CollectionViewSource() { Source = _sources }.View;
                itemsView.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Ascending, PropertyName = "Name" });
                Sources = itemsView as ICollectionViewLiveShaping;
                if (Sources.CanChangeLiveSorting)
                {
                    Sources.LiveSortingProperties.Add("Name");
                    Sources.IsLiveSorting = true;
                }
                this.DataContext = this;
            }
            catch (Exception ex)
            {
            }
        }

        private void AddNewSource_Click(object sender, RoutedEventArgs e)
        {
            var sourceItem = new SourceItem();
            sourceItem.InitForNew(_sources);
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack(sourceItem);
        }

        private void EditSelectedSource_Click(object sender, RoutedEventArgs e)
        {
            var sourceItem = new SourceItem();
            sourceItem.InitForEdit(SelectedSource, _sources);
            (Application.Current.MainWindow as MainWindow).LoadContentCanGoToBack(sourceItem);
        }
    }

    

}
