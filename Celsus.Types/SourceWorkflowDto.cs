using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("SourceWorkflow", Schema = "Celsus")]
    public class SourceWorkflowDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderNo { get; set; }
        public int SourceId { get; set; }
        public int WorkflowId { get; set; }
    }
}
