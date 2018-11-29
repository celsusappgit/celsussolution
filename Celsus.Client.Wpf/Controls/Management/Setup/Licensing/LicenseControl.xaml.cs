using Celsus.Client.Wpf.Types;
using Celsus.Types.NonDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
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

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    /// <summary>
    /// Interaction logic for LicenseControl.xaml
    /// </summary>
    [Description("SingleInstance")]

    public partial class LicenseControl : UserControl, INotifyPropertyChanged
    {
        private bool initDone;

        string serial;
        public string Serial
        {
            get
            {
                return serial;
            }
            set
            {
                if (Equals(value, serial)) return;
                serial = value;
                NotifyPropertyChanged(() => Serial);
            }
        }
        public LicenseControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private static readonly Lazy<LicenseControl> _lazyInstance = new Lazy<LicenseControl>(() => new LicenseControl());

        public static LicenseControl Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        public void Init()
        {
            if (initDone)
            {
                return;
            }

            TxtServerId.Value = SetupManager.Instance.ServerId;

            Serial = SettingsManager.Instance.GetSetting<string>(SettingName.Serial);

            CheckSerial(Serial);

            initDone = true;
        }

        private void CheckNewSerial(object sender, RoutedEventArgs e)
        {
            //SettingsManager.Instance.AddOrUpdateSetting(SettingName.Serial, TxtSerial.Text);

            CheckSerial(Serial);
        }

        private void CheckSerial(string newSerial)
        {

            SpLicenseOk.Visibility = Visibility.Collapsed;
            SpLicenseNoLicense.Visibility = Visibility.Collapsed;
            SpLicenseError.Visibility = Visibility.Collapsed;

            ExpireDateError.Visibility = Visibility.Collapsed;
            ExpireDateOk.Visibility = Visibility.Collapsed;

            ServerIdError.Visibility = Visibility.Collapsed;
            ServerIdOk.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(newSerial))
            {
                SpLicenseNoLicense.Visibility = Visibility.Visible;
                return;
            }

            var result = SetupManager.Instance.CheckSerial(newSerial, out LicenseData licenseInformation);

            BrdLicenseInformation.DataContext = licenseInformation;

            if (result == "NoLicense")
            {
                SpLicenseNoLicense.Visibility = Visibility.Visible;
            }

            if (result == "LicenseError")
            {
                SpLicenseError.Visibility = Visibility.Visible;
            }

            if (result == "LicenseOk")
            {
                SpLicenseOk.Visibility = Visibility.Visible;
            }

            if (licenseInformation != null && licenseInformation.ExpireDate < DateTime.UtcNow)
            {
                ExpireDateError.Visibility = Visibility.Visible;
                ExpireDateOk.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExpireDateError.Visibility = Visibility.Collapsed;
                ExpireDateOk.Visibility = Visibility.Visible;
            }

            if (licenseInformation != null && SetupManager.Instance.ServerId != licenseInformation.ServerId)
            {
                ServerIdError.Visibility = Visibility.Visible;
                ServerIdOk.Visibility = Visibility.Collapsed;
            }
            else
            {
                ServerIdError.Visibility = Visibility.Collapsed;
                ServerIdOk.Visibility = Visibility.Visible;
            }


        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;

        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.AddOrUpdateSetting(SettingName.Serial, Serial);
            //(App.Current.MainWindow as MainWindow).CheckSerial();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
