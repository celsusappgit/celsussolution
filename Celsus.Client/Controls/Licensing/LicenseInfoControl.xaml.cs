using Celsus.Client.Shared.Types;
using System;
using System.Collections.Generic;
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

namespace Celsus.Client.Controls.Licensing
{
    /// <summary>
    /// Interaction logic for LicenseInfoControl.xaml
    /// </summary>
    public partial class LicenseInfoControl : UserControl
    {
        public LicenseInfoControl()
        {
            InitializeComponent();
            DataContext = LicenseHelper.Instance.TrialLicenseInfo;
        }
    }
}
