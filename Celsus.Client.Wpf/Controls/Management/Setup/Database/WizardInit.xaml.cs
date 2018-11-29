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

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    /// <summary>
    /// Interaction logic for WizardInit.xaml
    /// </summary>
    public partial class WizardInit : UserControl
    {
        public WizardSQL Main { get; set; }
        public string NextParam { get; set; }
        public WizardInit()
        {
            InitializeComponent();
        }

        public async Task Do()
        {
            Main.BtnNext.IsEnabled = true;
            return ;
        }
    }
}
