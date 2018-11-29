using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum SetupHelperStatusEnum
    {
        GotError = 0,
        SetupHelperOk = 1
    }
    public class SetupHelper : BaseModel<SetupHelper>, MustInit
    {
        private readonly object balanceLock = new object();

        private bool isInitted = false;

        SetupHelperStatusEnum status;
        public SetupHelperStatusEnum Status
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
            }
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

        //public SetupHelper()
        //{
        //    IndexerHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        //    LicenseHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        //    RolesHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        //    DatabaseHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        //}


        //private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    CheckAll();
        //}

        internal void Invalidate()
        {
            CheckAll();
        }
        private void CheckAll()
        {
            if (RolesHelper.Instance.Status != RolesHelperStatusEnum.RolesGetted)
            {
                // error
                Status = SetupHelperStatusEnum.GotError;
                return;
            }
            else
            {
                if (RolesHelper.Instance.DatabaseRoleCount == 1 && RolesHelper.Instance.IndexerRoleCount==1)
                {

                }
                else
                {
                    // error
                    Status = SetupHelperStatusEnum.GotError;
                    return;
                }
            }
            if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveLicense || LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense)
            {

            }
            else
            {
                // error
                Status = SetupHelperStatusEnum.GotError;
                return;
            }
            if (DatabaseHelper.Instance.Status != DatabaseHelperStatusEnum.CelsusDatabaseVersionOk)
            {
                // error
                Status = SetupHelperStatusEnum.GotError;
                return;
            }
            Status = SetupHelperStatusEnum.SetupHelperOk;

        }

        
    }
}
