using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required(ErrorMessage = "Role Name is required.")]
        public string RoleName { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }/* = DateTime.UtcNow;*/
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<User> Users { get; set; }
    }
}
