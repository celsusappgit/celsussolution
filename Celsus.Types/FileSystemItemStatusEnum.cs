using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    public enum FileSystemItemStatusEnum
    {
        Done = 1,
        Started = 2,
        Omitted = 3,
        StopedWithError = 1001,
        
    }
}
