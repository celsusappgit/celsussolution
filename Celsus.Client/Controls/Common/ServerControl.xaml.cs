using Celsus.Client.Shared.Types;
using Celsus.Client.Types;
using Celsus.Client.Types.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Celsus.Client.Controls.Common
{
    public partial class ServerControl : UserControl
    {
        public static readonly DependencyProperty ServerIdProperty = DependencyProperty.Register("ServerId", typeof(string), typeof(ServerControl), new PropertyMetadata(new PropertyChangedCallback(OnServerIdChangedAsync)));

        public string ServerId
        {
            get
            {
                return (string)GetValue(ServerIdProperty);
            }
            set
            {
                SetValue(ServerIdProperty, value);
            }
        }

        private static async void OnServerIdChangedAsync(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ServerControl;

            if (DesignerProperties.GetIsInDesignMode(ctrl))
            {
                ctrl.Txt.Text = "Server Name";
                return;
            }
            
            if (e.NewValue == null)
            {
                ctrl.Txt.Text = "";
            }
            var value = e.NewValue.ToString();
            var server = await Repo.Instance.GetServer(value.ToString());
            if (server == null)
            {
                ctrl.Txt.Text = "";
            }
            ctrl.Txt.Text = server.ServerName;
            if (string.Equals(server.ServerId, ComputerHelper.Instance.ServerId, StringComparison.InvariantCultureIgnoreCase))
            {
                ctrl.Txt.Text += " (" + TranslationSource.Instance["ThisComputer"] + ")";
            }
            else
            {
                ctrl.Txt.Text += " (" + TranslationSource.Instance["AnotherComputer"] + ")";
            }
        }


        public ServerControl()
        {
            InitializeComponent();
        }
    }
}
