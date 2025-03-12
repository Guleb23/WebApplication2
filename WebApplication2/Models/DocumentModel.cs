using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class DocumentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string FileName { get; set; } // Имя файла
        public string Title { get; set; } // Название файла
        public string FilePath { get; set; } // Путь к файлу на сервере

        public int UserId { get; set; } // Внешний ключ для связи с пользователем

        [JsonIgnore] // Игнорируем циклическую ссылку
        public UserModel User { get; set; } // Навигационное свойство
    }
}
