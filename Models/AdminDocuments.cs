using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class AdminDocuments
    {
        [Key]
        public int AdminDocId { get; set; }
        public string Email { get; set; }
        public string DocName { get; set; }
        public string DocFile { get; set; }
    }
}
