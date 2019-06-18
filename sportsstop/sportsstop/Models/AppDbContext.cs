using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // To maintain data integrity. Do not delete if the row is foreign key to some other table
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.Restrict;

            // To ensure all the decimal datatypes has two decimal places of precision
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                                                       .SelectMany(t => t.GetProperties())
                                                       .Where(p => p.GetType() == typeof(decimal)))
            {
                property.Relational().ColumnType = "decimal(18,2)";
            }

            modelBuilder.Entity<CartItem>().HasKey(ci => new { ci.CartId, ci.ItemId });

            // To set default value of orderDate to current timestamp
            modelBuilder.Entity<Order>()
                .Property(b => b.OrderDate)
                .HasDefaultValueSql("CONVERT(date, GETDATE())");

            //modelBuilder.Entity<Item>().HasQueryFilter(object => object);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemComment> ItemComments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var changeSet = ChangeTracker.Entries<DBObject>();

        //    if(changeSet != null)
        //    {

        //    }
        //    return await base.SaveChangesAsync();
        //}
    }

    
}
