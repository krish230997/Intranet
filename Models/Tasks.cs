using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Pulse360.Models
{
    public class Tasks
    {
        [Key]
        public int TaskId { get; set; }

        // Correct Foreign Key Definition
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        // Correct Navigation Property
     
        public Projects Project { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        // Correct Foreign Key for Assigned User
        //[ForeignKey("User")]
        //public int UserId { get; set; }

        //public User User { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; }

        [StringLength(255)]
        public string? FilePath { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public List<TaskBoards> TaskBoards { get; set; }
        
    
        public List<TaskMembers> Taskmember { get; set; }

    }
}
