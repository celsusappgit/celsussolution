using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum ImageMagickHelperStatusEnum
    {
        GotError = 1,
        EmptySetting = 2,
        Ok = 3
    }
    public class ImageMagickHelper : BaseModel<ImageMagickHelper>, MustInit
    {
        private readonly object balanceLock = new object();

        private bool isInitted = false;

        ImageMagickHelperStatusEnum status;
        public ImageMagickHelperStatusEnum Status
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

        public ImageMagickHelper()
        {
            //SettingsHelper.Instance.PropertyChanged += SettingsHelper_PropertyChanged;
        }

        //private void SettingsHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "ImageMagickPath")
        //    {
        //        CheckImageMagickPath();
        //    }
        //}

        internal void Invalidate()
        {
            CheckImageMagickPath();
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
            CheckImageMagickPath();
        }

        private void CheckImageMagickPath()
        {
            var ImageMagickPath = SettingsHelper.Instance.ImageMagickPath;
            if (string.IsNullOrWhiteSpace(ImageMagickPath))
            {
                Status = ImageMagickHelperStatusEnum.EmptySetting;
                return;
            }
            if (System.IO.Directory.Exists(ImageMagickPath) == false)
            {
                Status = ImageMagickHelperStatusEnum.GotError;
                return;
            }
            Status = ImageMagickHelperStatusEnum.Ok;
        }

       
    }
}
