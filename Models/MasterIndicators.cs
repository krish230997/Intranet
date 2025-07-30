using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class MasterIndicators
    {
		[Key]
		public int MasterIndicatorId { get; set; }
		public string MasterIndicatorType { get; set; }
		public string MasterIndicatorName { get; set; }
		public string Status { get; set; }
	}
}
