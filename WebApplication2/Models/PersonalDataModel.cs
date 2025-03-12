using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class PersonalDataModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Seria { get; set; }
        public string? Nomer { get; set; }
        public string? DateVidachi { get; set; }
        public string? Propiska { get; set; }
        public string? WhoVidal { get; set; }
        public string? SNILS { get; set; }

        public int UserId { get; set; }


        [JsonIgnore]
        public UserModel UserModel { get; set; }

        
    }
}
