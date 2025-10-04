using InventoryLibrary.Model.Account;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace InventoryLibrary.Data {
    
    public class MyDbContext : DbContext
    {

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<AGD> AGD { get; set; }
        public DbSet<Furniture> Furnitures { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ItemCondition> itemConditions{ get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Room> Rooms { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Accounts");

            modelBuilder.Entity<InventoryItem>()
                .ToTable("InventoryItems")
                .HasOne(i => i.Location)
                .WithMany()
                .HasForeignKey(i => i.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AGD>().ToTable("AGD");
            modelBuilder.Entity<Furniture>().ToTable("Furniture");

            modelBuilder.Entity<ItemType>()
                .ToTable("ItemTypes")
                .HasMany(t => t.InventoryItems)
                .WithOne(i => i.ItemType)
                .HasForeignKey(i => i.ItemTypeId);

            modelBuilder.Entity<ItemCondition>()
                .ToTable("ItemConditions")
                .HasMany(t => t.InventoryItems)
                .WithOne(i => i.ItemCondition)
                .HasForeignKey(i => i.ItemConditionId);

            modelBuilder.Entity<Room>()
                .ToTable("Rooms")
                .HasOne(r => r.Department)
                .WithMany(d => d.Rooms)
                .HasForeignKey(r => r.DepartmentId);

            modelBuilder.Entity<Department>()
                .ToTable("Departments");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemType>().HasData(
                    new ItemType { Id = 1, TypeName = "NoType" },
                    new ItemType { Id = 2, TypeName = "AGD" },
                    new ItemType { Id = 3, TypeName = "Furniture" },
                    new ItemType { Id = 4, TypeName = "Book" }
                );
            modelBuilder.Entity<ItemCondition>().HasData(
                new ItemCondition {Id = 1, ConditionName = "New"},
                new ItemCondition {Id = 2, ConditionName = "Good"},
                new ItemCondition {Id = 3, ConditionName = "Damaged"},
                new ItemCondition {Id = 4, ConditionName = "Lost"},
                new ItemCondition {Id = 5, ConditionName = "Disposed"}
            );
        }
    }
}
