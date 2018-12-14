using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class ComputerHelper : BaseModel<ComputerHelper>
    {
        private static string GetProcessorId()
        {
            try
            {
                var managementObjectSearcher = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                var managementObjectCollection = managementObjectSearcher.Get();
                string id = string.Empty;
                foreach (var managementObject in managementObjectCollection)
                {
                    id = managementObject["ProcessorId"].ToString();
                    break;
                }
                return id;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetProcessorId");
                return string.Empty;
            }

        }

        public string ServerId
        {
            get
            {
                return GetServerId();
            }
        }
        private string GetMotherboardId()
        {
            try
            {
                ManagementObjectSearcher _mbs = new ManagementObjectSearcher("Select SerialNumber From Win32_BaseBoard");
                ManagementObjectCollection _mbsList = _mbs.Get();
                string _id = string.Empty;
                foreach (ManagementObject _mo in _mbsList)
                {
                    _id = _mo["SerialNumber"].ToString();
                    break;
                }

                return _id;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetMotherboardId");
                return string.Empty;
            }
        }
        private string GetServerName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetServerName");
                return string.Empty;
            }
        }

        public string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                // Machine not found...
            }
            return machineName;
        }


        public string GetIPAddressFromMachineName(string targetMachine)
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(targetMachine);
            if (addresslist == null || addresslist.Count() == 0)
            {
                return null;
            }
            return string.Join(", ", addresslist.Select(x => x.MapToIPv4().ToString()));
        }
        public string IPAddress
        {
            get
            {
                try
                {
                    IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                    if (localIPs == null || localIPs.Count() == 0)
                    {
                        return null;
                    }
                    return string.Join(", ", localIPs.Select(x => x.MapToIPv4().ToString()));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"GetIPAddress");
                    return string.Empty;
                }
            }
        }
        private string GetServerId()
        {
            var processorId = GetProcessorId();
            var motherboardId = GetMotherboardId();
            var servername = GetServerName();

            if (string.IsNullOrWhiteSpace(processorId) || string.IsNullOrWhiteSpace(motherboardId) || string.IsNullOrWhiteSpace(servername))
            {
                return null;
            }

            string id = string.Concat(processorId, motherboardId, servername);
            byte[] idAsByte = Encoding.UTF8.GetBytes(id);
            byte[] _checksum = null;
            using (var md5CryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                _checksum = md5CryptoServiceProvider.ComputeHash(idAsByte);
            }
            string _part1Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 0));
            string _part2Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 4));
            string _part3Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 8));
            string _part4Id = Base36Helper.Encode(BitConverter.ToUInt32(_checksum, 12));

            return $"{_part1Id}-{_part2Id}-{_part3Id}-{_part4Id}";
        }


    }

    public class Base36Helper
    {
        private const string _charList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly char[] _charArray = _charList.ToCharArray();

        public static long Decode(string input)
        {
            long _result = 0;
            double _pow = 0;
            for (int _i = input.Length - 1; _i >= 0; _i--)
            {
                char _c = input[_i];
                int pos = _charList.IndexOf(_c);
                if (pos > -1)
                    _result += pos * (long)Math.Pow(_charList.Length, _pow);
                else
                    return -1;
                _pow++;
            }
            return _result;
        }

        public static string Encode(ulong input)
        {
            StringBuilder _sb = new StringBuilder();
            do
            {
                _sb.Append(_charArray[input % (ulong)_charList.Length]);
                input /= (ulong)_charList.Length;
            } while (input != 0);

            return Reverse(_sb.ToString());
        }

        private static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
