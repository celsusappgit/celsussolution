using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class ComputerInformationModel : BaseModel
    {
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
