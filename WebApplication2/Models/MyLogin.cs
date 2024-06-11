using System.ComponentModel.DataAnnotations;

namespace GatePass_Project.Models
{
    public class MyLogin
    {
        [Required]
        public string Non_slt_Id { get; set; }
        [Required]
        public string NIC { get; set; }
    }
}
