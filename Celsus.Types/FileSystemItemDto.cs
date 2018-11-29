using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("FileSystemItem", Schema = "Celsus")]
    public class FileSystemItemDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int? FileTypeId { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public DateTime OperationDate { get; set; }
        public FileSystemItemStatusEnum FileSystemItemStatusEnum { get; set; }
        public bool IsDirectory { get; set; }
        public int? ParentId { get; set; }
    }
}
