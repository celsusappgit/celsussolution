using Celsus.Client.Admin.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Celsus.Client.Admin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Id.Text = Guid.NewGuid().ToString().ToUpper();
            Id.IsEnabled = false;
            CreatedBy.Text = Environment.UserName;
            CreatedBy.IsEnabled = false;
            CreatedDate.SelectedDate = DateTime.Now;
            CreatedDate.IsEnabled = false;
        }

        public void D()
        {
            byte[] certPrivateKeyData = null;
            var assembly = Assembly.GetExecutingAssembly();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                assembly.GetManifestResourceStream("Celsus.Client.Admin.Resources.EbdysCert.pfx").CopyTo(memoryStream);
                certPrivateKeyData = memoryStream.ToArray();
            }

            Celsus.Types.NonDatabase.LicenseData license = new Celsus.Types.NonDatabase.LicenseData();
            license.CreatedBy = CreatedBy.Text;
            license.CreatedDate = CreatedDate.SelectedDate.Value;
            license.Customer = Customer.Text;
            license.Description = Description.Text;
            license.ExpireDate = ExpireDate.SelectedDate.Value;
            license.Id = new Guid(Id.Text);
            license.IsTrial = IsTrial.IsChecked.GetValueOrDefault();
            license.ServerId = ServerId.Text;
            license.LicenseProperties = new List<Celsus.Types.NonDatabase.LicenseProperty>();
            AddProperty(license, LicencePropertiesKey1, LicencePropertiesValue1);
            AddProperty(license, LicencePropertiesKey2, LicencePropertiesValue2);
            AddProperty(license, LicencePropertiesKey3, LicencePropertiesValue3);
            AddProperty(license, LicencePropertiesKey4, LicencePropertiesValue4);
            AddProperty(license, LicencePropertiesKey5, LicencePropertiesValue5);
            var serial = SignHandler.GenerateSignedSerial(license, certPrivateKeyData);
            Clipboard.SetText(serial);
            MessageBox.Show(serial);
            
        }

        private void AddProperty(Celsus.Types.NonDatabase.LicenseData license, TextBox t1, TextBox t2)
        {
            if (string.IsNullOrWhiteSpace(t1.Text) == false && string.IsNullOrWhiteSpace(t2.Text) == false)
            {
                license.LicenseProperties.Add(new Celsus.Types.NonDatabase.LicenseProperty() { Name = t1.Text, Value = t2.Text });
            }
        }

        private void IsTrial_Checked(object sender, RoutedEventArgs e)
        {
            if (IsTrial.IsChecked.GetValueOrDefault())
            {
                ExpireDate.SelectedDate = DateTime.Now.AddDays(30);
                ExpireDate.IsEnabled = false;
            }
            else
            {
                ExpireDate.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            D();
        }
    }
}
