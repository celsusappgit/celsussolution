using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum SettingsHelperStatusEnum
    {
        SettingsDidnotGet = 0,
        SettingsGetted = 1,
        DontHaveSettingsFile = 2
    }
    public class SettingsHelper : BaseModel<SettingsHelper>, MustInit
    {
        #region Members

        private readonly object balanceLock = new object();

        private bool isInitted = false;
        List<Setting> settings = null;
        readonly string settingsFile = null;

        #endregion

        public string ConnectionString
        {
            get
            {
                return GetSetting<string>(SettingNameEnum.ConnectionString);
            }
            set
            {
                AddOrUpdateSetting(SettingNameEnum.ConnectionString, value);
                NotifyPropertyChanged(() => ConnectionString);
                DatabaseHelper.Instance.Invalidate();
            }
        }

        public string TesseractPath
        {
            get
            {
                return GetSetting<string>(SettingNameEnum.TesseractPath);
            }
            set
            {
                AddOrUpdateSetting(SettingNameEnum.TesseractPath, value);
                NotifyPropertyChanged(() => TesseractPath);
                TesseractHelper.Instance.Invalidate();
            }
        }

        public string ImageMagickPath
        {
            get
            {
                return GetSetting<string>(SettingNameEnum.ImageMagickPath);
            }
            set
            {
                AddOrUpdateSetting(SettingNameEnum.ImageMagickPath, value);
                NotifyPropertyChanged(() => ImageMagickPath);
                ImageMagickHelper.Instance.Invalidate();
            }
        }

        public string XPdfToolsPath
        {
            get
            {
                return GetSetting<string>(SettingNameEnum.XPdfToolsPath);
            }
            set
            {
                AddOrUpdateSetting(SettingNameEnum.XPdfToolsPath, value);
                NotifyPropertyChanged(() => XPdfToolsPath);
                XPdfToolsHelper.Instance.Invalidate();
            }
        }

        public string Serial
        {
            get
            {
                return GetSetting<string>(SettingNameEnum.Serial);
            }
            set
            {
                AddOrUpdateSetting(SettingNameEnum.Serial, value);
                NotifyPropertyChanged(() => Serial);
            }
        }

        SettingsHelperStatusEnum status;
        public SettingsHelperStatusEnum Status
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
            }
        }

        public SettingsHelper()
        {
            settingsFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Settings.txt");
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                string text = string.Empty;
                try
                {
                    text = File.ReadAllText(settingsFile);
                }
                catch (Exception ex)
                {
                    settings = new List<Setting>();
                    Status = SettingsHelperStatusEnum.SettingsDidnotGet;
                    NotifyPropertyChanged("");
                    logger.Error(ex, $"Exception has been thrown when reading settings file.");
                    return;
                }

                try
                {
                    settings = JsonConvert.DeserializeObject<List<Setting>>(text);
                    Status = SettingsHelperStatusEnum.SettingsGetted;
                    NotifyPropertyChanged("");
                }
                catch (Exception ex)
                {
                    settings = new List<Setting>();
                    Status = SettingsHelperStatusEnum.SettingsDidnotGet;
                    NotifyPropertyChanged("");
                    logger.Error(ex, $"Exception has been thrown when deserializing settings file.");
                    return;
                }
            }
            else
            {
                Status = SettingsHelperStatusEnum.DontHaveSettingsFile;
                settings = new List<Setting>();
                NotifyPropertyChanged("");
            }
        }

        private void AddOrUpdateSetting(SettingNameEnum name, object settingValue)
        {
            var oldSetting = settings.FirstOrDefault(x => x.Name == name);
            if (oldSetting == null)
            {
                oldSetting = new Setting();
                settings.Add(oldSetting);
            }
            oldSetting.Name = name;
            oldSetting.Value = settingValue;
            oldSetting.TypeName = settingValue.GetType().ToString();

            string text = string.Empty;

            try
            {
                text = JsonConvert.SerializeObject(settings);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when serializing settings file.");
                return;
            }

            try
            {
                File.WriteAllText(settingsFile, text);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Exception has been thrown when writing settings file.");
                return;
            }


        }

        private T GetSetting<T>(SettingNameEnum name)
        {
            var oldSetting = settings.FirstOrDefault(x => x.Name == name);
            if (oldSetting == null)
            {
                return default(T);
            }
            return (T)oldSetting.Value;
        }

        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }
                LoadSettings();
                isInitted = true;
            }
        }

        public class Setting
        {
            public SettingNameEnum Name { get; set; }
            public object Value { get; set; }
            public string TypeName { get; set; }
        }

        public enum SettingNameEnum
        {
            TesseractPath = 1,
            ImageMagickPath = 2,
            XPdfToolsPath = 3,
            ConnectionString = 4,
            Serial = 5,
        }
    }


}
