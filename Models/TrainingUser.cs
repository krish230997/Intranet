using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class TrainingUser
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Training")]
        public int TrainingId { get; set; }
        public Training Training { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [NotMapped]
        public string? ProfilePicture { get; set; }  // Can be retrieved from User entity
    }
}

