using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EmployeePerformance
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }

        public int EmployeeId { get; set; }
        public string Designation { get; set; }

        public DateTime DateofJoin { get; set; }
        public string ROName { get; set; }

        public DateTime DateofConfirmation { get; set; }

        public string RODesignation { get; set; }
        public string Qualification { get; set; }

        public int PreviousyearsofExp { get; set; }
        public string Category { get; set; }

        [Column("Sub_Category")]
        public string SubCategory { get; set; }

        public int? Weightage { get; set; }

        public decimal? Percentage_Achieved_Self { get; set; }

        public int Points_Scored_Self { get; set; }

        public decimal? Percentage_Achieved_RO { get; set; }

        public int Points_Scored_RO { get; set; }
    }
}
