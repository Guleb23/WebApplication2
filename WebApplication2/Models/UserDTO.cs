namespace WebApplication2.Models
{
    public class UserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; } 
        public int PaymentMethodId { get; set; } 
        public int GetDocsSposobId { get; set; } = 1;
    }
}


