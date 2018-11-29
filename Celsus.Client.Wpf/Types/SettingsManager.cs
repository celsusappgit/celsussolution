using Celsus.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Wpf.Types
{
    public class SettingsManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        List<Setting> settings = null;
        readonly string settingsFile = null;
        public SettingsManager()
        {
            settingsFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Celsus"), "Settings.txt");
        }

        private static Lazy<SettingsManager> _lazyInstance = new Lazy<SettingsManager>(() => new SettingsManager());
        public static SettingsManager Instance
        {
            get
            {
                if (_lazyInstance == null)
                {
                    _lazyInstance = new Lazy<SettingsManager>(() => new SettingsManager());
                }
                return _lazyInstance.Value;
            }
        }

        public void LoadSettings()
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
                    logger.Error(ex, $"Exception has been thrown when reading settings file.");
                    settings = new List<Setting>();
                    return;
                }

                try
                {
                    settings = JsonConvert.DeserializeObject<List<Setting>>(text);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Exception has been thrown when deserializing settings file.");
                    settings = new List<Setting>();
                    return;
                }
            }
            else
            {
                settings = new List<Setting>();
            }
        }

        public void AddOrUpdateSetting(SettingName name, object settingValue)
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

            if (name == SettingName.ConnectionString)
            {
                SetupManager.Instance.InvalidateConnectionString();
            }
            if (name == SettingName.ImageMagickPath)
            {
                SetupManager.Instance.InvalidateIsOCRInstalled();
            }
            if (name == SettingName.TesseractPath)
            {
                SetupManager.Instance.InvalidateIsOCRInstalled();
            }
            if (name == SettingName.XPdfToolsPath)
            {
                SetupManager.Instance.InvalidateIsOCRInstalled();
            }
        }

        public T GetSetting<T>(SettingName name)
        {
            var oldSetting = settings.FirstOrDefault(x => x.Name == name);
            if (oldSetting == null)
            {
                return default(T);
            }
            return (T)oldSetting.Value;
        }

    }

    public class Setting
    {
        public SettingName Name { get; set; }
        public object Value { get; set; }
        public string TypeName { get; set; }
    }

    public enum SettingName
    {
        TesseractPath = 1,
        ImageMagickPath = 2,
        XPdfToolsPath = 3,
        ConnectionString = 4,
        Serial = 5,
    }
}
