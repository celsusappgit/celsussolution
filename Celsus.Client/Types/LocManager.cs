using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Celsus.Client.Types
{
    public class LocManager
    {
        private List<SmartText> smartTexts = null;
        public static LocManager Instance { get; } = new LocManager();

        public LocManager()
        {
            smartTexts = ReadFileFromTextFile(@"Resources\Languages");
        }

        public IEnumerable<LanguageInfo> Languages
        {
            get
            {
                var t = smartTexts.SelectMany(x => x.TextInfos);
                return t.Select(x => x.Language).Distinct().Select(x => new LanguageInfo() { Key = x });
            }
        }

        bool? isDesign;
        private bool IsDesign
        {
            get
            {
                if (isDesign == null)
                {
                    isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
                }
                return isDesign.Value;
            }
        }

        internal string GetString(string key, CultureInfo currentCulture)
        {
            if (IsDesign)
            {
                if (smartTexts == null)
                {
                    smartTexts = ReadFileFromTextFile(@"C:\Users\efeo\source\repos\CelcusSolution\Celsus.Client\Resources\Languages\");
                }
                if (smartTexts != null)
                {
                    var smartTextDesigner = smartTexts.FirstOrDefault(x => string.Compare(x.Key, key, true) == 0);
                    if (smartTextDesigner == null)
                    {
                        return "NaN." + key;
                    }
                    return smartTextDesigner.TextInfos.FirstOrDefault()?.Text;
                }
                return key;
            }
            if (smartTexts == null)
            {
                return key;
            }
            var smartText = smartTexts.FirstOrDefault(x => string.Compare(x.Key, key, true) == 0);
            if (smartText == null)
            {
                return key;
            }
            if (currentCulture == null)
            {
                return key;
            }
            if (smartText.TextInfos == null)
            {
                return key;
            }
            var textInfo = smartText.TextInfos.FirstOrDefault(x => string.Compare(x.Language, currentCulture.TwoLetterISOLanguageName, true) == 0);
            if (textInfo == null)
            {
                textInfo = smartText.TextInfos.FirstOrDefault();
                if (textInfo == null)
                {
                    return key;
                }
                else
                {
                    return textInfo.Text;
                }
            }
            else
            {
                return textInfo.Text;
            }
        }


        private void ReadFile()
        {
            var sourceFolder = @"Resources\Languages";
            var sourceFile = Path.Combine(sourceFolder, "strings.json");
            string fileContent = null;
            try
            {
                if (File.Exists(sourceFile) == true)
                {
                    fileContent = File.ReadAllText(sourceFile);
                }
            }
            catch (Exception ex)
            {
            }
            smartTexts = JsonConvert.DeserializeObject<List<SmartText>>(fileContent);
        }

        private void ReadFile2()
        {
            var sourceFolder = @"Resources\Languages";
            var sourceFile = Path.Combine(sourceFolder, "Strings.csv");
            List<SmartText> localSmartTexts = null;
            try
            {
                if (File.Exists(sourceFile) == true)
                {
                    string line = string.Empty;
                    string firstLine = string.Empty;
                    List<string> languages = null;
                    localSmartTexts = new List<SmartText>();
                    try
                    {
                        using (var myReader = File.OpenText(sourceFile))
                        {
                            string[] splitted;
                            if (string.IsNullOrWhiteSpace(firstLine))
                            {
                                firstLine = myReader.ReadLine();
                                splitted = firstLine.Split('@');
                                languages = splitted.Skip(1).ToList();
                            }

                            while (!myReader.EndOfStream)
                            {
                                line = myReader.ReadLine();
                                if (line.StartsWith("--"))
                                {

                                }
                                else
                                {
                                    splitted = line.Split('@');
                                    if (splitted.Length > 1)
                                    {
                                        if (localSmartTexts.Count(x => string.Compare(x.Key, splitted.First(), true) == 0) > 0)
                                        {
                                            continue;
                                        }
                                        var newSmartText = new SmartText
                                        {
                                            Key = splitted.First(),
                                            TextInfos = new List<TextInfo>()
                                        };
                                        for (int i = 1; i < splitted.Length; i++)
                                        {
                                            var newTextInfo = new TextInfo
                                            {
                                                Language = languages[i - 1],
                                                Text = splitted[i]
                                            };
                                            newSmartText.TextInfos.Add(newTextInfo);
                                        }
                                        localSmartTexts.Add(newSmartText);
                                    }
                                }
                            }
                        }
                        smartTexts = localSmartTexts;
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

        private List<SmartText> ReadFileFromTextFile(string sourceFolder)
        {
            var sourceFile = Path.Combine(sourceFolder, "Strings.txt");
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
                                        Key = line.Replace("key:", ""),
                                        TextInfos = new List<TextInfo>()
                                    };
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
                                    smartText.TextInfos.Add(textInfo);
                                }
                                else
                                {
                                    textInfo.Text += Environment.NewLine + line.Replace("[", "").Replace("]", "");
                                }
                            }
                        }
                        return localSmartTexts;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        private class TextInfo
        {
            public string Language { get; set; }
            public string Text { get; set; }
        }

        private class SmartText
        {
            public string Key { get; set; }
            public List<TextInfo> TextInfos { get; set; }
        }
    }

    public class LanguageInfo
    {
        public string Key { get; set; }

        public string Value
        {
            get
            {
                return TranslationSource.Instance[Key];
            }
        }
    }
}
