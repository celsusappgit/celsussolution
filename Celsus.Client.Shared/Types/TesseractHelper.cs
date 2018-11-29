using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum TesseractHelperStatusEnum
    {
        GotError = 1,
        EmptySetting = 2,
        Ok = 3
    }
    public class TesseractHelper : BaseModel<TesseractHelper>, MustInit
    {
        private readonly object balanceLock = new object();

        private bool isInitted = false;

        TesseractHelperStatusEnum status;
        public TesseractHelperStatusEnum Status
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

        //public TesseractHelper()
        //{
        //    SettingsHelper.Instance.PropertyChanged += SettingsHelper_PropertyChanged;
        //}

        internal void Invalidate()
        {
            CheckTesseractPath();
        }

        //private  void SettingsHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "TesseractPath")
        //    {
        //         CheckTesseractPath();
        //    }
        //}

        public  void Init()
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
             CheckTesseractPath();
        }

        private void CheckTesseractPath()
        {
            var tesseractPath = SettingsHelper.Instance.TesseractPath;
            if (string.IsNullOrWhiteSpace(tesseractPath))
            {
                Status = TesseractHelperStatusEnum.EmptySetting;
                return;
            }
            if (System.IO.Directory.Exists(tesseractPath)== false)
            {
                Status = TesseractHelperStatusEnum.GotError;
                return;
            }
            Status = TesseractHelperStatusEnum.Ok;
        }

        
    }
}
