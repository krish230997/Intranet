using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EarningType
    {
        [Key]
        public int EarntypeId { get; set; }
        public string EarningName { get; set; }

        public List<Earning> Earnings { get; set; }
    }
}
