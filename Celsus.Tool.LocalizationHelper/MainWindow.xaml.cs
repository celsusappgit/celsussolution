using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Celsus.Tool.LocalizationHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private class NotifyBase : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;
            protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
            {
                var memberExpression = (MemberExpression)exp.Body;
                string propertyName = memberExpression.Member.Name;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion
        }
        private class TextInfo : NotifyBase
        {
            string language;
            public string Language
            {
                get
                {
                    return language;
                }
                set
                {
                    if (Equals(value, language)) return;
                    language = value;
                    NotifyPropertyChanged(() => Language);
                }
            }
            string text;
            public string Text
            {
                get
                {
                    return text;
                }
                set
                {
                    if (Equals(value, text)) return;
                    text = value;
                    NotifyPropertyChanged(() => Text);
                }
            }

            bool isFocused;
            public bool IsFocused
            {
                get
                {
                    return isFocused;
                }
                set
                {
                    isFocused = value;
                    NotifyPropertyChanged(() => IsFocused);
                }
            }
        }
        private class SmartText : NotifyBase
        {
            public SolidColorBrush Background
            {
                get
                {
                    if (TextInfos.Count(x => string.IsNullOrWhiteSpace(x.Text)) > 0)
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22FF0000"));
                    }
                    if (TextInfos.GroupBy(x => x.Text).Count() == 1)
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#220000FF"));
                    }
                    if (Key.Length > 40)
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22EEE8AA"));
                    }
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2299DD00"));
                }
            }
            string key;
            public string Key
            {
                get
                {
                    return key;
                }
                set
                {
                    if (Equals(value, key)) return;
                    key = value;
                    NotifyPropertyChanged(() => Key);
                    NotifyPropertyChanged(() => Background);
                }
            }
            public List<TextInfo> TextInfos { get; private set; }

            public void SetTextInfos(List<TextInfo> value)
            {
                if (Equals(value, TextInfos)) return;
                TextInfos = value;
                if (TextInfos != null && TextInfos.Count > 0)
                {
                    foreach (var textInfo in TextInfos)
                    {
                        textInfo.PropertyChanged += TextInfo_PropertyChanged;
                    }
                }
                NotifyPropertyChanged(() => TextInfos);
                NotifyPropertyChanged(() => Background);
            }

            public void AddTextInfos(TextInfo textInfo)
            {
                textInfo.PropertyChanged += TextInfo_PropertyChanged;
                TextInfos.Add(textInfo);
                NotifyPropertyChanged(() => TextInfos);
                NotifyPropertyChanged(() => Background);
            }

            private void TextInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                NotifyPropertyChanged(() => Background);
            }

            bool isFocused;
            public bool IsFocused
            {
                get
                {
                    return isFocused;
                }
                set
                {
                    isFocused = value;
                    NotifyPropertyChanged(() => IsFocused);
                }
            }

            //bool isSelected;
            //public bool IsSelected
            //{
            //    get
            //    {
            //        return isSelected;
            //    }
            //    set
            //    {
            //        if (Equals(value, isSelected)) return;
            //        isSelected = value;
            //        NotifyPropertyChanged(() => IsSelected);
            //        NotifyPropertyChanged(() => BorderBackground);
            //    }
            //}

            //public SolidColorBrush BorderBackground
            //{
            //    get
            //    {
            //        if (IsSelected)
            //        {
            //            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66000000"));
            //        }
            //        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22000000"));
            //    }
            //}
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        public ICollectionViewLiveShaping SmartTextLiveShaping { get; private set; }
        private ICollectionView smartTextsView;

        private ObservableCollection<SmartText> smartTexts = null;
        
        string sourceFile = "";

        Translator translator = new Translator();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReadFile3();
        }

        private void ReadFile3()
        {
            var sourceFolder = @"..\..\..\Celsus.Client\Resources\Languages";
            sourceFile = Path.Combine(sourceFolder, "Strings.txt");
            List<SmartText> localSmartTexts = null;
            try
            {
                if (File.Exists(sourceFile) == true)
                {
                    string line = string.Empty;
                    string firstLine = string.Empty;
                    List<string> languages = new List<string>();
                    localSmartTexts = new List<SmartText>();
                    SmartText smartText = null;
                    TextInfo textInfo = null;
                    int languageCounter = 0;
                    try
                    {
                        using (var myReader = File.OpenText(sourceFile))
                        {

                            while (!myReader.EndOfStream)
                            {
                                line = myReader.ReadLine();
                                if (line.StartsWith("--"))
                                {

                                }
                                else if (line.StartsWith("language:"))
                                {
                                    languages.Add(line.Replace("language:", ""));
                                }
                                else if (line.StartsWith("key:"))
                                {
                                    languageCounter = -1;
                                    smartText = new SmartText
                                    {
                                        Key = line.Replace("key:", "")
                                    };
                                    smartText.SetTextInfos(new List<TextInfo>());
                                    localSmartTexts.Add(smartText);
                                }
                                else if (line.Contains("["))
                                {
                                    languageCounter++;
                                    textInfo = new TextInfo
                                    {
                                        Language = languages[languageCounter],
                                        Text = line.Replace("[", "").Replace("]", "")
                                    };
                                    smartText.AddTextInfos(textInfo);
                                }
                                else
                                {
                                    textInfo.Text += Environment.NewLine + line.Replace("[", "").Replace("]", "");
                                }
                            }
                        }
                        foreach (var item in localSmartTexts)
                        {
                            if (item.TextInfos == null)
                            {
                                item.SetTextInfos(new List<TextInfo>() { new TextInfo() { Language = "en", Text = "" }, new TextInfo() { Language = "tr", Text = "" } });
                            }
                            if (item.TextInfos.Count(x => x.Language == "en") == 0)
                            {
                                item.AddTextInfos(new TextInfo() { Language = "en", Text = "" });
                            }
                            if (item.TextInfos.Count(x => x.Language == "tr") == 0)
                            {
                                item.AddTextInfos(new TextInfo() { Language = "tr", Text = "" });
                            }
                        }
                        smartTexts = new ObservableCollection<SmartText>(localSmartTexts.OrderBy(x => x.Key));

                        smartTextsView = new CollectionViewSource() { Source = smartTexts }.View;

                        smartTextsView.SortDescriptions.Add(new SortDescription() { Direction = ListSortDirection.Ascending, PropertyName = "Key" });
                        smartTextsView.Filter = new Predicate<object>(Contains);
                        SmartTextLiveShaping = smartTextsView as ICollectionViewLiveShaping;
                        if (SmartTextLiveShaping.CanChangeLiveSorting)
                        {
                            SmartTextLiveShaping.LiveSortingProperties.Add("Key");
                            SmartTextLiveShaping.IsLiveSorting = true;
                        }
                        Ic.ItemsSource = (ICollectionView)SmartTextLiveShaping;

                        AddWord.IsEnabled = true;
                        BtnReadAndAdd.IsEnabled = true;
                        BtnSave.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public bool Contains(object de)
        {
            var smartText = de as SmartText;
            if (smartText.Key.IndexOf(TxtSearch.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (smartText.TextInfos != null && smartText.TextInfos.Count(x => x.Text.IndexOf(TxtSearch.Text, StringComparison.InvariantCultureIgnoreCase) >= 0) > 0)
            {
                return true;
            }
            return false;
        }

        private void BtnReadAndAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
                TxtWord.Text = Clipboard.GetText();
            AddWord_Click(null, null);
        }

        private async void AddWord_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(TxtWord.Text)))
            {

                var translated = await translator.Translate(TxtWord.Text);
                string withoutSpaces = "";
                var spl = TxtWord.Text.Trim().Replace(".", "").Replace(",", "").Replace("'", "").Replace("!", "").Replace(":", "").Split(' ');
                foreach (var item in spl)
                {
                    withoutSpaces += item.Substring(0, 1).ToUpper(new CultureInfo("en-EN"));
                    withoutSpaces += item.Substring(1);
                }

                if (withoutSpaces.Length > 40)
                {
                    withoutSpaces = withoutSpaces.Substring(0, 40);
                }

                if (smartTexts.Count(x => string.Compare(x.Key, withoutSpaces, true) == 0) > 0)
                {
                    MessageBox.Show("Word is already in list.");
                    Clipboard.SetText("{loc:Loc " + withoutSpaces + "}");
                    return;
                }
                var newSmartText = new SmartText { Key = withoutSpaces };
                newSmartText.SetTextInfos(new List<TextInfo>() { new TextInfo() { Language = "en", Text = TxtWord.Text }, new TextInfo() { Language = "tr", Text = translated } });
                smartTexts.Add(newSmartText);
                Clipboard.SetText("{loc:Loc " + withoutSpaces + "}");
                BtnSave_Click(null, null);
                TxtSearch.Text = withoutSpaces;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var writer = File.CreateText(sourceFile))
                {
                    var t = smartTexts.SelectMany(x => x.TextInfos);
                    var languages = t.Select(x => x.Language).Distinct().OrderBy(x => x).ToList();
                    foreach (var language in languages)
                    {
                        writer.WriteLine("language:" + language);
                    }
                    foreach (var item in smartTexts.OrderBy(x => x.Key).Distinct())
                    {
                        writer.WriteLine("key:" + item.Key);
                        foreach (var textInfo in item.TextInfos.OrderBy(x => x.Language))
                        {
                            writer.WriteLine("[" + textInfo.Text + "]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnFindSameWords_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder haswordCountTwoOrMore = new StringBuilder();

            foreach (var word in smartTexts)
            {
                if (smartTexts.Count(x => x.Key == word.Key) > 1)
                {
                    haswordCountTwoOrMore.AppendLine(word.Key);
                }
            }
            MessageBox.Show(haswordCountTwoOrMore.ToString());
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFileDiff_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            smartTextsView.Refresh();
            //foreach (var item in smartTexts)
            //{
            //    item.IsSelected = false;
            //}
            //var possibleSmartText = smartTexts.Where(x => x.Key.IndexOf(TxtSearch.Text) >= 0);
            //if (possibleSmartText.Any())
            //{
            //    foreach (var item in possibleSmartText)
            //    {
            //        item.IsSelected = true;
            //    }
            //    //possibleSmartText.First().IsFocused = true;
            //    Ic.ScrollIntoView(possibleSmartText.First());
            //}
            //var possibleTextInfos = smartTexts.SelectMany(x => x.TextInfos).Where(x => x.Text.IndexOf(TxtSearch.Text) >= 0);
            //if (possibleTextInfos.Any())
            //{
            //    foreach (var item in possibleTextInfos)
            //    {
            //        var smartText = smartTexts.FirstOrDefault(x => x.TextInfos.Contains(item));
            //        smartText.IsSelected = true;
            //    }
            //    //possibleTextInfos.First().IsFocused = true;
            //    {
            //        var smartText = smartTexts.FirstOrDefault(x => x.TextInfos.Contains(possibleTextInfos.First()));
            //        Ic.ScrollIntoView(smartText);
            //    }
            //}
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

    public class Translator
    {
        const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "de975f5d4e9d4121b21fac6928b73c54";

        public static readonly string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";

        public async Task<string> Translate(string TextToTranslate)
        {
            string textToTranslate = TextToTranslate.Trim();
            string fromLanguageCode = "en";
            string toLanguageCode = "tr";

            if (textToTranslate == "" || fromLanguageCode == toLanguageCode)
            {
                return "";
            }

            string endpoint = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "translate");
            string uri = string.Format(endpoint + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);

            object[] body = new System.Object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
                request.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
                var translations = result[0]["translations"];
                var translation = translations[0]["text"];

                return translation;
            }
        }



    }
}
