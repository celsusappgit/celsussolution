using Celsus.Client.Wpf.Types;
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

namespace Celsus.Client.Wpf.Controls.Management.Setup
{
    public class NextItem
    {
        public string Param { get; set; }
        public string Name { get; set; }
    }

    public class WizardItem
    {
        public Lazy<UserControl> Control { get; set; }
        public string Name { get; set; }
        public List<NextItem> NextItems { get; set; }
    }

    [Description("SingleInstance")]
    public partial class WizardSQL : UserControl
    {
        private static readonly Lazy<WizardSQL> _lazyInstance = new Lazy<WizardSQL>(() => new WizardSQL());

        public static WizardSQL Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }
        public ConnectionInfo ConnectionInfo { get; set; } = new ConnectionInfo();

        private List<WizardItem> controls = new List<WizardItem>();
        private int index = 0;
        private bool initDone;
        private List<WizardItem> myList = new List<WizardItem>();

        public WizardSQL()
        {
            InitializeComponent();
        }

        public void Init()
        {
            if (initDone)
            {
                return;
            }
            controls.Add(new WizardItem()
            {
                Name = "WizardInit",
                Control = new Lazy<UserControl>(() => new WizardInit()),
                NextItems = new List<NextItem>()
                {
                     new NextItem() { Name="WizardCheckSqlServer", Param="" }
                }
            });
            controls.Add(new WizardItem()
            {
                Name = "WizardCheckSqlServer",
                Control = new Lazy<UserControl>(() => new WizardCheckSqlServer()),
                NextItems = new List<NextItem>()
                {
                     new NextItem() { Name="WizardCheckDatabase", Param="WizardCheckDatabase" },
                     new NextItem() { Name="WizardEnterConnection", Param="WizardEnterConnection" },
                     new NextItem() { Name="WizardInstallSQL", Param="WizardInstallSQL" }
                }
            });
            controls.Add(new WizardItem()
            {
                Name = "WizardEnterConnection",
                Control = new Lazy<UserControl>(() => new WizardEnterConnection()),
                NextItems = new List<NextItem>()
                {
                     new NextItem() { Name="WizardCheckDatabase", Param="WizardCheckDatabase" },
                     new NextItem() { Name="WizardEnterConnection", Param="WizardEnterConnection" },
                }
            });
            controls.Add(new WizardItem()
            {
                Name = "WizardCheckDatabase",
                Control = new Lazy<UserControl>(() => new WizardCheckDatabase()),
                NextItems = new List<NextItem>()
                {
                     new NextItem() { Name="WizardInit", Param="" },
                }
            });
            controls.Add(new WizardItem()
            {
                Name = "WizardInstallSQL",
                Control = new Lazy<UserControl>(() => new WizardInstallSQL()),
                NextItems = new List<NextItem>()
                {
                     new NextItem() { Name="WizardCheckDatabase", Param="WizardCheckDatabase" },
                     new NextItem() { Name="WizardInit", Param="" },
                }
            });
            Sett(controls.First());
            index = myList.Count - 2;
            initDone = true;
        }

        private async void Sett(WizardItem wizardItem)
        {
            ContentControl.Content = wizardItem.Control.Value;
            ContentControl.Tag = wizardItem;
            myList.Add(wizardItem);
            index = myList.Count - 2;

            (ContentControl.Content as dynamic).Main = this;
            BtnNext.IsEnabled = false;
            await (ContentControl.Content as dynamic).Do();
        }

        private async void Next(object sender, RoutedEventArgs e)
        {
            var loadedItem = (WizardItem)ContentControl.Tag;
            var nextParam = (ContentControl.Content as dynamic).NextParam;
            if (nextParam == null)
            {
                nextParam = "";
            }
            var n = loadedItem.NextItems.FirstOrDefault(x => x.Param == nextParam);
            var c = controls.SingleOrDefault(x => x.Name == n.Name);
            Sett(c);
            //if (ContentControl.Content.GetType() == typeof(WizardInit))
            //{
            //    ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardCheckSqlServer").Control.Value;
            //    myList.Add(controls.SingleOrDefault(x => x.Name == "WizardCheckSqlServer"));
            //    index = myList.Count - 2;
            //}
            //else if (ContentControl.Content.GetType() == typeof(WizardCheckSqlServer))
            //{
            //    var nextParam = (ContentControl.Content as dynamic).NextParam;
            //    if (string.IsNullOrWhiteSpace(nextParam) == false)
            //    {
            //        if (nextParam == "WizardEnterConnection")
            //        {
            //            ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardEnterConnection").Control.Value;
            //            myList.Add(controls.SingleOrDefault(x => x.Name == "WizardEnterConnection"));
            //            index = myList.Count - 2;
            //        }
            //        else if (nextParam == "WizardInstallSQL")
            //        {
            //            ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardInstallSQL").Control.Value;
            //            myList.Add(controls.SingleOrDefault(x => x.Name == "WizardInstallSQL"));
            //            index = myList.Count - 2;
            //        }
            //        else if (nextParam == "WizardCheckDatabase")
            //        {
            //            ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardCheckDatabase").Control.Value;
            //            myList.Add(controls.SingleOrDefault(x => x.Name == "WizardCheckDatabase"));
            //            index = myList.Count - 2;
            //        }
            //    }
            //}
            //else if (ContentControl.Content.GetType() == typeof(WizardEnterConnection))
            //{
            //    var nextParam = (ContentControl.Content as dynamic).NextParam;
            //    if (string.IsNullOrWhiteSpace(nextParam) == false)
            //    {
            //        if (nextParam == "WizardCheckDatabase")
            //        {
            //            ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardCheckDatabase").Control.Value;
            //            myList.Add(controls.SingleOrDefault(x => x.Name == "WizardCheckDatabase"));
            //            index = myList.Count - 2;
            //        }
            //        else if (nextParam == "WizardInstallSQL")
            //        {
            //            ContentControl.Content = controls.SingleOrDefault(x => x.Name == "WizardInstallSQL").Control.Value;
            //            myList.Add(controls.SingleOrDefault(x => x.Name == "WizardInstallSQL"));
            //            index = myList.Count - 2;
            //        }
            //    }

            //}
            (ContentControl.Content as dynamic).Main = this;
            BtnNext.IsEnabled = false;
            await (ContentControl.Content as dynamic).Do();

        }

        private async void Back(object sender, RoutedEventArgs e)
        {
            //if (myList.Count - 1 >= index)
            {
                if (myList.Count > 1)
                {
                    ContentControl.Content = myList[myList.Count - 2].Control.Value;
                    ContentControl.Tag = myList[myList.Count - 2];
                    BtnNext.IsEnabled = false;
                    await (ContentControl.Content as dynamic).Do();
                    myList.RemoveAt(myList.Count - 1);
                }
                //ContentControl.Content = myList[index].Control.Value;
                //index = index - 1;
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).Back();
        }
    }


}
