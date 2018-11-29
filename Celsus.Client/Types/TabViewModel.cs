using Celsus.Client.Shared.Types;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Celsus.Client.Types
{
    public class TabViewModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public TabViewModel()
        {
            //this.mainViewModel = mainViewModel;

            //Action<object> addAction = new Action<object>(x => { this.mainViewModel.AddItem(this); });
            //Predicate<object> addPred = new Predicate<object>(x => { return this.mainViewModel.Tabs.Count < 5; });

            //this.AddItemCommand = new DelegateCommand(addAction, addPred);

            //Action<object> removeAction = new Action<object>(x => { this.mainViewModel.RemoveItem(this); });
            //Predicate<object> removePred = new Predicate<object>(x => { return this.mainViewModel.Tabs.Count > 1; });

            //this.RemoveItemCommand = new DelegateCommand(removeAction, removePred);
        }

        public object Header
        {
            get;
            set;
        }

        public object Content { get; set; }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        ICommand removeItemCommand;
        public ICommand RemoveItemCommand
        {
            get
            {
                if (removeItemCommand == null)
                    removeItemCommand = new RelayCommand(param => RemoveItem(param), param => { return FirstWindowModel.Instance.TabItems.Count > 1; });
                return removeItemCommand;
            }
        }

        private void RemoveItem(object param)
        {
            FirstWindowModel.Instance.CloseTabItem(this);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}