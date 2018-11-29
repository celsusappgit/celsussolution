using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types.NonDatabase
{
    public class LicenseData
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Customer { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsTrial { get; set; }
        public string ServerId { get; set; }
        public List<LicenseProperty> LicenseProperties { get; set; }
    }

    public class LicenseProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
