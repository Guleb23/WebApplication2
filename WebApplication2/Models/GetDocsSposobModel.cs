
namespace WebApplication2.Models
{
    public class GetDocsSposobModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserModel>? UserModels { get; set; }
    }
}
