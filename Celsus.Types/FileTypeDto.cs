using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("FileType", Schema = "Celsus")]
    public class FileTypeDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Regex { get; set; }
        public byte[] Workflow { get; set; }
        public bool IsActive { get; set; }
        public int SourceId { get; set; }
    }
}
