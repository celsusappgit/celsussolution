using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
using Telerik.Windows.Controls;

namespace Celsus.Client.Wpf.Controls.Main
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : UserControl, INotifyPropertyChanged
    {

        bool searchInFileContents = true;
        public bool SearchInFileContents
        {
            get
            {
                return searchInFileContents;
            }
            set
            {
                if (Equals(value, searchInFileContents)) return;
                searchInFileContents = value;
                NotifyPropertyChanged(() => SearchInFileContents);
            }
        }

        bool searchInFileNames;
        public bool SearchInFileNames
        {
            get
            {
                return searchInFileNames;
            }
            set
            {
                if (Equals(value, searchInFileNames)) return;
                searchInFileNames = value;
                NotifyPropertyChanged(() => SearchInFileNames);
            }
        }
        bool searchInFileMetadata;
        public bool SearchInFileMetadata
        {
            get
            {
                return searchInFileMetadata;
            }
            set
            {
                if (Equals(value, searchInFileMetadata)) return;
                searchInFileMetadata = value;
                NotifyPropertyChanged(() => SearchInFileMetadata);
            }
        }

        public string ShowOptionsText
        {
            get
            {
                if (ShowOptionsValue == "Dont")
                {
                    return "Show Options";
                }
                return "Hide Options";
            }
        }
        public Visibility ShowOptionsVisibility
        {
            get
            {
                if (ShowOptionsValue == "Dont")
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        string searchText;
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                if (Equals(value, searchText)) return;
                searchText = value;
                if (string.IsNullOrWhiteSpace(searchText) == false)
                {
                    NoItemsText01 = string.Empty;
                    NoItemsText02 = string.Empty;
                    NoItemsContent = string.Empty;
                }
                NotifyPropertyChanged(() => SearchText);
            }
        }

        string noItemsText01;
        public string NoItemsText01
        {
            get
            {
                return noItemsText01;
            }
            set
            {
                if (Equals(value, noItemsText01)) return;
                noItemsText01 = value;
                NotifyPropertyChanged(() => NoItemsText01);
            }
        }

        string noItemsText02;
        public string NoItemsText02
        {
            get
            {
                return noItemsText02;
            }
            set
            {
                if (Equals(value, noItemsText02)) return;
                noItemsText02 = value;
                NotifyPropertyChanged(() => NoItemsText02);
            }
        }

        string noItemsContent;
        public string NoItemsContent
        {
            get
            {
                return noItemsContent;
            }
            set
            {
                if (Equals(value, noItemsContent)) return;
                noItemsContent = value;
                NotifyPropertyChanged(() => NoItemsContent);
            }
        }

        List<FileSystemItemDto> searchResult;
        public List<FileSystemItemDto> SearchResult
        {
            get
            {
                return searchResult;
            }
            set
            {
                if (Equals(value, searchResult)) return;
                searchResult = value;
                NotifyPropertyChanged(() => SearchResult);
            }
        }
        string noResultText;
        public string NoResultText
        {
            get
            {
                return noResultText;
            }
            set
            {
                if (Equals(value, noResultText)) return;
                noResultText = value;
                NotifyPropertyChanged(() => NoResultText);
            }
        }

        string showOptionsValue = "Dont";
        public string ShowOptionsValue
        {
            get
            {
                return showOptionsValue;
            }
            set
            {
                if (Equals(value, showOptionsValue)) return;
                showOptionsValue = value;
                NotifyPropertyChanged(() => ShowOptionsValue);
                NotifyPropertyChanged(() => ShowOptionsVisibility);
                NotifyPropertyChanged(() => ShowOptionsText);
            }
        }

        public Search()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void RadMaskedTextInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT [Celsus].[FileSystemItem].* ");
                sb.Append("FROM   [Celsus].[ClearText] ");
                sb.Append("       INNER JOIN [Celsus].[FileSystemItem] ");
                sb.Append("               ON [Celsus].[ClearText].FileSystemItemId = [Celsus].[FileSystemItem].Id ");
                sb.Append("WHERE  CONTAINS(( [Celsus].[ClearText].TextInFile ), '*" + SearchText + "*')  ");

                RadBusyIndicator.IsBusy = true;
                try
                {
                    using (var context = new SqlDbContext())
                    {
                        var results = context.Database.SqlQuery<FileSystemItemDto>(sb.ToString()).ToList();
                        SearchResult = results;
                        if (results == null || results.Count == 0)
                        {
                            NoItemsContent = SearchText;
                            NoItemsText01 = "There is no items found for search term";
                            NoItemsText02 = ". Please refine your search.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                RadBusyIndicator.IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowOptions(object sender, RoutedEventArgs e)
        {
            if (ShowOptionsValue == "Dont")
            {
                ShowOptionsValue = "Show";
            }
            else
            {
                ShowOptionsValue = "Dont";
            }
        }
    }
}

