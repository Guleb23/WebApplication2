namespace WebApplication2.Models
{
    public class PaymentMethodModel
    {
        public int Id { get; set; } // Уникальный идентификатор способа оплаты
        public string Name { get; set; } // Название способа оплаты (например, "Карта", "Наличные", "Криптовалюта")

        // Навигационное свойство для связи с клиентами
        public ICollection<UserModel>? UserModels { get; set; }
    }
}
