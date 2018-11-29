using Celsus.Types;
using System.Collections.Generic;

namespace Celsus.Worker
{
    /// <summary>
    /// Job configuration properties
    /// </summary>
    public class JobConfigDetails
    {
        public bool Enabled { get; set; }
        public string Cron { get; set; }
        public string TypeName { get; set; }
        public SourceDto Source { get; internal set; }
        public List<FileTypeDto> FileTypes { get; internal set; }
        public string Id { get; internal set; }
    }

}
