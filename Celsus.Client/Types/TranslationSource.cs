using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Celsus.Client.Types
{
    public class TranslationSource : INotifyPropertyChanged
    {
        public static TranslationSource Instance { get; } = new TranslationSource();

        public TranslationSource()
        {
            //if (IsDesign == false)
            {
                resManager = LocManager.Instance;
                currentCulture = Thread.CurrentThread.CurrentUICulture;
            }

        }
        //bool? isDesign;
        //private bool IsDesign
        //{
        //    get
        //    {
        //        if (isDesign == null)
        //        {
        //            isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        //        }
        //        return isDesign.Value;
        //    }
        //}

        private readonly LocManager resManager = null;
        private CultureInfo currentCulture = null;

        public string this[string key]
        {

            get
            {
                //if (IsDesign)
                //{
                //    return key;
                //}
                if (key.StartsWith("[]"))
                {
                    var localizedValue = resManager.GetString(key, currentCulture);
                    return string.Format(localizedValue, "OSMAN");
                }
                return resManager.GetString(key, currentCulture);
            }
        }

        public CultureInfo CurrentCulture
        {
            get { return currentCulture; }
            set
            {
                if (currentCulture != value)
                {
                    if (LocManager.Instance.Languages.Count(x => string.Compare(x.Key, currentCulture.TwoLetterISOLanguageName, StringComparison.InvariantCultureIgnoreCase) == 0 ) > 0)
                    {

                    }
                    else
                    {
                        return;
                    }
                    currentCulture = value;
                    var @event = PropertyChanged;
                    if (@event != null)
                    {
                        @event.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class LocExtension : Binding
    {
        public LocExtension(string name) : base("[" + name + "]")
        {
            Mode = BindingMode.OneWay;
            Source = TranslationSource.Instance;
        }
    }
}
