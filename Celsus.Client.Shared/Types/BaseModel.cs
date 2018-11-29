using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Celsus.Client.Shared.Types
{
    public class BaseModel : INotifyPropertyChanged
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> exp)
        {
            var memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        protected void Run(Action method)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(method));
        }
    }

    public class BaseModel<T> : BaseModel where T : new()
    {
        private static readonly Lazy<T> _lazyInstance = new Lazy<T>(() => new T(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        public static T Instance
        {
            get
            {
                if (_lazyInstance.IsValueCreated == false)
                {
                    if (typeof(MustInit).IsAssignableFrom(typeof(T)))
                    {
                        var value = _lazyInstance.Value;
                        ((MustInit)value).Init();
                        return value;
                    }
                }

                return _lazyInstance.Value;
            }
        }
    }

    public interface MustInit
    {
        void Init();
    }
}
