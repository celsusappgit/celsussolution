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
    [Table("ServerRole", Schema = "Celsus")]
    public class ServerRoleDto : ModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ServerName { get; set; }
        public ServerRoleEnum ServerRoleEnum { get; set; }
        public string ServerId { get; set; }
        public string ServerIP { get; set; }
        public bool IsActive { get; set; }
    }
}
