using System;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum XPdfToolsHelperStatusEnum
    {
        GotError = 1,
        EmptySetting = 2,
        Ok = 3
    }
    public class XPdfToolsHelper : BaseModel<XPdfToolsHelper>, MustInit
    {
        private readonly object balanceLock = new object();

        private bool isInitted = false;

        XPdfToolsHelperStatusEnum status;
        public XPdfToolsHelperStatusEnum Status
        {
            get
            {
                return status;
            }
            set
            {
                if (Equals(value, status)) return;
                status = value;
                NotifyPropertyChanged(() => Status);
                IndexerHelper.Instance.UpdateStatus();
            }
        }

        //public XPdfToolsHelper()
        //{
        //    SettingsHelper.Instance.PropertyChanged += SettingsHelper_PropertyChanged;
            
        //}

        //private void SettingsHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "XPdfToolsPath")
        //    {
        //        CheckXPdfToolsPath();
        //    }
        //}
        internal void Invalidate()
        {
            CheckXPdfToolsPath();
        }

        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }

            }
            CheckAll();
            lock (balanceLock)
            {
                isInitted = true;
            }
        }

        private void CheckAll()
        {
            CheckXPdfToolsPath();
        }

        private void CheckXPdfToolsPath()
        {
            var XPdfToolsPath = SettingsHelper.Instance.XPdfToolsPath;
            if (string.IsNullOrWhiteSpace(XPdfToolsPath))
            {
                Status = XPdfToolsHelperStatusEnum.EmptySetting;
                return;
            }
            if (System.IO.Directory.Exists(XPdfToolsPath) == false)
            {
                Status = XPdfToolsHelperStatusEnum.GotError;
                return;
            }
            Status = XPdfToolsHelperStatusEnum.Ok;
        }

        
    }
}
