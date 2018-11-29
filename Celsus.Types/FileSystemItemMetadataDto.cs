using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("FileSystemItemMetadata", Schema = "Celsus")]
    public class FileSystemItemMetadataDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FileSystemItemId { get; set; }
        public string Key { get; set; }
        public string StringValue { get; set; }
        public bool? BoolValue { get; set; }
        public int? IntValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public long? LongValue { get; set; }
        public string ValueType { get; set; }
    }
}
