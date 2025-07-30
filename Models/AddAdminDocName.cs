using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
	public class AddAdminDocName
	{
        public int Id { get; set; }

        [Required(ErrorMessage = "DocName is required.")]

        public string DocName { get; set; }
	}
}
