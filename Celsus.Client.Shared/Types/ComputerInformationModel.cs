using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class ComputerInformationModel : BaseModel
    {
        public bool IsRegistered
        {
            get
            {
                if (ServerRoleEnum == ServerRoleEnum.Indexer)
                {
                    if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense)
                    {
                        if (string.IsNullOrWhiteSpace(LicenseHelper.Instance.TrialLicenseInfo.Indexer01) == false)
                        {
                            if (LicenseHelper.Instance.TrialLicenseInfo.Indexer01.Equals(ServerId, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    else if (LicenseHelper.Instance.Status == LicenseHelperStatusEnum.HaveTrialLicense)
                    {
                        if (LicenseHelper.Instance.LicenseInfo.Indexers != null)
                        {
                            if (LicenseHelper.Instance.LicenseInfo.Indexers.Count(x => x.Equals(ServerId, StringComparison.InvariantCultureIgnoreCase)) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        ServerRoleEnum serverRoleEnum;
        public ServerRoleEnum ServerRoleEnum
        {
            get
            {
                return serverRoleEnum;
            }
            set
            {
                if (Equals(value, serverRoleEnum)) return;
                serverRoleEnum = value;
                NotifyPropertyChanged(() => ServerRoleEnum);
            }
        }
        string serverName;
        public string ServerName
        {
            get
            {
                return serverName;
            }
            set
            {
                if (Equals(value, serverName)) return;
                serverName = value;
                NotifyPropertyChanged(() => ServerName);
            }
        }
        string serverId;
        public string ServerId
        {
            get
            {
                return serverId;
            }
            set
            {
                if (Equals(value, serverId)) return;
                serverId = value;
                NotifyPropertyChanged(() => ServerId);
            }
        }
        string serverIP;
        public string ServerIP
        {
            get
            {
                return serverIP;
            }
            set
            {
                if (Equals(value, serverIP)) return;
                serverIP = value;
                NotifyPropertyChanged(() => ServerIP);
                NotifyPropertyChanged(() => ServerIPs);
            }
        }

        public List<string> ServerIPs
        {
            get
            {
                return ServerIP.Split(',').Select(x => x.Trim()).ToList();
            }
        }
    }
}
