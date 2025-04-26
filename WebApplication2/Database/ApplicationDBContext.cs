using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Database
{
    public class ApplicationDBContext:DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasOne(user => user.DataModel)
                .WithOne(data => data.UserModel)
                .HasForeignKey<PersonalDataModel>(data => data.UserId);
            modelBuilder.Entity<UserModel>()
           .HasOne(c => c.PaymentMethod) // У клиента один способ оплаты
           .WithMany(p => p.UserModels) // У способа оплаты много клиентов
           .HasForeignKey(c => c.PaymentMethodId) // Внешний ключ
           .OnDelete(DeleteBehavior.SetNull);// При удалении способа оплаты, PaymentMethodId станет null

            modelBuilder.Entity<UserModel>()
          .HasOne(c => c.GetDocsModel) 
          .WithMany(p => p.UserModels) 
          .HasForeignKey(c => c.GetDocsSposobId)
          .OnDelete(DeleteBehavior.SetNull);



            base.OnModelCreating(modelBuilder);
        }


        public DbSet<UserModel>  Users { get; set; }
        public DbSet<PersonalDataModel>  PersonalData { get; set; }
        public DbSet<PaymentMethodModel>  PaymentMethods { get; set; }
        public DbSet<GetDocsSposobModel>  GetDocsSposobModels { get; set; }
        public DbSet<DocumentModel>  Documents { get; set; }
        public DbSet<StatusModel>  Status { get; set; }
        public DbSet<OrderModel>  Orders { get; set; }
    }
}
