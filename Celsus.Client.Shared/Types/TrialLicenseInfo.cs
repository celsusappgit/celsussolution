using System;

namespace Celsus.Client.Shared.Types
{
    public class TrialLicenseInfo
    {
        public string FirstName { get; internal set; }
        public string Organization { get; internal set; }
        public string EMail { get; internal set; }
        public string LastName { get; internal set; }
        public DateTime TrialDueDate { get; internal set; }
    }
}