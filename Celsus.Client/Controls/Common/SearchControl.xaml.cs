using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.DataLayer;
using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;

namespace Celsus.Client.Controls.Common
{
    public class SearchControlModel : BaseModel<SearchControlModel>
    {

        public SearchControlModel()
        {
            FirstWindowModel.Instance.AnalitycsMonitor.TrackScreenView("Search");
        }

        private List<FileSystemItemMetadataDto> uniqueMetaDatas;

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
                NotifyPropertyChanged(() => SearchText);
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

        bool mainTextIsFocused;
        public bool MainTextIsFocused
        {
            get
            {
                return mainTextIsFocused;
            }
            set
            {
                mainTextIsFocused = value;
                NotifyPropertyChanged(() => MainTextIsFocused);
            }
        }

        List<FileSystemItemModel> searchResult;
        public List<FileSystemItemModel> SearchResult
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

        bool isPopUpOpen;
        public bool IsPopUpOpen
        {
            get
            {
                return isPopUpOpen;
            }
            set
            {
                if (Equals(value, isPopUpOpen)) return;
                isPopUpOpen = value;
                NotifyPropertyChanged(() => IsPopUpOpen);
            }
        }

        FileSystemItemMetadataDto selectedMetadata = null;
        public FileSystemItemMetadataDto SelectedMetadata
        {
            get
            {
                return selectedMetadata;
            }
            set
            {
                if (Equals(value, selectedMetadata)) return;
                selectedMetadata = value;
                NotifyPropertyChanged(() => SelectedMetadata);
            }
        }

        ICommand showMetadataCommand;

        public ICommand ShowMetadataCommand
        {
            get
            {
                if (showMetadataCommand == null)
                    showMetadataCommand = new RelayCommand(param => ShowMetadata(param), param => { return true; });
                return showMetadataCommand;
            }
        }

        private void ShowMetadata(object param)
        {
            var selectedFileSystemItem = (FileSystemItemModel)((FrameworkElement)((RoutedEventArgs)param).Source).DataContext;
            var metadataViewerControl = new MetadataViewerControl();
            metadataViewerControl.Prepare(selectedFileSystemItem.Id);
            RadWindow newWindow = new RadWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = (App.Current.MainWindow as FirstWindow),
                Content = metadataViewerControl,
                SizeToContent = false,
                Width = (App.Current.MainWindow as FirstWindow).Width - 100,
                Height = (App.Current.MainWindow as FirstWindow).Height - 100,
                Header = TranslationSource.Instance["MetadataViewerControl"]
            };
            RadWindowInteropHelper.SetAllowTransparency(newWindow, false);
            newWindow.ShowDialog();
        }

        ICommand showContentsCommand;

        public ICommand ShowContentsCommand
        {
            get
            {
                if (showContentsCommand == null)
                    showContentsCommand = new RelayCommand(param => ShowContents(param), param => { return true; });
                return showContentsCommand;
            }
        }

        private void ShowContents(object param)
        {
            var selectedFileSystemItem = (FileSystemItemModel)((FrameworkElement)((RoutedEventArgs)param).Source).DataContext;
            var documentViewerControl = new DocumentViewerControl();
            documentViewerControl.Prepare(selectedFileSystemItem.Id);
            RadWindow newWindow = new RadWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = (App.Current.MainWindow as FirstWindow),
                Content = documentViewerControl,
                SizeToContent = false,
                Width = (App.Current.MainWindow as FirstWindow).Width - 100,
                Height = (App.Current.MainWindow as FirstWindow).Height - 100,
                Header = TranslationSource.Instance["DocumentViewerControl"]
            };
            RadWindowInteropHelper.SetAllowTransparency(newWindow, false);
            newWindow.ShowDialog();
        }

        ICommand selectMetadataCommand;

        public ICommand SelectMetadataCommand
        {
            get
            {
                if (selectMetadataCommand == null)
                    selectMetadataCommand = new RelayCommand(param => SelectMetadata(param), param => { return true; });
                return selectMetadataCommand;
            }
        }

        private void SelectMetadata(object param)
        {
            if (SelectedMetadata != null)
            {
                SearchMetadatas.Add(new FileSystemItemMetadataModel() { StartDate = DateTime.Now, IsFocused = true, BoolValue = false, DateTimeValue = DateTime.Now, FileSystemItemId = 0, Key = SelectedMetadata.Key, IntValue = 0, LongValue = 0, StringValue = "", ValueType = SelectedMetadata.ValueType });
                IsPopUpOpen = false;
                SearchText = "";
            }
        }

        ICommand searchPreviewKeyDownCommand;

        public ICommand SearchPreviewKeyDownCommand
        {
            get
            {
                if (searchPreviewKeyDownCommand == null)
                    searchPreviewKeyDownCommand = new RelayCommand(param => SearchPreviewKeyDown(param), param => { return true; });
                return searchPreviewKeyDownCommand;
            }
        }

        private void SearchPreviewKeyDown(object param)
        {
            var e = param as KeyEventArgs;
            if (isPopUpOpen)
            {
                if (e.Key == Key.Down)
                {
                    if (SelectedMetadata == null)
                    {
                        if (PossibleMetadatas != null && PossibleMetadatas.Count > 0)
                        {
                            SelectedMetadata = PossibleMetadatas.First();
                        }
                    }
                    else
                    {
                        if (PossibleMetadatas != null && PossibleMetadatas.Count > 0)
                        {
                            var i = PossibleMetadatas.IndexOf(SelectedMetadata);
                            if (PossibleMetadatas.Count > (i + 1))
                            {
                                SelectedMetadata = PossibleMetadatas[i + 1];
                            }
                        }
                    }
                }
                else if (e.Key == Key.Up)
                {
                    if (SelectedMetadata == null)
                    {
                        if (PossibleMetadatas != null && PossibleMetadatas.Count > 0)
                        {
                            SelectedMetadata = PossibleMetadatas.Last();
                        }
                    }
                    else
                    {
                        if (PossibleMetadatas != null && PossibleMetadatas.Count > 0)
                        {
                            var i = PossibleMetadatas.IndexOf(SelectedMetadata);
                            if (i == 1)
                            {
                                if (PossibleMetadatas.Count > 0)
                                {
                                    SelectedMetadata = PossibleMetadatas[0];
                                }
                            }
                            else if (i > 1)
                            {
                                if (PossibleMetadatas.Count > (i - 1))
                                {
                                    SelectedMetadata = PossibleMetadatas[i - 1];
                                }
                            }
                        }
                    }
                }
            }
        }

        ICommand deleteSearchMetadataCommand;
        public ICommand DeleteSearchMetadataCommand
        {
            get
            {
                if (deleteSearchMetadataCommand == null)
                    deleteSearchMetadataCommand = new RelayCommand(param => DeleteSearchMetadata(param), param => { return true; });
                return deleteSearchMetadataCommand;
            }
        }
        private void DeleteSearchMetadata(object param)
        {
            var selectedMetadata = (FileSystemItemMetadataModel)((FrameworkElement)((RoutedEventArgs)param).Source).DataContext;
            SearchMetadatas.Remove(selectedMetadata);
            Status = "CriteriaRemoved".ConvertToBindableText();
        }

       

        //StringValueKeyUpCommand

        ICommand stringValueKeyUpCommand;

        public ICommand StringValueKeyUpCommand
        {
            get
            {
                if (stringValueKeyUpCommand == null)
                    stringValueKeyUpCommand = new RelayCommand(param => StringValueKeyUp(param), param => { return true; });
                return stringValueKeyUpCommand;
            }
        }

        private void StringValueKeyUp(object param)
        {
            var e = param as KeyEventArgs;
            if (e.Key == Key.Enter)
            {
                MainTextIsFocused = false;
                MainTextIsFocused = true;
            }
        }

        ICommand searchCommand;

        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                    searchCommand = new RelayCommand(param => Search(param), param => { return true; });
                return searchCommand;
            }
        }

        List<FileSystemItemMetadataDto> possibleMetadatas;
        public List<FileSystemItemMetadataDto> PossibleMetadatas
        {
            get
            {
                return possibleMetadatas;
            }
            set
            {
                if (Equals(value, possibleMetadatas)) return;
                possibleMetadatas = value;
                NotifyPropertyChanged(() => PossibleMetadatas);
            }
        }

        ObservableCollection<FileSystemItemMetadataModel> searchMetadatas;
        public ObservableCollection<FileSystemItemMetadataModel> SearchMetadatas
        {
            get
            {
                if (searchMetadatas == null)
                {
                    searchMetadatas = new ObservableCollection<FileSystemItemMetadataModel>();
                }
                return searchMetadatas;
            }
            set
            {
                if (Equals(value, searchMetadatas)) return;
                searchMetadatas = value;
                NotifyPropertyChanged(() => SearchMetadatas);
            }
        }

        private void Search(object obj)
        {
            var e = obj as KeyEventArgs;
            if (e.Key == Key.Enter)
            {
                if (IsPopUpOpen)
                {
                    if (SelectedMetadata != null)
                    {
                        SearchMetadatas.Add(new FileSystemItemMetadataModel() { IsFocused = true, BoolValue = false, DateTimeValue = DateTime.Now, FileSystemItemId = 0, Key = SelectedMetadata.Key, IntValue = 0, LongValue = 0, StringValue = "", ValueType = SelectedMetadata.ValueType });
                        IsPopUpOpen = false;
                        SearchText = "";
                        Status = "CriteriaAdded".ConvertToBindableText();
                        return;
                    }

                }

                

                var query = new StringBuilder();
                var sqlParameters = new List<SqlParameter>();
                var clauses = new List<string>();
                var innerJoins = new List<string>();
                ;

                if (string.IsNullOrWhiteSpace(SearchText) && SearchMetadatas.Any() == false)
                {
                    Status = "PleaseEnterAnySearchTerm".ConvertToBindableText();
                    return;
                }

                var searchSentence = SearchText.Trim();

                query.AppendLine("SELECT [Celsus].[FileSystemItem].*, (SELECT COUNT(*) FROM [Celsus].[FileSystemItemMetadata] WHERE [FileSystemItemMetadata].FileSystemItemId = [Celsus].[FileSystemItem].Id) AS MetadataCount ");
                query.AppendLine("FROM   [Celsus].[FileSystemItem] ");

                if (string.IsNullOrWhiteSpace(searchSentence) == false)
                {
                    innerJoins.Add("       INNER JOIN [Celsus].[ClearText] ON [Celsus].[FileSystemItem].Id = [Celsus].[ClearText].FileSystemItemId");
                }


                if (SearchMetadatas.Any())
                {
                    GenerateMetadataQuery(ref innerJoins, ref clauses, ref sqlParameters);
                }
                if (string.IsNullOrWhiteSpace(searchSentence) == false)
                {
                    clauses.Add("CONTAINS(([Celsus].[ClearText].TextInFile ), '\"" + SearchText + "\"')  ");
                }
                query.AppendLine(string.Join($"{Environment.NewLine}", innerJoins.ToArray()));
                query.AppendLine("WHERE  ");
                query.AppendLine(string.Join($"{Environment.NewLine}AND{Environment.NewLine}", clauses.ToArray()));

                if (query.Length > 0)
                {
                    IsBusy = true;
                    try
                    {
                        using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                        {
                            var t = query.ToString();
                            var results = context.Database.SqlQuery<FileSystemItemModel>(query.ToString(), sqlParameters.ToArray()).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
                            SearchResult = results;
                            //Status = $"Found {results.Count} items";
                            if (results == null || results.Count == 0)
                            {
                                NoItemsContent = SearchText;
                                Status = "ThereIsNoItemsFoundForSearchTerm".ConvertToBindableText();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Status = "ErrorInSearch".ConvertToBindableText();
                        logger.Error(ex, "Error in Search");
                    }
                    IsBusy = false;
                }


            }
            else
            {
                if (uniqueMetaDatas == null)
                {
                    try
                    {
                        using (var context = new SqlDbContext(DatabaseHelper.Instance.ConnectionInfo.ConnectionString))
                        {
                            uniqueMetaDatas = context.FileSystemItemMetadatas.GroupBy(x => x.Key).Select(x => x.FirstOrDefault()).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                if (uniqueMetaDatas != null && string.IsNullOrWhiteSpace(SearchText) == false)
                {
                    PossibleMetadatas = uniqueMetaDatas.Where(x => x.Key.StartsWith(SearchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (PossibleMetadatas.Any())
                    {
                        IsPopUpOpen = true;
                    }
                    else
                    {
                        IsPopUpOpen = false;
                    }
                }
            }
        }

        private void GenerateMetadataQuery(ref List<string> innerJoins, ref List<string> clauses, ref List<SqlParameter> sqlParameters)
        {
            int paramIndex = 0;
            int tableIndex = 0;
            foreach (var searchMetadata in SearchMetadatas)
            {
                paramIndex++;
                tableIndex++;
                if (searchMetadata.ValueType == "String" && string.IsNullOrWhiteSpace(searchMetadata.StringValue) == false)
                {
                    clauses.Add($"FileSystemItemMetadata{tableIndex}.StringValue = @P{paramIndex}");
                    innerJoins.Add($"       INNER JOIN [Celsus].[FileSystemItemMetadata] AS FileSystemItemMetadata{tableIndex} ON [Celsus].[FileSystemItem].Id = FileSystemItemMetadata{tableIndex}.FileSystemItemId");
                    sqlParameters.Add(new SqlParameter($"@P{paramIndex}", searchMetadata.StringValue));
                }
                else if (searchMetadata.ValueType == "Bool" && searchMetadata.BoolValue.HasValue)
                {
                    clauses.Add($"FileSystemItemMetadata{tableIndex}.BoolValue = @P{paramIndex}");
                    innerJoins.Add($"       INNER JOIN [Celsus].[FileSystemItemMetadata] AS FileSystemItemMetadata{tableIndex} ON [Celsus].[FileSystemItem].Id = FileSystemItemMetadata{tableIndex}.FileSystemItemId");
                    sqlParameters.Add(new SqlParameter($"@P{paramIndex}", searchMetadata.BoolValue));
                }
                else if (searchMetadata.ValueType == "Int" && searchMetadata.IntValue.HasValue)
                {
                    clauses.Add($"FileSystemItemMetadata{tableIndex}.IntValue = @P{paramIndex}");
                    innerJoins.Add($"       INNER JOIN [Celsus].[FileSystemItemMetadata] AS FileSystemItemMetadata{tableIndex} ON [Celsus].[FileSystemItem].Id = FileSystemItemMetadata{tableIndex}.FileSystemItemId");
                    sqlParameters.Add(new SqlParameter($"@P{paramIndex}", searchMetadata.IntValue));
                }
                else if (searchMetadata.ValueType == "DateTime" && (searchMetadata.StartDate.HasValue || searchMetadata.EndDate.HasValue))
                {
                    innerJoins.Add($"       INNER JOIN [Celsus].[FileSystemItemMetadata] AS FileSystemItemMetadata{tableIndex} ON [Celsus].[FileSystemItem].Id = FileSystemItemMetadata{tableIndex}.FileSystemItemId");

                    if (searchMetadata.StartDate.HasValue)
                    {
                        clauses.Add($"FileSystemItemMetadata{tableIndex}.DateTimeValue >= @P{paramIndex}");
                        sqlParameters.Add(new SqlParameter($"@P{paramIndex}", new DateTime(searchMetadata.StartDate.Value.Year, searchMetadata.StartDate.Value.Month, searchMetadata.StartDate.Value.Day)));
                    }
                    if (searchMetadata.EndDate.HasValue)
                    {
                        paramIndex++;
                        clauses.Add($"FileSystemItemMetadata{tableIndex}.DateTimeValue <= @P{paramIndex}");
                        sqlParameters.Add(new SqlParameter($"@P{paramIndex}", new DateTime(searchMetadata.EndDate.Value.Year, searchMetadata.EndDate.Value.Month, searchMetadata.EndDate.Value.Day).AddDays(1)));
                    }
                }
                else if (searchMetadata.ValueType == "Long" && searchMetadata.LongValue.HasValue)
                {
                    clauses.Add($"FileSystemItemMetadata{tableIndex}.LongValue = @P{paramIndex}");
                    innerJoins.Add($"       INNER JOIN [Celsus].[FileSystemItemMetadata] AS FileSystemItemMetadata{tableIndex} ON [Celsus].[FileSystemItem].Id = FileSystemItemMetadata{tableIndex}.FileSystemItemId");
                    sqlParameters.Add(new SqlParameter($"@P{paramIndex}", searchMetadata.LongValue));
                }
            }
        }

    }
    public partial class SearchControl : UserControl
    {
        public SearchControl()
        {
            InitializeComponent();
            DataContext = SearchControlModel.Instance;
        }
    }

    public class FocusHelper
    {
        private static void OnEnsureFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (true)
            {
                var control = d as UIElement;
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    control.Focus();
                }));
            }
        }

        public static bool GetEnsureFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnsureFocusProperty);
        }

        public static void SetEnsureFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(EnsureFocusProperty, value);
        }

        public static readonly DependencyProperty EnsureFocusProperty =
            DependencyProperty.RegisterAttached(
            "EnsureFocus",
            typeof(bool),
            typeof(FocusHelper),
            new PropertyMetadata(OnEnsureFocusChanged));
    }

    public class FileSystemItemModel : FileSystemItemDto
    {
        int metadataCount;
        public int MetadataCount
        {
            get
            {
                return metadataCount;
            }
            set
            {
                if (Equals(value, metadataCount)) return;
                metadataCount = value;
                NotifyPropertyChanged(() => MetadataCount);
                NotifyPropertyChanged(() => HasMetadata);
            }
        }

        public bool HasMetadata
        {
            get
            {
                return MetadataCount > 0;
            }
        }

    }

    public class FileSystemItemMetadataModel : FileSystemItemMetadataDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFocused { get; set; }
    }
}
