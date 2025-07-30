using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class MasterLeaveType
    {
        [Key]
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; }

        public List<DepartmentLeaves> DepartmentLeaves { get; set; }
        public List<LeaveBalance> LeaveBalances { get; set; }
        public List<LeaveRequest> LeaveRequests { get; set; }
    }
}
