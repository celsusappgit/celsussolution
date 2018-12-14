﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Shared.Types
{
    public class LicenseInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EMail { get; set; }
        public string Organization { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseKey { get; internal set; }
        public string LicenseUserEmail { get; internal set; }
        public string LicenseUserName { get; internal set; }
        public int? MaxAllowedIndexerRoleCount { get;  set; }
        public List<string> Indexers { get; internal set; }
        public void AddIndexer(string serverId)
        {
            if (Indexers==null)
            {
                Indexers = new List<string>();
            }
            Indexers.Add(serverId);
        }
    }
}
