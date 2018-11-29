using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("FileSystemItemLog", Schema = "Celsus")]
    public class FileSystemItemLogDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int FileSystemItemId { get; set; }
        public DateTime StartDate { get; set; }
        public FileSystemItemLogTypeEnum FileSystemItemLogTypeEnum { get; set; }
        public string Exception { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}
