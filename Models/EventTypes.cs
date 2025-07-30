using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EventTypes
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(20)]
        public string Color { get; set; }
    }
}
