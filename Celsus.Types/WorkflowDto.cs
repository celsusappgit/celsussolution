using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Types
{
    [Table("Workflow", Schema = "Celsus")]
    public class WorkflowDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public int OrderNo { get; set; }
        public string InternalTypeName { get; set; }
        public string InternalTypeParameters { get; set; }
        public byte[] WfWorkflow { get; set; }

        bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (Equals(value, isActive)) return;
                isActive = value;
                NotifyPropertyChanged(() => IsActive);
            }
        }

        
    }
}