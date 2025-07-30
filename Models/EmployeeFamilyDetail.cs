using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EmployeeFamilyDetail
    {
        [Key]
        public int FamilyDetailId { get; set; }
        public string Name { get; set; }
        public string Relation { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string phone { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
