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

namespace Celsus.Client.Wpf.Controls.Main
{
    /// <summary>
    /// Interaction logic for MessageBorder.xaml
    /// </summary>
    public partial class MessageBorder : UserControl
    {
        public MessageBorder()
        {
            InitializeComponent();
        }

        public MessageBoxImage MessageBoxImage
        {
            get { return (MessageBoxImage)this.GetValue(MessageBoxImageProperty); }
            set { this.SetValue(MessageBoxImageProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxImageProperty = DependencyProperty.Register(
                                                                                    "MessageBoxImage",
                                                                                    typeof(MessageBoxImage),
                                                                                    typeof(MessageBorder),
                                                                                    new FrameworkPropertyMetadata(MessageBoxImagePropertyChanged));

        public object InnerContent
        {
            get { return (Object)this.GetValue(InnerContentProperty); }
            set { this.SetValue(InnerContentProperty, value); }
        }

        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register(
                                                                                    "InnerContent",
                                                                                    typeof(object),
                                                                                    typeof(MessageBorder),
                                                                                    new FrameworkPropertyMetadata(InnerContentPropertyChanged));

        private static void InnerContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as MessageBorder;
            ctrl.ContentControl.Content = e.NewValue;
        }

        private static void MessageBoxImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as MessageBorder;

            ctrl.PathAlert.Visibility = Visibility.Hidden;
            ctrl.PathCheckBox.Visibility = Visibility.Hidden;
            ctrl.PathInformation.Visibility = Visibility.Hidden;

            if (e.NewValue == null)
            {
                ctrl.PathAlert.Visibility = Visibility.Hidden;
                ctrl.PathCheckBox.Visibility = Visibility.Hidden;
                ctrl.PathInformation.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBoxImage messageBoxImage = (MessageBoxImage)e.NewValue;
                switch (messageBoxImage)
                {
                    case MessageBoxImage.None:
                        break;
                    case MessageBoxImage.Hand:
                        ctrl.PathAlert.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxImage.Question:
                        ctrl.PathInformation.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxImage.Warning:
                        ctrl.PathInformation.Visibility = Visibility.Collapsed;
                        break;
                    case MessageBoxImage.Information:
                        ctrl.PathCheckBox.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }

            if (ctrl.PathAlert.Visibility == Visibility.Visible)
            {
                ctrl.SpMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1D4D4"));
            }
            else if (ctrl.PathCheckBox.Visibility == Visibility.Visible)
            {
                ctrl.SpMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4F1D4"));
            }
            else if (ctrl.PathInformation.Visibility == Visibility.Visible)
            {
                ctrl.SpMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F7F7D4"));
            }
            else
            {
                ctrl.SpMain.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            }
        }
    }


}
