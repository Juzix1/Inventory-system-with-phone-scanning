using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Model.StockTake;
using InventoryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryLibrary.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<AGD> AGD { get; set; }
        public DbSet<Furniture> Furnitures { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ItemCondition> itemConditions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Stocktake> Stocktakes { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<HistoricalItem> HistoricalItems { get; set; }

        private string imagesPath = "";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account configuration
            modelBuilder.Entity<Account>()
                .ToTable("Accounts")
                .Property(a => a.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.InventoryItems)
                .WithOne(i => i.personInCharge)
                .HasForeignKey(i => i.PersonInChargeId)
                .OnDelete(DeleteBehavior.SetNull);

            // InventoryItem configuration
            modelBuilder.Entity<InventoryItem>()
                .ToTable("InventoryItems")
                .HasOne(i => i.Location)
                .WithMany()
                .HasForeignKey(i => i.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // HistoricalItem configuration
            modelBuilder.Entity<HistoricalItem>()
                .ToTable("HistoricalItems");

            // Stocktake configuration
            modelBuilder.Entity<Stocktake>(entity =>
            {
                entity.ToTable("Stocktakes");

                // Relacja one-to-many z InventoryItem
                entity.HasMany(s => s.ItemsToCheck)
                    .WithOne(i => i.Stocktake)
                    .HasForeignKey(i => i.StocktakeId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relacja many-to-many z Account (AuthorizedAccounts)
                entity.HasMany(s => s.AuthorizedAccounts)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "StocktakeAuthorizedAccounts",
                        j => j.HasOne<Account>()
                            .WithMany()
                            .HasForeignKey("AccountId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Stocktake>()
                            .WithMany()
                            .HasForeignKey("StocktakeId")
                            .OnDelete(DeleteBehavior.Cascade)
                    );

                // Konwersja listy CheckedItemIdList do formatu JSON
                entity.Property(s => s.CheckedItemIdList)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<int>()
                    )
                    .HasColumnType("nvarchar(max)");

                // Indeksy dla lepszej wydajności
                entity.HasIndex(s => s.Status);
                entity.HasIndex(s => s.StartDate);
                entity.HasIndex(s => s.EndDate);
            });

            
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

            modelBuilder.Entity<Setting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<ItemType>().HasData(
                new ItemType { Id = 1, TypeName = "NoType" },
                new ItemType { Id = 2, TypeName = "AGD" },
                new ItemType { Id = 3, TypeName = "Furniture" }
            );

            modelBuilder.Entity<ItemCondition>().HasData(
                new ItemCondition { Id = 1, ConditionName = "New" },
                new ItemCondition { Id = 2, ConditionName = "Good" },
                new ItemCondition { Id = 3, ConditionName = "Damaged" },
                new ItemCondition { Id = 4, ConditionName = "Lost" },
                new ItemCondition { Id = 5, ConditionName = "Disposed" }
            );

            modelBuilder.Entity<Account>().HasData(
                new Account 
                { 
                    Id = 1, 
                    Name = "Admin", 
                    Email = "", 
                    PasswordHash = "", 
                    Role = "Admin", 
                    IsAdmin = true, 
                    resetPasswordOnNextLogin = true 
                }
            );

            try
            {
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                    .Parent.Parent.Parent.Parent.FullName;
                imagesPath = Path.Combine(projectRoot, "Images");
            }
            catch (Exception)
            {
                // Ignore
            }

            modelBuilder.Entity<Setting>().HasData(
                new Setting { Id = 1, Key = "FileStoragePath", Value = $"{imagesPath}" },
                new Setting { Id = 2, Key = "MaxFileSize", Value = "10485760" },
                new Setting { Id = 3, Key = "CompanyName", Value = "My Company" },
                new Setting { Id = 4, Key = "ChosenDepartmentId", Value = "" }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentName = "", DepartmentLocation = "" }
            );
        }
    }
}