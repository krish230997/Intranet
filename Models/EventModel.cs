using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EventModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Date { get; set; }
        public int EventTypeId { get; set; }

        public string Status { get; set; }
    }
}
