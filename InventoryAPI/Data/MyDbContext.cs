using InventoryAPI.Model.Account;
using InventoryAPI.Model.Inventory;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Data {
    
    public class MyDbContext : DbContext
    {

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Furniture> Furnitures { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<InventoryItem>().ToTable("InventoryItems");
            modelBuilder.Entity<Computer>().ToTable("Computers");
            modelBuilder.Entity<Furniture>().ToTable("Furniture");

            base.OnModelCreating(modelBuilder);
        }
    }
}
