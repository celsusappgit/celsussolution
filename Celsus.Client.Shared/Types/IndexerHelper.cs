using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Celsus.Client.Shared.Types
{
    public enum IndexerHelperStatusEnum
    {
        Unknown=0,
        GotError = 1,
        EmptySetting = 2,
        Ok = 3
    }
    public class IndexerHelper : BaseModel<IndexerHelper>, MustInit
    {
        private readonly object balanceLock = new object();

        private bool isInitted = false;

        IndexerHelperStatusEnum status;
        public IndexerHelperStatusEnum Status
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
                SetupHelper.Instance.Invalidate();
            }
        }

        public IndexerHelper()
        {
            //TesseractHelper.Instance.PropertyChanged += TesseractHelper_PropertyChanged;
            //ImageMagickHelper.Instance.PropertyChanged += ImageMagickHelper_PropertyChanged;
            //XPdfToolsHelper.Instance.PropertyChanged += XPdfToolsHelper_PropertyChanged;
            //ServiceHelper.Instance.PropertyChanged += ServiceHelper_PropertyChanged;
        }

        //private void ServiceHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Status")
        //    {
        //        UpdateStatus();
        //    }
        //}

        public void UpdateStatus()
        {
            if (TesseractHelper.Instance.Status == TesseractHelperStatusEnum.Ok
                                &&
                                ImageMagickHelper.Instance.Status == ImageMagickHelperStatusEnum.Ok
                                &&
                                XPdfToolsHelper.Instance.Status == XPdfToolsHelperStatusEnum.Ok
                                &&
                                ServiceHelper.Instance.Status == ServiceHelperStatusEnum.Ok)
            {
                Status = IndexerHelperStatusEnum.Ok;
            }
            if (TesseractHelper.Instance.Status == TesseractHelperStatusEnum.GotError
                ||
                ImageMagickHelper.Instance.Status == ImageMagickHelperStatusEnum.GotError
                ||
                XPdfToolsHelper.Instance.Status == XPdfToolsHelperStatusEnum.GotError
                ||
                ServiceHelper.Instance.Status == ServiceHelperStatusEnum.GotError)
            {
                Status = IndexerHelperStatusEnum.GotError;
            }
        }

        //private void XPdfToolsHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Status")
        //    {
        //        UpdateStatus();
        //    }
        //}

        //private void ImageMagickHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Status")
        //    {
        //        UpdateStatus();
        //    }
        //}

        //private void TesseractHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Status")
        //    {
        //        UpdateStatus();
        //    }
        //}


        public void Init()
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    return;
                }

            }
            lock (balanceLock)
            {
                UpdateStatus();
                isInitted = true;
            }
        }



    }
}
