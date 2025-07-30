using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class LeaveBalance
    {
        [Key]
        public int LeaveBalanceId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("DepartmentLeaves")]
        public int DepartmentLeavesId { get; set; }
        public DepartmentLeaves DepartmentLeaves { get; set; }

        [ForeignKey("MasterLeaveType")]
        public int LeaveTypeId { get; set; }
        public MasterLeaveType MasterLeaveType { get; set; }

        public int TotalLeaves { get; set; }

        public int UsedLeaves { get; set; }

        // Computed property for remaining leaves
        [NotMapped] // This property will not be stored in the database
        public int RemainingLeaves => TotalLeaves - UsedLeaves;

        // Method to apply leave
        public void ApplyLeave(int numberOfDays)
        {
            if (RemainingLeaves < numberOfDays)
            {
                throw new InvalidOperationException("Insufficient leave balance.");
            }

            UsedLeaves += numberOfDays;
        }

        // Method to revert leave
        public void RevertLeave(int numberOfDays)
        {
            UsedLeaves -= numberOfDays;

            if (UsedLeaves < 0)
            {
                UsedLeaves = 0;
            }
        }

    }
}
