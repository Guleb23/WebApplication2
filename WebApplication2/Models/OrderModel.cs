using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class OrderModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }

        public int ManagerId { get; set; }
        public int GeodesistId { get; set; }
        public int KadastrEngineerId { get; set; }

        public int StatusId { get; set; }

        public string Address { get; set; }
        public string KadastrNumber { get; set; }

        public string TypeOfWork { get; set; }

        public decimal Price { get; set; }

        [ForeignKey("UserId")]
        public UserModel User { get; set; }

        [ForeignKey("ManagerId")]
        public UserModel Manager { get; set; }

        [ForeignKey("GeodesistId")]
        public UserModel Geodesist { get; set; }

        [ForeignKey("KadastrEngineerId")]
        public UserModel KadastrEngineer { get; set; }
        [ForeignKey("StatusId")]
        public StatusModel Status { get; set; }
    }
}
