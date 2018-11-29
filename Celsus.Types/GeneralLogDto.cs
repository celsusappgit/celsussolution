using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("GeneralLog", Schema = "Celsus")]
    public class GeneralLogDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime LogDate { get; set; }
        public string Exception { get; set; }
        public GeneralLogTypeEnum GeneralLogTypeEnum { get; set; }
    }
}
