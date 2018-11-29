﻿using Celsus.Client.Shared.Lex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public enum LicenseHelperStatusEnum
    {
        DontHaveLicense,
        GotError,
        HaveLicense,
        HaveTrialLicense,
        LicenseSuspended,
        LicenseGracePeriodOver,
        WrongProductKey,
        WrongProductId,
        ComputerClockError,
        ComputerClockCracked,
        LicenseExpired,
        LicenseCracked,
        TrialLicenseExpired,
    }
    public class LicenseHelper : BaseModel<LicenseHelper>, MustInit
    {


        private readonly object balanceLock = new object();

        private bool isInitted = false;
        private bool hasErrorInternal = false;
        private int statusCodeInternal = 0;

        LicenseHelperStatusEnum status;
        public LicenseHelperStatusEnum Status
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
                NotifyPropertyChanged(() => SubStatus);
                NotifyPropertyChanged(() => IsLicensed);
                SetupHelper.Instance.Invalidate();
            }
        }

        public string SubStatus
        {
            get
            {
                switch (Status)
                {
                    case LicenseHelperStatusEnum.DontHaveLicense:
                        return "DontHaveLicenseSubMessage";
                    case LicenseHelperStatusEnum.GotError:
                        return "GotErrorSubMessage";
                    case LicenseHelperStatusEnum.HaveLicense:
                        return "HaveLicenseSubMessage";
                    case LicenseHelperStatusEnum.HaveTrialLicense:
                        return "HaveTrialLicenseSubMessage";
                    case LicenseHelperStatusEnum.LicenseSuspended:
                        return "LicenseSuspendedSubMessage";
                    case LicenseHelperStatusEnum.LicenseGracePeriodOver:
                        return "LicenseGracePeriodOverSubMessage";
                    case LicenseHelperStatusEnum.WrongProductKey:
                        return "WrongProductKeySubMessage";
                    case LicenseHelperStatusEnum.WrongProductId:
                        return "WrongProductIdSubMessage";
                    case LicenseHelperStatusEnum.ComputerClockError:
                        return "ComputerClockErrorSubMessage";
                    case LicenseHelperStatusEnum.ComputerClockCracked:
                        return "ComputerClockCrackedSubMessage";
                    case LicenseHelperStatusEnum.LicenseExpired:
                        return "LicenseExpiredSubMessage";
                    case LicenseHelperStatusEnum.LicenseCracked:
                        return "LicenseCrackedSubMessage";
                    case LicenseHelperStatusEnum.TrialLicenseExpired:
                        return "TrialLicenseExpiredSubMessage";
                    default:
                        return "GotErrorSubMessage";
                }
            }
        }

        public bool IsLicensed
        {
            get
            {
                if (Status == LicenseHelperStatusEnum.HaveLicense || Status == LicenseHelperStatusEnum.HaveTrialLicense)
                {
                    return true;
                }
                return false;
            }
        }

        public void Init()
        {
            var licenseHelperInit = Init(@"Resources\Lex", out bool hasErrorInit, out int statusCodeInit);
            if (licenseHelperInit)
            {
                var checkAnyLicense = CheckAnyLicense();
            }
        }
        private bool Init(string sourceFolder, out bool hasError, out int statusCode)
        {
            lock (balanceLock)
            {
                if (isInitted)
                {
                    hasError = hasErrorInternal;
                    statusCode = statusCodeInternal;
                    return true;
                }
                try
                {
                    if (File.Exists("LexActivator.dll") == false)
                    {
                        try
                        {
                            File.Copy(Path.Combine(sourceFolder, "LexActivator_86.dll"), "LexActivator.dll", true);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "LexActivator_86 copy error");
                            hasError = hasErrorInternal;
                            statusCode = statusCodeInternal;
                            return false;
                        }

                    }
#if LA_ANY_CPU

                    try
                    {
                        if (File.Exists("LexActivator64.dll") == false)
                        {
                            File.Copy(Path.Combine(sourceFolder, "LexActivator_64.dll"), "LexActivator64.dll", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "LexActivator_64 copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }

                    try
                    {
                        File.Copy(Path.Combine(sourceFolder, "msvcp100_64.dll"), "msvcp100.dll", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "msvcp100_64 copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }

                    try
                    {
                        File.Copy(Path.Combine(sourceFolder, "msvcr100_64.dll"), "msvcr100.dll", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "msvcr100_64 copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }


#else
                    try
                    {
                        File.Copy(Path.Combine(sourceFolder, "msvcp100_86.dll"), "msvcp100.dll", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "msvcp100_86 copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }

                    try
                    {
                        File.Copy(Path.Combine(sourceFolder, "msvcr100_86.dll"), "msvcr100.dll", true);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "msvcr100_86 copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }


#endif

                    try
                    {
                        if (File.Exists("Product.dat") == false)
                        {
                            File.Copy(Path.Combine(sourceFolder, "Product.dat"), "Product.dat", true);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Product.dat copy error");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }

                    int setProductFile = 0;
                    try
                    {
                        setProductFile = LexActivator.SetProductFile("Product.dat");
                    }
                    catch (DllNotFoundException dllNotFoundException)
                    {
                        logger.Error(dllNotFoundException, $"SetProductFile error. Code: {setProductFile}");
                        logger.Error(dllNotFoundException.ToString());
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"SetProductFile error. Code: {setProductFile}");
                        hasError = hasErrorInternal;
                        statusCode = statusCodeInternal;
                        return false;
                    }

                    if (setProductFile != LexActivator.StatusCodes.LA_OK)
                    {
                        logger.Error($"SetProductFile error. Code: {setProductFile}");
                        hasError = true;
                        statusCode = setProductFile;
                        statusCodeInternal = statusCode;
                        hasErrorInternal = hasError;
                        isInitted = true;
                        return false;
                    }
                    var setProductId = LexActivator.SetProductId("f4a935cd-21ad-4bd2-8b5b-c929e0275340", LexActivator.PermissionFlags.LA_USER);
                    if (setProductId != LexActivator.StatusCodes.LA_OK)
                    {
                        logger.Error($"SetProductId error. Code: {setProductFile}");
                        hasError = true;
                        statusCode = setProductId;
                        statusCodeInternal = statusCode;
                        hasErrorInternal = hasError;
                        isInitted = true;
                        return false;
                    }
                    else
                    {
                        hasError = false;
                        statusCode = setProductId;
                    }

                    //var licenseHelperReset = Reset(out int statusCodeReset);

                    statusCodeInternal = statusCode;
                    hasErrorInternal = hasError;
                    isInitted = true;
                    return true;
                }
                catch (Exception ex)
                {
                    hasError = true;
                    statusCode = 0;
                    logger.Error(ex);
                }

                statusCodeInternal = statusCode;
                hasErrorInternal = hasError;
                isInitted = true;

                return false;
            }
        }



        private bool CheckLicense()
        {
            var isLicenseGenuine = LexActivator.IsLicenseGenuine();
            if (isLicenseGenuine == LexActivator.StatusCodes.LA_OK)
            {
                Status = LicenseHelperStatusEnum.HaveLicense;
                return true;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_SUSPENDED)
            {
                Status = LicenseHelperStatusEnum.LicenseSuspended;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_EXPIRED)
            {
                Status = LicenseHelperStatusEnum.LicenseExpired;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_GRACE_PERIOD_OVER)
            {
                Status = LicenseHelperStatusEnum.LicenseGracePeriodOver;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_FAIL)
            {
                Status = LicenseHelperStatusEnum.DontHaveLicense;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_E_PRODUCT_ID)
            {
                Status = LicenseHelperStatusEnum.WrongProductId;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_E_LICENSE_KEY)
            {
                Status = LicenseHelperStatusEnum.WrongProductKey;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_E_TIME)
            {
                Status = LicenseHelperStatusEnum.ComputerClockError;
                return false;
            }
            else if (isLicenseGenuine == LexActivator.StatusCodes.LA_E_TIME_MODIFIED)
            {
                Status = LicenseHelperStatusEnum.ComputerClockCracked;
                return false;
            }
            else
            {
                Status = LicenseHelperStatusEnum.DontHaveLicense;
                return false;
            }
        }
        private bool CheckAnyLicense()
        {
            if (CheckLicense() == false)
            {
                if (CheckTrialLicense() == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        TrialLicenseInfo trialLicenseInfo;
        public TrialLicenseInfo TrialLicenseInfo
        {
            get
            {
                if (trialLicenseInfo == null)
                {
                    trialLicenseInfo = GetTrialLicenseInfo();
                }
                return trialLicenseInfo;
            }
        }

        private TrialLicenseInfo GetTrialLicenseInfo()
        {
            var trialLicenseInfo = new TrialLicenseInfo();
            int status = 0;

            var sbFirstName = new StringBuilder();
            status = LexActivator.GetTrialActivationMetadata("FirstName", sbFirstName, 100);
            if (status != LexActivator.StatusCodes.LA_OK) return null;
            trialLicenseInfo.FirstName = sbFirstName.ToString();

            var sbLastName = new StringBuilder();
            status = LexActivator.GetTrialActivationMetadata("LastName", sbLastName, 100);
            if (status != LexActivator.StatusCodes.LA_OK) return null;
            trialLicenseInfo.LastName = sbLastName.ToString();

            var sbEMail = new StringBuilder();
            status = LexActivator.GetTrialActivationMetadata("eMail", sbEMail, 100);
            if (status != LexActivator.StatusCodes.LA_OK) return null;
            trialLicenseInfo.EMail = sbEMail.ToString();

            var sbOrganization = new StringBuilder();
            status = LexActivator.GetTrialActivationMetadata("Organization", sbOrganization, 100);
            if (status != LexActivator.StatusCodes.LA_OK) return null;
            trialLicenseInfo.Organization = sbOrganization.ToString();

            uint trialExpiryDate = 0;
            status = LexActivator.GetTrialExpiryDate(ref trialExpiryDate);
            if (status != LexActivator.StatusCodes.LA_OK) return null;
            var expireDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var TrialDueDate = expireDate.AddSeconds(trialExpiryDate).ToLocalTime();
            trialLicenseInfo.TrialDueDate = TrialDueDate;
            return trialLicenseInfo;

        }
        private bool CheckTrialLicense()
        {
            var isTrialGenuine = LexActivator.IsTrialGenuine();
            if (isTrialGenuine == LexActivator.StatusCodes.LA_OK)
            {
                Status = LicenseHelperStatusEnum.HaveTrialLicense;

                //var timeToExpire = expireDate.Subtract(DateTime.Now);
                GetTrialLicenseInfo();
                return true;
            }
            else if (isTrialGenuine == LexActivator.StatusCodes.LA_TRIAL_EXPIRED)
            {
                Status = LicenseHelperStatusEnum.TrialLicenseExpired;
                return false;
            }
            else if (isTrialGenuine == LexActivator.StatusCodes.LA_FAIL)
            {
                Status = LicenseHelperStatusEnum.DontHaveLicense;
                return false;
            }
            else if (isTrialGenuine == LexActivator.StatusCodes.LA_E_TIME)
            {
                Status = LicenseHelperStatusEnum.ComputerClockError;
                return false;
            }
            else if (isTrialGenuine == LexActivator.StatusCodes.LA_E_TIME_MODIFIED)
            {
                Status = LicenseHelperStatusEnum.ComputerClockCracked;
                return false;
            }
            else if (isTrialGenuine == LexActivator.StatusCodes.LA_E_PRODUCT_ID)
            {
                Status = LicenseHelperStatusEnum.WrongProductId;
                return false;
            }
            else
            {
                Status = LicenseHelperStatusEnum.DontHaveLicense;
                return false;
            }
        }
        public bool ActivateTrialLicense(out bool hasError, out int status, string firstName, string lastName, string eMail, string organization)
        {
            status = LexActivator.SetTrialActivationMetadata("FirstName", firstName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata FirstName Code {status}");
                hasError = true;
                return false;
            }
            status = LexActivator.SetTrialActivationMetadata("LastName", lastName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata LastName Code {status}");
                hasError = true;
                return false;
            }
            status = LexActivator.SetTrialActivationMetadata("eMail", eMail);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata eMail Code {status}");
                hasError = true;
                return false;
            }


            status = LexActivator.SetTrialActivationMetadata("Organization", organization);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetTrialActivationMetadata Organization Code {status}");
                hasError = true;
                return false;
            }

            status = LexActivator.ActivateTrial();
            var statusEnum = (StatusCodesEnum)status;
            if (statusEnum == StatusCodesEnum.LA_OK)
            {
                hasError = false;
                Init();
                return true;
            }
            else if (status == LexActivator.StatusCodes.LA_TRIAL_EXPIRED)
            {
                hasError = false;
                return false;
            }
            else
            {
                hasError = false;
                return false;
            }
        }

        public bool ActivateSerial(out bool hasError, out int status, string firstName, string lastName, string eMail, string organization, string serial)
        {
            status = LexActivator.SetActivationMetadata("FirstName", firstName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetActivationMetadata FirstName Code {status}");
                hasError = true;
                return false;
            }
            status = LexActivator.SetActivationMetadata("LastName", lastName);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetActivationMetadata LastName Code {status}");
                hasError = true;
                return false;
            }
            status = LexActivator.SetActivationMetadata("eMail", eMail);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetActivationMetadata eMail Code {status}");
                hasError = true;
                return false;
            }


            status = LexActivator.SetActivationMetadata("Organization", organization);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetActivationMetadata Organization Code {status}");
                hasError = true;
                return false;
            }

            status = LexActivator.SetLicenseKey(serial);
            if (status != LexActivator.StatusCodes.LA_OK)
            {
                logger.Warn($"SetActivationMetadata Serial Code {status}");
                hasError = true;
                return false;
            }

            status = LexActivator.ActivateLicense();
            var statusEnum = (StatusCodesEnum)status;
            if (statusEnum == StatusCodesEnum.LA_OK)
            {
                hasError = false;
                Init();
                return true;
            }
            else if (status == LexActivator.StatusCodes.LA_EXPIRED)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_SUSPENDED)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_REVOKED)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_FAIL)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_PRODUCT_ID)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_INET)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_VM)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_TIME)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_ACTIVATION_LIMIT)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_SERVER)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_CLIENT)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_LICENSE_KEY)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_LICENSE_TYPE)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_COUNTRY)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_IP)
            {
                hasError = false;
                return false;
            }
            else if (status == LexActivator.StatusCodes.LA_E_RATE_LIMIT)
            {
                hasError = false;
                return false;
            }
            else
            {
                hasError = false;
                return false;
            }
        }
        public bool Reset(out int statusCode)
        {

            var reset = LexActivator.Reset();
            if (reset == LexActivator.StatusCodes.LA_OK)
            {
                statusCode = reset;
                Init();
                return true;
            }
            else
            {
                statusCode = reset;
                Init();
                return false;
            }
        }
    }
}
