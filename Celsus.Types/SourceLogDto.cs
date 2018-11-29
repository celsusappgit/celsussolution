using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("SourceLog", Schema = "Celsus")]
    public class SourceLogDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime StartDate { get; set; }
        public SourceLogTypeEnum SourceLogTypeEnum { get; set; }
        public string Exception { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}
