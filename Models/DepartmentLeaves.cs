using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class DepartmentLeaves
    {
        [Key]
        public int DepartmentLeavesId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        [ForeignKey("MasterLeaveType")]
        public int LeaveTypeId { get; set; }
        public MasterLeaveType MasterLeaveType { get; set; }
        //public string LeaveType { get; set; } 
        public int LeavesCount { get; set; }
        public string Status { get; set; }
    }
}
