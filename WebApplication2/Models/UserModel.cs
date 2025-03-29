
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int PaymentMethodId { get; set; } = 1;
        public int GetDocsSposobId { get; set; } = 1;





        [JsonIgnore]
        public PersonalDataModel? DataModel { get; set; }
        [JsonIgnore]
        public GetDocsSposobModel? GetDocsModel { get; set; }
        [JsonIgnore]
        public PaymentMethodModel? PaymentMethod { get; set; }
        [JsonIgnore] 
        public List<DocumentModel> Documents { get; set; } = new List<DocumentModel>();
    }
}
